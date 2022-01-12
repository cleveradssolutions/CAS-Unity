#if UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace CAS.UEditor
{
    internal class CASPostprocessBuild
    {
        [PostProcessBuild()]
        public static void OnPostProcessBuild( BuildTarget buildTarget, string buildPath )
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            // Init Settings can be null
            var initSettings = CASEditorUtils.GetSettingsAsset( BuildTarget.iOS, false );
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create( BuildTarget.iOS, Audience.Mixed, true );

            string plistPath = Path.Combine( buildPath, "Info.plist" );
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile( plistPath );

            UpdateGADDelayMeasurement( plist, editorSettings );
            UpdateGADAppId( plist, initSettings, depManager );
            UpdateSKAdNetworksInfo( plist );
            UpdateLSApplicationQueriesSchames( plist );
            UpdateAppTransportSecuritySettings( plist );
            SetAttributionReportEndpoint( plist, editorSettings );
            SetDefaultUserTrackingDescription( plist, editorSettings );

            File.WriteAllText( plistPath, plist.WriteToString() );

            var project = OpenXCode( buildPath );
            string mainTargetGuid;
            string frameworkTargetGuid;
            GetTargetsGUID( project, out mainTargetGuid, out frameworkTargetGuid );

            var swiftVersion = project.GetBuildPropertyForAnyConfig( frameworkTargetGuid, "SWIFT_VERSION" );
            if (string.IsNullOrEmpty( swiftVersion ))
                project.SetBuildProperty( frameworkTargetGuid, "SWIFT_VERSION", "5.0" );
            project.SetBuildProperty( frameworkTargetGuid, "CLANG_ENABLE_MODULES", "YES" );

            CopyRawSettingsFile( buildPath, project, mainTargetGuid, initSettings );
            EnableSwiftForMainTarget( buildPath, project, mainTargetGuid );
            EmbedSwiftStandardLibraries( project, mainTargetGuid );
            SetExecutablePath( buildPath, project, mainTargetGuid, depManager );

            SaveXCode( project, buildPath );

            ApplyCrosspromoDynamicLinks( buildPath, mainTargetGuid, initSettings, depManager );

            Debug.Log( CASEditorUtils.logTag + "Postrocess Build done." );
        }

#if UNITY_2019_3_OR_NEWER || CASDeveloper
        [PostProcessBuild( 47 )]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        public static void FixPodFileBug( BuildTarget target, string buildPath )
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

        [PostProcessBuild( int.MaxValue )]
        public static void OnCocoaPodsReady( BuildTarget buildTarget, string buildPath )
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            var editorSettings = CASEditorSettings.Load();
            var needLocalizeUserTracking = IsNeedLocalizeUserTrackingDescription( editorSettings );
            var needEmbedDynamicLibraries = IsNeedEmbedDynamicLibraries();
            if (!needEmbedDynamicLibraries && !needLocalizeUserTracking)
                return;

            var project = OpenXCode( buildPath );
            string mainTargetGuid;
            string frameworkTargetGuid;
            GetTargetsGUID( project, out mainTargetGuid, out frameworkTargetGuid );
            if (needLocalizeUserTracking)
                LocalizeUserTrackingDescription( buildPath, project, mainTargetGuid, editorSettings );

            if (needEmbedDynamicLibraries)
            {
                var depManager = DependencyManager.Create( BuildTarget.iOS, Audience.Mixed, true );
                EmbedDynamicLibrariesIfNeeded( buildPath, project, mainTargetGuid, depManager );
            }

            SaveXCode( project, buildPath );
        }

        #region Utils
        private static string GetXCodeProjectPath( string buildPath )
        {
            return Path.Combine( buildPath, "Unity-iPhone.xcodeproj/project.pbxproj" );
        }

        private static PBXProject OpenXCode( string buildPath )
        {
            var project = new PBXProject();
            project.ReadFromString( File.ReadAllText( GetXCodeProjectPath( buildPath ) ) );
            return project;
        }

        private static void SaveXCode( PBXProject project, string buildPath )
        {
            File.WriteAllText( GetXCodeProjectPath( buildPath ), project.WriteToString() );
        }

        private static void GetTargetsGUID( PBXProject project, out string main, out string framework )
        {
#if UNITY_2019_3_OR_NEWER
            main = project.GetUnityMainTargetGuid();
            framework = project.GetUnityFrameworkTargetGuid();
#elif UNITY_2018_1_OR_NEWER
            main = project.TargetGuidByName( PBXProject.GetUnityTargetName() );
            framework = main;
#else
            main = project.TargetGuidByName( "Unity-iPhone" );
            framework = main;
#endif
        }
        #endregion

        #region Info PList
        private static void UpdateGADDelayMeasurement( PlistDocument plist, CASEditorSettings editorSettings )
        {
            plist.root.SetBoolean( "GADDelayAppMeasurementInit", editorSettings.delayAppMeasurementGADInit );
        }

        private static void UpdateGADAppId( PlistDocument plist, CASInitSettings initSettings, DependencyManager deps )
        {
            if (!initSettings)
                return;
            #region Read Admob App ID from CAS Settings
            bool admobAppIdRequired = deps == null;
            if (deps != null)
            {
                var admobDep = deps.Find( AdNetwork.GoogleAds );
                if (admobDep != null)
                    admobAppIdRequired = admobDep.IsInstalled();
            }

            string admobAppId = null;
            if (initSettings.managersCount > 0)
            {
                string settingsPath = CASEditorUtils.GetNativeSettingsPath( BuildTarget.iOS, initSettings.GetManagerId( 0 ) );
                if (File.Exists( settingsPath ))
                {
                    try
                    {
                        admobAppId = CASEditorUtils.GetAdmobAppIdFromJson( File.ReadAllText( settingsPath ) );
                    }
                    catch (Exception e)
                    {
                        if (!initSettings.IsTestAdMode() && admobAppIdRequired)
                            CASEditorUtils.StopBuildWithMessage( e.ToString(), BuildTarget.iOS );
                    }
                }
            }
            if (string.IsNullOrEmpty( admobAppId ) && initSettings.IsTestAdMode())
            {
                admobAppId = CASEditorUtils.iosAdmobSampleAppID;
            }

            #endregion

            if (!string.IsNullOrEmpty( admobAppId ))
                plist.root.SetString( "GADApplicationIdentifier", admobAppId );
        }

        private static void UpdateSKAdNetworksInfo( PlistDocument plist )
        {
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
        }

        private static void UpdateLSApplicationQueriesSchames( PlistDocument plist )
        {

            PlistElementArray schemesList;
            var applicationQueriesSchemesField = plist.root["LSApplicationQueriesSchemes"];
            if (applicationQueriesSchemesField == null)
                schemesList = plist.root.CreateArray( "LSApplicationQueriesSchemes" );
            else
                schemesList = applicationQueriesSchemesField.AsArray();
            var schemes = new string[] { "fb", "instagram", "tumblr", "twitter" };
            for (int i = 0; i < schemes.Length; i++)
            {
                var scheme = schemes[i];
                if (string.IsNullOrEmpty( scheme ))
                    continue;
                var exist = false;
                for (int findI = 0; findI < schemesList.values.Count; findI++)
                {
                    if (schemesList.values[findI].AsString() == scheme)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    schemesList.AddString( scheme );
            }
        }

        private static void UpdateAppTransportSecuritySettings( PlistDocument plist )
        {
            PlistElement atsRoot;
            plist.root.values.TryGetValue( "NSAppTransportSecurity", out atsRoot );

            if (atsRoot == null || atsRoot.GetType() != typeof( PlistElementDict ))
            {
                // Add the missing App Transport Security settings for publishers if needed. 
                Debug.Log( CASEditorUtils.logTag + "Adding App Transport Security settings..." );
                atsRoot = plist.root.CreateDict( "NSAppTransportSecurity" );
                atsRoot.AsDict().SetBoolean( "NSAllowsArbitraryLoads", true );
                return;
            }

            // Check if both NSAllowsArbitraryLoads and NSAllowsArbitraryLoadsInWebContent are present
            // and remove NSAllowsArbitraryLoadsInWebContent if both are present.
            var atsRootDict = atsRoot.AsDict().values;
            if (atsRootDict.ContainsKey( "NSAllowsArbitraryLoads" )
                && atsRootDict.ContainsKey( "NSAllowsArbitraryLoadsInWebContent" ))
            {
                Debug.Log( CASEditorUtils.logTag + "Removing NSAllowsArbitraryLoadsInWebContent" );
                atsRootDict.Remove( "NSAllowsArbitraryLoadsInWebContent" );
            }
        }

        private static void SetAttributionReportEndpoint( PlistDocument plist, CASEditorSettings settings )
        {
            if (!string.IsNullOrEmpty( settings.attributionReportEndpoint ))
                plist.root.SetString( "NSAdvertisingAttributionReportEndpoint", settings.attributionReportEndpoint );
        }

        private static void SetDefaultUserTrackingDescription( PlistDocument plist, CASEditorSettings settings )
        {
            if (settings.userTrackingUsageDescription.Length == 0)
                return;
            var description = settings.userTrackingUsageDescription[0].value;
            if (string.IsNullOrEmpty( description ))
                return;
            plist.root.SetString( "NSUserTrackingUsageDescription", description );
        }
        #endregion

        #region XCode project
        private static void EnableSwiftForMainTarget( string rootPath, PBXProject project, string target )
        {
#if ENABLE_SWIFT_MAIN_TARGET
            try
            {
                var compileSourcesPhase = project.GetSourcesBuildPhaseByTarget( target );
                var swiftEnableFile = project.FindFileGuidByProjectPath( CASEditorUtils.swiftEnableFileInXCode );
                if (swiftEnableFile == null)
                {
                    var fileName = "Libraries/CASEnableSwift.swift";
                    File.WriteAllText( Path.Combine( rootPath, fileName ), "import Foundation\n" );
                    swiftEnableFile = project.AddFile( fileName, fileName, PBXSourceTree.Source );
                }
                project.AddFileToBuildSection( target, compileSourcesPhase, swiftEnableFile );
            }
            catch (Exception e)
            {
                Debug.LogWarning( CASEditorUtils.logTag + "Enable Swift failed: " + e.ToString() );
            }
#endif
        }

        /// <summary>
        /// For Swift 5+ code that uses the standard libraries,
        /// the Swift Standard Libraries MUST be embedded for iOS < 12.2
        /// 
        /// Swift 5 introduced ABI stability, which allowed iOS to start bundling
        /// the standard libraries in the OS starting with iOS 12.2
        /// 
        /// Issue Reference: https://github.com/facebook/facebook-sdk-for-unity/issues/506
        /// </summary>
        private static void EmbedSwiftStandardLibraries( PBXProject project, string mainTargetGuid )
        {
            string embedStandardLib = "YES";
            try
            {
                if (new Version( PlayerSettings.iOS.targetOSVersionString ) < new Version( 12, 2 ))
                    embedStandardLib = "YES";
                else
                    embedStandardLib = "NO";
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            // This needs to be added the main target.
            // App Store may reject builds if added to UnityFramework (i.e. MoPub in FT).
            project.SetBuildProperty( mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", embedStandardLib );
        }

        private static void CopyRawSettingsFile( string rootPath, PBXProject project, string target, CASInitSettings casSettings )
        {
            if (!casSettings)
                return;

            var resourcesBuildPhase = project.GetResourcesBuildPhaseByTarget( target );
            for (int i = 0; i < casSettings.managersCount; i++)
            {
                string managerId = casSettings.GetManagerId( i );
                int managerIdLength = managerId.Length;
                string suffixChar = char.ToLower( managerId[managerIdLength - 1] ).ToString();
                string fileName = "cas_settings" + managerIdLength.ToString() + suffixChar + ".json";
                string pathInAssets = CASEditorUtils.GetNativeSettingsPath( BuildTarget.iOS, managerId );
                if (File.Exists( pathInAssets ))
                {
                    try
                    {
                        File.Copy( pathInAssets, Path.Combine( rootPath, fileName ), true );
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
        }

        private static void ApplyCrosspromoDynamicLinks( string buildPath, string targetGuid, CASInitSettings casSettings, DependencyManager deps )
        {
            if (!casSettings || casSettings.IsTestAdMode() || casSettings.managersCount == 0 || string.IsNullOrEmpty( casSettings.GetManagerId( 0 ) ))
                return;
            if (deps != null)
            {
                var crossPromoDependency = deps.FindCrossPromotion();
                if (crossPromoDependency != null && !crossPromoDependency.IsInstalled())
                    return;
            }
            try
            {
                var identifier = Application.identifier;
                var productName = identifier.Substring( identifier.LastIndexOf( "." ) + 1 );
                var projectPath = GetXCodeProjectPath( buildPath );
                var entitlements = new ProjectCapabilityManager( projectPath, productName + ".entitlements",
#if UNITY_2019_3_OR_NEWER
                    null, targetGuid );
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

        private static void AddSwiftSupport( string buildPath, PBXProject project, string targetGuid )
        {
            var swiftFileRelativePath = "Classes/CASSwiftEnable.swift";
            var swiftFilePath = Path.Combine( buildPath, swiftFileRelativePath );

            // Add Swift file
            if (File.Exists( swiftFilePath )) return;

            // Create a file to write to.
            using (var writer = File.CreateText( swiftFilePath ))
            {
                writer.WriteLine( "\nimport Foundation\n" );
                writer.WriteLine( "// This file ensures the project includes Swift support." );
                writer.WriteLine( "// It is automatically generated by the CAS Unity Plugin." );
                writer.Close();
            }

            var swiftFileGuid = project.AddFile( swiftFilePath, swiftFileRelativePath, PBXSourceTree.Source );
            project.AddFileToBuild( targetGuid, swiftFileGuid );
        }

        private static void SetExecutablePath( string buildPath, PBXProject project, string targetGuid, DependencyManager deps )
        {
#if !UNITY_2019_3_OR_NEWER
#if UNITY_2018_2_OR_NEWER
            string runpathSearchPaths = project.GetBuildPropertyForAnyConfig( targetGuid, "LD_RUNPATH_SEARCH_PATHS" );
#else
            string runpathSearchPaths = "$(inherited)";          
#endif
            if (string.IsNullOrEmpty( runpathSearchPaths ))
            {
                runpathSearchPaths = "";
            }
            else
            {
                // Check if runtime search paths already contains the required search paths for dynamic libraries.
                if (runpathSearchPaths.Contains( "@executable_path/Frameworks" ))
                    return;

                runpathSearchPaths += " ";
            }

            runpathSearchPaths += "@executable_path/Frameworks";
            project.SetBuildProperty( targetGuid, "LD_RUNPATH_SEARCH_PATHS", runpathSearchPaths );
#endif

            // Needed to build successfully on Xcode 12+,
            // as framework was build with latest Xcode but not as an xcframework

            //project.AddBuildProperty( targetGuid, "VALIDATE_WORKSPACE", "YES" );
        }

        private static bool IsNeedEmbedDynamicLibraries()
        {
#if UNITY_2019_3_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        private static void EmbedDynamicLibrariesIfNeeded( string buildPath, PBXProject project, string targetGuid, DependencyManager deps )
        {
            for (int i = 0; i < deps.networks.Length; i++)
            {
                var dynamicLibraryPath = deps.networks[i].embedFramework;
                if (string.IsNullOrEmpty( dynamicLibraryPath ))
                    continue;
                dynamicLibraryPath = Path.Combine( "Pods", dynamicLibraryPath );
                if (!Directory.Exists( Path.Combine( buildPath, dynamicLibraryPath ) ))
                    continue;

#if UNITY_2019_3_OR_NEWER
                var fileGuid = project.AddFile( dynamicLibraryPath, dynamicLibraryPath );
                project.AddFileToEmbedFrameworks( targetGuid, fileGuid );
#endif
            }
        }

        private static bool IsNeedLocalizeUserTrackingDescription( CASEditorSettings settings )
        {
            return settings.userTrackingUsageDescription.Length > 1;
        }

        private static void LocalizeUserTrackingDescription( string buildPath, PBXProject project, string targetGuid, CASEditorSettings settings )
        {
            const string LegacyResourcesDirectoryName = "Resources";
            const string CASResourcesDirectoryName = "CASUResources";

            if (settings.userTrackingUsageDescription.Length < 2)
                return;

            // Use the legacy resources directory name if the build is being appended (the "Resources" directory already exists if it is an incremental build).
            var resourcesDirectoryName = Directory.Exists( Path.Combine( buildPath, LegacyResourcesDirectoryName ) )
                ? LegacyResourcesDirectoryName : CASResourcesDirectoryName;
            var resourcesDirectoryPath = Path.Combine( buildPath, resourcesDirectoryName );

            for (int i = 0; i < settings.userTrackingUsageDescription.Length; i++)
            {
                var keyValue = settings.userTrackingUsageDescription[i];
                var description = keyValue.value;
                var localeCode = keyValue.key;
                if (string.IsNullOrEmpty( localeCode ))
                    continue;
                var localeSpecificDirectoryName = localeCode + ".lproj";
                var localeSpecificDirectoryPath = Path.Combine( resourcesDirectoryPath, localeSpecificDirectoryName );
                var infoPlistStringsFilePath = Path.Combine( localeSpecificDirectoryPath, "InfoPlist.strings" );

                // Check if localization has been disabled between builds, and remove them as needed.
                if (string.IsNullOrEmpty( description ))
                {
                    if (File.Exists( infoPlistStringsFilePath ))
                        File.Delete( infoPlistStringsFilePath );
                    continue;
                }

                // Create intermediate directories as needed.
                if (!Directory.Exists( resourcesDirectoryPath ))
                    Directory.CreateDirectory( resourcesDirectoryPath );
                if (!Directory.Exists( localeSpecificDirectoryPath ))
                    Directory.CreateDirectory( localeSpecificDirectoryPath );


                var localizedDescriptionLine = "\"NSUserTrackingUsageDescription\" = \"" + description + "\";\n";
                // File already exists, update it in case the value changed between builds.
                if (File.Exists( infoPlistStringsFilePath ))
                {
                    var output = new List<string>();
                    var lines = File.ReadAllLines( infoPlistStringsFilePath );
                    var keyUpdated = false;
                    foreach (var line in lines)
                    {
                        if (line.Contains( "NSUserTrackingUsageDescription" ))
                        {
                            output.Add( localizedDescriptionLine );
                            keyUpdated = true;
                        }
                        else
                        {
                            output.Add( line );
                        }
                    }

                    if (!keyUpdated)
                        output.Add( localizedDescriptionLine );

                    File.WriteAllText( infoPlistStringsFilePath, string.Join( "\n", output.ToArray() ) + "\n" );
                }
                // File doesn't exist, create one.
                else
                {
                    File.WriteAllText( infoPlistStringsFilePath, "/* Localized versions of Info.plist keys - Generated by CAS plugin */\n" + localizedDescriptionLine );
                }

                var guid = project.AddFolderReference( localeSpecificDirectoryPath, Path.Combine( resourcesDirectoryName, localeSpecificDirectoryName ) );
                project.AddFileToBuild( targetGuid, guid );
            }
        }
        #endregion
    }
}
#endif