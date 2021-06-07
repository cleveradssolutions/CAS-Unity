#if UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace CAS.UEditor
{
    internal class CASPostprocessBuild
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild( BuildTarget buildTarget, string path )
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            var casSettings = CASEditorUtils.GetSettingsAsset( BuildTarget.iOS );

            string plistPath = Path.Combine( path, "Info.plist" );
            ConfigureInfoPlist( plistPath, casSettings );

            var projectPath = Path.Combine( path, "Unity-iPhone.xcodeproj/project.pbxproj" );
            var project = ConfigureXCodeProject( path, projectPath, casSettings );
            ApplyCrosspromoDynamicLinks( projectPath, project, casSettings );
            Debug.Log( CASEditorUtils.logTag + "Postrocess Build done." );
        }

#if UNITY_2019_3_OR_NEWER
        [PostProcessBuild( 47 )]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        private static void FixPodFileBug( BuildTarget target, string buildPath )
        {
            if (target != BuildTarget.iOS)
                return;
            var podPath = buildPath + "/Podfile";
            if (!File.Exists( podPath ))
            {
                Debug.LogError( CASEditorUtils.logTag + "Podfile not found.\n" +
                    "Please add `target 'Unity-iPhone' do end` to the Podfile in root folder of XCode project and call `pod install --no-repo-update`" );
                return;
            }
            var content = File.ReadAllText( podPath );
            if (content.Contains( "'Unity-iPhone'" ))
                return;
            using (StreamWriter sw = File.AppendText( podPath ))
            {
                sw.WriteLine();
                sw.WriteLine( "target 'Unity-iPhone' do" );
                sw.WriteLine( "end" );
            }
        }
#endif

        private static void ConfigureInfoPlist( string plistPath, CASInitSettings casSettings )
        {
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile( plistPath );

            #region Read Admob App ID from CAS Settings
            string admobAppId = null;
            if (casSettings.testAdMode)
            {
                admobAppId = CASEditorUtils.iosAdmobSampleAppID;
            }
            else if (casSettings.managersCount > 0)
            {
                string settingsPath = CASEditorUtils.GetNativeSettingsPath( BuildTarget.iOS, casSettings.GetManagerId( 0 ) );
                if (File.Exists( settingsPath ))
                {
                    try
                    {
                        admobAppId = CASEditorUtils.GetAdmobAppIdFromJson( File.ReadAllText( settingsPath ) );
                    }
                    catch (Exception e)
                    {
                        CASEditorUtils.StopBuildWithMessage( e.ToString(), BuildTarget.iOS );
                    }
                }
            }

            #endregion

            if (admobAppId != null)
                plist.root.SetString( "GADApplicationIdentifier", admobAppId );
            plist.root.SetBoolean( "GADDelayAppMeasurementInit", true );

            #region Write SKAdNetworks
            var templateFile = CASEditorUtils.GetTemplatePath( CASEditorUtils.iosSKAdNetworksTemplateFile );
            if (string.IsNullOrEmpty( templateFile ))
            {
                Debug.LogError( CASEditorUtils.logTag + "Not found SKAdNetworkItems. Try reimport CAS package." );
            }
            else
            {
                var networksLines = File.ReadAllLines( templateFile );

                PlistElementArray adNetworkItems;
                var adNetworkItemsField = plist.root["SKAdNetworkItems"];
                if (adNetworkItemsField == null)
                    adNetworkItems = plist.root.CreateArray( "SKAdNetworkItems" );
                else
                    adNetworkItems = adNetworkItemsField.AsArray();

                for (int i = 0; i < networksLines.Length; i++)
                {
                    if (!string.IsNullOrEmpty( networksLines[i] ))
                    {
                        var dict = adNetworkItems.AddDict();
                        dict.SetString( "SKAdNetworkIdentifier", networksLines[i] );
                    }
                }
            }
            #endregion

            #region Write LSApplicationQueriesSchemes
            PlistElementArray applicationQueriesSchemes;
            var applicationQueriesSchemesField = plist.root["LSApplicationQueriesSchemes"];
            if (applicationQueriesSchemesField == null)
                applicationQueriesSchemes = plist.root.CreateArray( "LSApplicationQueriesSchemes" );
            else
                applicationQueriesSchemes = applicationQueriesSchemesField.AsArray();
            foreach (var scheme in new[] { "fb", "instagram", "tumblr", "twitter" })
                if (applicationQueriesSchemes.values.Find( x => x.AsString() == scheme ) == null)
                    applicationQueriesSchemes.AddString( scheme );
            #endregion

            File.WriteAllText( plistPath, plist.WriteToString() );
        }

        private static PBXProject ConfigureXCodeProject( string rootPath, string projectPath, CASInitSettings casSettings )
        {
            var project = new PBXProject();
            project.ReadFromString( File.ReadAllText( projectPath ) );
#if UNITY_2019_3_OR_NEWER
            var target = project.GetUnityMainTargetGuid();
#else
            var target = project.TargetGuidByName( PBXProject.GetUnityTargetName() );
#endif
            //project.SetBuildProperty( target, "ENABLE_BITCODE", "No" );
            project.AddBuildProperty( target, "OTHER_LDFLAGS", "-ObjC" );
            //project.AddBuildProperty( target, "OTHER_LDFLAGS", "-lxml2 -ObjC -fobjc-arc" );
            //project.AddBuildProperty( target, "CLANG_ENABLE_MODULES", "YES" ); // InMobi required
            project.SetBuildProperty( target, "SWIFT_VERSION", "5.0" );


            var resourcesBuildPhase = project.GetResourcesBuildPhaseByTarget( target );
            for (int i = 0; i < casSettings.managersCount; i++)
            {
                string managerId = casSettings.GetManagerId( i );
                int managerIdLength = managerId.Length;
                string suffix = managerIdLength.ToString() + managerId[managerIdLength - 1] + ".json";
                string fileName = "cas_settings" + suffix;
                string pathInAssets = CASEditorUtils.iosResSettingsPath + suffix;
                if (File.Exists( pathInAssets ))
                {
                    try
                    {
                        File.Copy( pathInAssets, Path.Combine( rootPath, fileName ) );
                        var fileGuid = project.AddFile( fileName, fileName, PBXSourceTree.Source );
                        project.AddFileToBuildSection( target, resourcesBuildPhase, fileGuid );
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning( CASEditorUtils.logTag + "Copy Raw File To XCode Project failed: " + e.ToString() );
                    }
                }
                else
                {
                    Debug.Log( CASEditorUtils.logTag + "Not found Raw file: " + pathInAssets );
                }
            }

            File.WriteAllText( projectPath, project.WriteToString() );
            return project;
        }

        private static void ApplyCrosspromoDynamicLinks( string projectPath, PBXProject project, CASInitSettings casSettings )
        {
            if (casSettings.testAdMode || casSettings.managersCount == 0
                || string.IsNullOrEmpty( casSettings.GetManagerId( 0 ) ))
                return;
            var depManager = DependencyManager.Create( BuildTarget.iOS, Audience.Mixed, false );
            if (depManager != null)
            {
                var crossPromoDependency = depManager.FindCrossPromotion();
                if (crossPromoDependency != null && !crossPromoDependency.isInstalled())
                    return;
            }
            try
            {
                var identifier = Application.identifier;
                var productName = identifier.Substring( identifier.LastIndexOf( "." ) + 1 );

                var entitlements = new ProjectCapabilityManager( projectPath, productName + ".entitlements",
#if UNITY_2019_3_OR_NEWER
                    null, project.GetUnityMainTargetGuid() );
#else
                    PBXProject.GetUnityTargetName() );
#endif
                string link = "applinks:psvios" + casSettings.GetManagerId( 0 ) + ".page.link";
                entitlements.AddAssociatedDomains( new[] { link } );
                entitlements.WriteToFile();
                Debug.Log( CASEditorUtils.logTag + "Apply application shame: " + link );
            }
            catch (Exception e)
            {
                Debug.LogError( CASEditorUtils.logTag + "Dynamic link creation fail: " + e.ToString() );
            }
        }
    }
}
#endif