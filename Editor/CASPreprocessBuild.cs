#if UNITY_ANDROID || UNITY_IOS || CASDeveloper
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Networking;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace CAS.UEditor
{
#if UNITY_2018_1_OR_NEWER
    internal class CASPreprocessBuild : IPreprocessBuildWithReport
#else
    public class CASPreprocessBuild : IPreprocessBuild
#endif
    {
        private const string casTitle = "CAS Preprocess Build";
        #region IPreprocessBuild
        public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild( BuildReport report )
        {
            BuildTarget target = report.summary.platform;
#else
        public void OnPreprocessBuild( BuildTarget target, string path )
        {
#endif
            try
            {
                ValidateIntegration( target );
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                throw e;
            }
        }
        #endregion


        public static void ValidateIntegration( BuildTarget target )
        {
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
                return;

            var settings = CASEditorUtils.GetSettingsAsset( target );
            if (!settings)
                CASEditorUtils.StopBuildWithMessage( "Settings not found. Please use menu Assets/CleverAdsSolutions/Settings " +
                    "for create and set settings to build.", target );

            string admobAppId;

            if (settings.testAdMode)
            {
                const string message = "CAS Mediation work in Test Ad mode.";
                if (EditorUtility.DisplayDialog( casTitle, message, "Cancel build", "Continue" ))
                    CASEditorUtils.StopBuildWithMessage( "Cancel build with Test Ad mode", target );

                if (target == BuildTarget.Android)
                {
                    admobAppId = CASEditorUtils.androidAdmobSampleAppID;
                }
                else
                {
                    admobAppId = CASEditorUtils.iosAdmobSampleAppID;
                    try
                    {
                        if (File.Exists( CASEditorUtils.iosResSettingsPath ))
                            File.Delete( CASEditorUtils.iosResSettingsPath );
                    }
                    catch (Exception e)
                    {
                        Debug.LogException( e );
                    }
                }
            }
            else
            {
                if (settings.managerIds.Length == 0)
                {
                    CASEditorUtils.OpenSettingsWindow( target );
                    StopBuildIDNotFound( target );
                }

                if (string.IsNullOrEmpty( settings.managerIds[0] ))
                {
                    CASEditorUtils.OpenSettingsWindow( target );
                    StopBuildIDNotFound( target );
                }

                admobAppId = DownloadRemoteSettings( settings.managerIds[0], "US", target );
            }

            if (string.IsNullOrEmpty( admobAppId ))
                CASEditorUtils.StopBuildWithMessage( "CAS server provides wrong settings for managerID " + settings.managerIds[0] +
                    ". Please try using a different identifier in the first place or contact support.", target );

            if (admobAppId.IndexOf( '~' ) < 0)
                CASEditorUtils.StopBuildWithMessage( "CAS server provides invalid Admob App Id not match pattern ca-app-pub-0000000000000000~0000000000. " +
                    "Please try using a different identifier in the first place or contact support.", target );

            if (target == BuildTarget.Android)
            {
                EditorUtility.DisplayProgressBar( casTitle, "Validate CAS Android Build Settings", 0.8f );
                CopyTemplateIfNeedToAndroidLib(
                    CASEditorUtils.androidLibPropTemplateFile, CASEditorUtils.androidLibPropertiesPath );

                SetAdmobAppIdToAndroidManifest( admobAppId, false );

                if (File.Exists( CASEditorUtils.mainGradlePath ))
                    ConfigurateGradleSettings();
                else
                    Debug.LogWarning( CASEditorUtils.logTag + "We recomended use Gradle Build system. " +
                        "Enable PlayerSettings> Publishing Settings> Custom Gradle Template" );

                if (PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel16)
                {
                    const string message = "CAS required minimum API level 17.";
                    if (EditorUtility.DisplayDialog( "CAS Preprocess Build", message, "Cancel build", "Set API 17" ))
                        CASEditorUtils.StopBuildWithMessage( message, BuildTarget.NoTarget );

                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel17;
                }
            }
            else if (target == BuildTarget.iOS)
            {
                EditorUtility.DisplayProgressBar( casTitle, "Validate CAS iOS Build Settings", 0.8f );
                if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK)
                {
                    const string message = "To use CAS on iOS Simulator, you need apply Scripting Define Symbols: TARGET_OS_SIMULATOR.";
                    if (EditorUtility.DisplayDialog( "CAS Preprocess Build", message, "Cancel build", "Continue" ))
                        CASEditorUtils.StopBuildWithMessage( message, BuildTarget.NoTarget );
                }

                if (CASEditorUtils.IsDependencyFileExists( CASEditorUtils.promoTemplateDependency, BuildTarget.iOS )
                    && !CASEditorUtils.IsFirebaseServiceExist( "dynamic" ))
                {
                    const string message = "CAS Cross-promotion uses deep links to track conversions. Please add Firebase Deep Link dependency to the project.";
                    if (EditorUtility.DisplayDialog( "CAS Preprocess Build", message, "Cancel build", "Continue" ))
                        CASEditorUtils.StopBuildWithMessage( message, BuildTarget.NoTarget );
                }

                if (!File.Exists( CASEditorUtils.iosResSettingsPath ))
                {
                    var appIdSettings = new CASEditorUtils.AdmobAppIdData();
                    appIdSettings.admob_app_id = admobAppId;
                    File.WriteAllText( CASEditorUtils.iosResSettingsPath, JsonUtility.ToJson( appIdSettings ) );
                    AssetDatabase.ImportAsset( CASEditorUtils.iosResSettingsPath );
                }
            }

            EditorUtility.DisplayProgressBar( casTitle, "Validate CAS Dependencies", 0.9f );
            string activeDependencyError = null;
            if (settings.audienceTagged == Audience.Children)
            {
                if (!CASEditorUtils.IsDependencyFileExists( CASEditorUtils.generalTemplateDependency, target ))
                    activeDependencyError = "Ad Children Audience required CAS General Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for enable it.";

                if (CASEditorUtils.IsDependencyFileExists( CASEditorUtils.teenTemplateDependency, target ))
                    activeDependencyError = "Ad Children Audience not allowed active CAS Teen Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for disable it.";
            }
            else
            {
                if (!CASEditorUtils.IsDependencyFileExists( CASEditorUtils.teenTemplateDependency, target ))
                    activeDependencyError = "Ad " + settings.audienceTagged.ToString() +
                        " Audience required CAS Teen Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for enable it.";

                if (CASEditorUtils.IsDependencyFileExists( CASEditorUtils.generalTemplateDependency, target ))
                    activeDependencyError = "Ad " + settings.audienceTagged.ToString() +
                        " Audience not allowed active CAS General Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for disable it.";
            }

            if (!string.IsNullOrEmpty( activeDependencyError ))
            {
                if (EditorUtility.DisplayDialog( casTitle, activeDependencyError,
                        "Cancel build", "Continue" ))
                    CASEditorUtils.StopBuildWithMessage( activeDependencyError, target );
            }

            if (settings.bannerSize == AdSize.Banner && CASEditorUtils.IsPortraitOrientation())
            {
                if (EditorUtility.DisplayDialog( "CAS Preprocess Build",
                        "For portrait applications, we recommend using the adaptive banner size." +
                        "This will allow you to get more expensive advertising.",
                        "Set Adaptive size",
                        "Continue" ))
                    CASEditorUtils.StopBuildWithMessage( "Cancel build for change Banner size", target );
            }
            Debug.Log( CASEditorUtils.logTag + "Preprocess Build done." );
            EditorUtility.DisplayProgressBar( "Hold on", "Prepare components...", 0.95f );
        }

        private static string ConfigurateGradleSettings()
        {
            //const string enabledAndroidX = "\"android.useAndroidX\", true";
            //const string enabledResolveInGradle = "// Android Resolver Dependencies";

            //string gradle = File.ReadAllText( CASEditorUtils.mainGradlePath );

            //if (gradle.Contains( enabledResolveInGradle ) && !gradle.Contains( enabledAndroidX ))
            //{
            //    StopBuildWithMessage( "For successful build need enable Jetfier by Android Resolver Settings and do Android Resolve again. " +
            //        "Project should use Gradle tools version 3.4+." );
            //}

            const string multidexObsoleted = "implementation 'com.android.support:multidex:1.0.3'";
            const string multidexImplementation = "implementation 'androidx.multidex:multidex:2.0.1'";

            const string multidexConfig = "multiDexEnabled true";

            const string dependencies = "implementation fileTree";
            const string defaultConfig = "defaultConfig";

            const string sourceCompatibility = "sourceCompatibility JavaVersion.VERSION_1_8";
            const string targetCompatibility = "targetCompatibility JavaVersion.VERSION_1_8";

            const string excludeOption = "exclude 'META-INF/proguard/androidx-annotations.pro'";


            var gradle = new List<string>( File.ReadAllLines( CASEditorUtils.mainGradlePath ) );
            // Find Dependency
            int line = 0;
            while (line < gradle.Count && !gradle[line++].Contains( dependencies )) { }

            if (line >= gradle.Count)
                CASEditorUtils.StopBuildWithMessage( "Not found Dependencies scope in Gradle template. " +
                    "Please try delete and create new mainTemplate.gradle", BuildTarget.NoTarget );

            // chek exist
            bool multidexExist = false;
            while (line < gradle.Count && !gradle[line].Contains( "}" ))
            {
                if (gradle[line].Contains( multidexObsoleted ))
                {
                    gradle[line] = "    " + multidexImplementation;
                    Debug.Log( CASEditorUtils.logTag + "Replace dependency " + multidexObsoleted +
                        " to " + multidexImplementation );
                    multidexExist = true;
                    break;
                }
                if (gradle[line].Contains( multidexImplementation ))
                {
                    multidexExist = true;
                    break;
                }
                line++;
            }

            // write dependency
            if (!multidexExist)
            {
                gradle.Insert( line, "    " + multidexImplementation );
                Debug.Log( CASEditorUtils.logTag + "Append dependency " + multidexImplementation );
                line++;
            }

            var sourceCompatibilityExist = false;
            var packagingOptExist = false;

            // Find Default Config
            while (line < gradle.Count && !gradle[line].Contains( defaultConfig ))
            {
                if (!sourceCompatibilityExist && gradle[line].Contains( sourceCompatibility ))
                    sourceCompatibilityExist = true;
                if (!sourceCompatibilityExist && gradle[line].Contains( targetCompatibility ))
                    sourceCompatibilityExist = true;
                if (!packagingOptExist && gradle[line].Contains( excludeOption ))
                    packagingOptExist = true;
                ++line;
            }

            if (line >= gradle.Count)
                CASEditorUtils.StopBuildWithMessage( "Not found Default Config scope in Gradle template. " +
                    "Please try delete and create new mainTemplate.gradle", BuildTarget.NoTarget );

            if (!sourceCompatibilityExist)
            {
                gradle.InsertRange( line, new[] {
                    "	compileOptions {",
                    "        " + sourceCompatibility,
                    "        " + targetCompatibility,
                    "	}",
                    ""
                } );
                Debug.Log( CASEditorUtils.logTag + "Append Compile options to use Java Version 1.8." );
                line += 5;
            }

            if (!packagingOptExist)
            {
                gradle.InsertRange( line, new[] {
                    "	packagingOptions {",
                    "        " + excludeOption,
                    "	}",
                    ""
                } );
                Debug.Log( CASEditorUtils.logTag + "Append Packaging options to exclude duplicate files." );
                line += 4;
            }

            // Find multidexEnable
            var multidexEnabled = false;
            while (line < gradle.Count && !gradle[line].Contains( "}" ))
            {
                if (gradle[line].Contains( multidexConfig ))
                {
                    multidexEnabled = true;
                    break;
                }
                line++;
            }

            if (!multidexEnabled)
            {
                gradle.Insert( line, "        " + multidexConfig );
                Debug.Log( CASEditorUtils.logTag + "Enable Multidex in Default Config" );
            }

            File.WriteAllLines( CASEditorUtils.mainGradlePath, gradle.ToArray() );
            AssetDatabase.ImportAsset( CASEditorUtils.mainGradlePath );
            return null;
        }

        private static void CopyTemplateIfNeedToAndroidLib( string template, string path )
        {
            if (!File.Exists( path ))
            {
                if (!AssetDatabase.IsValidFolder( CASEditorUtils.androidLibFolderPath ))
                {
                    Directory.CreateDirectory( CASEditorUtils.androidLibFolderPath );
                    AssetDatabase.ImportAsset( CASEditorUtils.androidLibFolderPath );
                }
                var templateFile = CASEditorUtils.GetTemplatePath( template );
                if (templateFile == null || !CASEditorUtils.TryCopyFile( templateFile, path ))
                    CASEditorUtils.StopBuildWithMessage( "Build failed", BuildTarget.NoTarget );
            }
        }

        private static void SetAdmobAppIdToAndroidManifest( string admobAppId, bool newFile )
        {
            const string metaAdmobApplicationID = "com.google.android.gms.ads.APPLICATION_ID";
            XNamespace ns = "http://schemas.android.com/apk/res/android";

            string manifestPath = Path.GetFullPath( CASEditorUtils.androidLibManifestPath );

            CopyTemplateIfNeedToAndroidLib(
                    CASEditorUtils.androidLibManifestTemplateFile, CASEditorUtils.androidLibManifestPath );

            try
            {
                XDocument manifest = XDocument.Load( manifestPath );
                XElement elemManifest = manifest.Element( "manifest" );
                XElement elemApplication = elemManifest.Element( "application" );
                IEnumerable<XElement> metas = elemApplication.Descendants()
                    .Where( elem => elem.Name.LocalName.Equals( "meta-data" ) );

                foreach (XElement elem in metas)
                {
                    IEnumerable<XAttribute> attrs = elem.Attributes();
                    foreach (XAttribute attr in attrs)
                    {
                        if (attr.Name.Namespace.Equals( ns )
                                && attr.Name.LocalName.Equals( "name" ) && attr.Value.Equals( metaAdmobApplicationID ))
                        {
                            elem.SetAttributeValue( ns + "value", admobAppId );
                            elemManifest.Save( manifestPath );
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            if (newFile)
                CASEditorUtils.StopBuildWithMessage(
                    "AndroidManifest.xml is not valid. Try re-importing the plugin.", BuildTarget.Android );

            Debug.LogWarning( CASEditorUtils.logTag + "AndroidManifest.xml is not valid. Created new file by template." );
            AssetDatabase.DeleteAsset( CASEditorUtils.androidLibManifestPath );
            SetAdmobAppIdToAndroidManifest( admobAppId, true );
        }

        public static string DownloadRemoteSettings( string managerID, string country, BuildTarget platform )
        {
            string title = "Download CAS settings for " + platform.ToString();
            string url = CASEditorUtils.BuildRemoteUrl( managerID, country, platform );

            using (var loader = UnityWebRequest.Get( url ))
            {
                loader.SendWebRequest();
                while (!loader.isDone)
                {
                    if (EditorUtility.DisplayCancelableProgressBar( casTitle, title,
                        Mathf.Repeat( ( float )EditorApplication.timeSinceStartup, 1.0f ) ))
                    {
                        loader.Dispose();
                        return null;
                    }
                }
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar( casTitle, "Write CAS settings", 0.7f );
                if (string.IsNullOrEmpty( loader.error ))
                {
                    string content = loader.downloadHandler.text.Trim();
                    if (string.IsNullOrEmpty( content ))
                    {
                        CASEditorUtils.StopBuildWithMessage( "Server have no settings for " + managerID +
                            " Please try using a different identifier in the first place or contact support.", platform );
                    }
                    else
                    {
                        if (platform == BuildTarget.Android)
                            CASEditorUtils.WriteToFile( content, CASEditorUtils.androidResSettingsPath );
                        else
                            CASEditorUtils.WriteToFile( content, CASEditorUtils.iosResSettingsPath );

                        return CASEditorUtils.GetAdmobAppIdFromJson( content );
                    }
                }
                else
                {
                    Debug.LogError( CASEditorUtils.logTag + " Server connect rrror " + loader.responseCode + ": " + loader.error );
                }
            }
            return null;
        }

        private static void StopBuildIDNotFound( BuildTarget target )
        {
            CASEditorUtils.StopBuildWithMessage( "Settings not found manager ids for " + target.ToString() +
                        " platform. For a successful build, you need to specify at least one ID" +
                        " that you use in the project. To test integration, you can use test mode.", target );
        }
    }
}
#endif