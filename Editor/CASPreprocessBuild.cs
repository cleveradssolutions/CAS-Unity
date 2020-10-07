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
using System.Reflection;
using Utils = CAS.UEditor.CASEditorUtils;

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
        public int callbackOrder { get { return -25000; } }

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

            var settings = Utils.GetSettingsAsset( target );
            if (!settings)
                Utils.StopBuildWithMessage( "Settings not found. Please use menu Assets/CleverAdsSolutions/Settings " +
                    "for create and set settings to build.", target );

            string admobAppId;
            if (settings.testAdMode)
            {
                DialogOrCancel( "CAS Mediation work in Test Ad mode.", BuildTarget.NoTarget );

                if (target == BuildTarget.Android)
                {
                    admobAppId = Utils.androidAdmobSampleAppID;
                }
                else
                {
                    admobAppId = Utils.iosAdmobSampleAppID;
                    try
                    {
                        if (File.Exists( Utils.iosResSettingsPath ))
                            File.Delete( Utils.iosResSettingsPath );
                    }
                    catch (Exception e)
                    {
                        Debug.LogException( e );
                    }
                }
            }
            else
            {
                if (settings.managerIds == null || settings.managerIds.Length == 0 || string.IsNullOrEmpty( settings.managerIds[0] ))
                    StopBuildIDNotFound( target );

                admobAppId = DownloadRemoteSettings( settings.managerIds[0], "US", target );
            }

            if (string.IsNullOrEmpty( admobAppId ))
                Utils.StopBuildWithMessage( "CAS server provides wrong settings for managerID " + settings.managerIds[0] +
                    ". Please try using a different identifier in the first place or contact support.", target );

            if (admobAppId.IndexOf( '~' ) < 0)
                Utils.StopBuildWithMessage( "CAS server provides invalid Admob App Id not match pattern ca-app-pub-0000000000000000~0000000000. " +
                    "Please try using a different identifier in the first place or contact support.", target );

            if (target == BuildTarget.Android)
            {
                EditorUtility.DisplayProgressBar( casTitle, "Validate CAS Android Build Settings", 0.8f );

                const string deprecatedPluginPath = "Assets/Plugins/CAS";
                if (AssetDatabase.IsValidFolder( deprecatedPluginPath ))
                {
                    AssetDatabase.DeleteAsset( deprecatedPluginPath );
                    Debug.Log( "Removed deprecated plugin: " + deprecatedPluginPath );
                }

                CopyTemplateIfNeedToAndroidLib(
                    Utils.androidLibPropTemplateFile, Utils.androidLibPropertiesPath );

                SetAdmobAppIdToAndroidManifest( admobAppId, false );

                ConfigurateGradleSettings();

                if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel19)
                {
                    DialogOrCancel( "CAS required minimum SDK API level 19 (KitKat).", BuildTarget.NoTarget, "Set API 19" );
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
                }
            }
            else if (target == BuildTarget.iOS)
            {
                EditorUtility.DisplayProgressBar( casTitle, "Validate CAS iOS Build Settings", 0.8f );
                if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK)
                    DialogOrCancel( "To use CAS on iOS Simulator, you need apply Scripting Define Symbols: TARGET_OS_SIMULATOR.", BuildTarget.NoTarget );

                if (Utils.IsDependencyFileExists( Utils.promoTemplateDependency, BuildTarget.iOS )
                    && !Utils.IsFirebaseServiceExist( "dynamic" ))
                    DialogOrCancel( "CAS Cross-promotion uses deep links to track conversions. Please add Firebase Deep Link dependency to the project.", BuildTarget.NoTarget );

                if (!File.Exists( Utils.iosResSettingsPath ))
                {
                    var appIdSettings = new Utils.AdmobAppIdData();
                    appIdSettings.admob_app_id = admobAppId;
                    File.WriteAllText( Utils.iosResSettingsPath, JsonUtility.ToJson( appIdSettings ) );
                    AssetDatabase.ImportAsset( Utils.iosResSettingsPath );
                }
            }

            EditorUtility.DisplayProgressBar( casTitle, "Validate CAS Dependencies", 0.9f );
            bool allowReimportDeps = PlayerPrefs.GetInt( Utils.editorReimportDepsOnBuildPrefs, 1 ) == 1;
            string activeDependencyError = null;
            if (settings.audienceTagged == Audience.Children)
            {
                if (!Utils.IsDependencyFileExists( Utils.generalTemplateDependency, target ))
                    activeDependencyError = "Ad Children Audience required CAS General Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for enable it.";
                else if (allowReimportDeps)
                    Utils.TryActivateDependencies( Utils.generalTemplateDependency, target );

                if (Utils.IsDependencyFileExists( Utils.teenTemplateDependency, target ))
                    activeDependencyError = "Ad Children Audience not allowed active CAS Teen Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for disable it.";
            }
            else
            {
                if (!Utils.IsDependencyFileExists( Utils.teenTemplateDependency, target ))
                    activeDependencyError = "Ad " + settings.audienceTagged.ToString() +
                        " Audience required CAS Teen Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for enable it.";
                else if (allowReimportDeps)
                    Utils.TryActivateDependencies( Utils.teenTemplateDependency, target );

                if (Utils.IsDependencyFileExists( Utils.generalTemplateDependency, target ))
                    activeDependencyError = "Ad " + settings.audienceTagged.ToString() +
                        " Audience not allowed active CAS General Depencency. " +
                        "Please use menu Assets/CleverAdsSolutions/Settings for disable it.";
            }

            if (!string.IsNullOrEmpty( activeDependencyError ))
                DialogOrCancel( activeDependencyError, target );

            if (allowReimportDeps && Utils.IsDependencyFileExists( Utils.promoTemplateDependency, target ))
                Utils.TryActivateDependencies( Utils.promoTemplateDependency, target );

            if (settings.bannerSize == AdSize.Banner && Utils.IsPortraitOrientation())
            {
                DialogOrCancel( "For portrait applications, we recommend using the adaptive banner size." +
                        "This will allow you to get more expensive advertising.", target );
            }

            if (target == BuildTarget.Android && allowReimportDeps)
            {
                bool success = true;
                try
                {
                    var resolverType = Type.GetType( "GooglePlayServices.PlayServicesResolver, Google.JarResolver", false );
                    if (resolverType != null)
                    {
                        var autoResolve = ( bool )resolverType.GetProperty( "AutomaticResolutionEnabled",
                            BindingFlags.Public | BindingFlags.Static )
                            .GetValue( null );
                        if (!autoResolve)
                        {
                            success = ( bool )resolverType.GetMethod( "ResolveSync", BindingFlags.Public | BindingFlags.Static, null,
                                new[] { typeof( bool ) }, null )
                                .Invoke( null, new object[] { true } );
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning( Utils.logTag + "GooglePlayServices.PlayServicesResolver error: " + e.Message );
                }
                if (!success)
                    Utils.StopBuildWithMessage( "Cancel build: Resolution Failed. See the log for details.", BuildTarget.NoTarget );
            }

            Debug.Log( Utils.logTag + "Preprocess Build done." );
            EditorUtility.DisplayProgressBar( "Hold on", "Prepare components...", 0.95f );
        }

        private static void ConfigurateGradleSettings()
        {
            //const string enabledAndroidX = "\"android.useAndroidX\", true";
            //const string enabledResolveInGradle = "// Android Resolver Dependencies";

            //string gradle = File.ReadAllText( Utils.mainGradlePath );

            //if (gradle.Contains( enabledResolveInGradle ) && !gradle.Contains( enabledAndroidX ))
            //{
            //    StopBuildWithMessage( "For successful build need enable Jetfier by Android Resolver Settings and do Android Resolve again. " +
            //        "Project should use Gradle tools version 3.4+." );
            //}

            const string multidexObsoleted = "implementation 'com.android.support:multidex:1.0.3'";
            const string multidexImplementation = "implementation 'androidx.multidex:multidex:2.0.1'";

            const string multidexConfig = "multiDexEnabled true";

            const string dependencies = "implementation ";
            const string defaultConfig = "defaultConfig";

            const string sourceCompatibility = "sourceCompatibility JavaVersion.VERSION_1_8";
            const string targetCompatibility = "targetCompatibility JavaVersion.VERSION_1_8";

            const string excludeOption = "exclude 'META-INF/proguard/androidx-annotations.pro'";

#if UNITY_2019_3_OR_NEWER
            if (File.Exists( Utils.projectGradlePath ))
            {
                try
                {
                    var projectGradle = new List<string>( File.ReadAllLines( Utils.projectGradlePath ) );
                    projectGradle = UpdateGradlePluginVersion( projectGradle );
                    File.WriteAllLines( Utils.projectGradlePath, projectGradle.ToArray() );
                    AssetDatabase.ImportAsset( Utils.projectGradlePath );
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
            }
            else if (RequestUpdateGradleVersion( "" ))
            {
                Utils.StopBuildWithMessage(
                        "Build failed because CAS could not update Gradle plugin version in Unity 2019.3 without " +
                        "Custom Base Gradle Template. Please enable 'Custom Base Gradle Template' found under " +
                        "'Player Settings > Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
            }

            const string gradlePath = Utils.launcherGradlePath;
            if (!File.Exists( gradlePath ))
                Utils.StopBuildWithMessage(
                    "Build failed because CAS could not enable MultiDEX in Unity 2019.3 without " +
                    "Custom Launcher Gradle Template. Please enable 'Custom Launcher Gradle Template' found under " +
                    "'Player Settings > Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
            List<string> gradle = new List<string>( File.ReadAllLines( gradlePath ) );
#else
            const string gradlePath = Utils.mainGradlePath;
            if (!File.Exists( gradlePath ))
            {
                Debug.LogWarning( Utils.logTag + "We recomended use Gradle Build system. " +
                        "Enable PlayerSettings> Publishing Settings> Custom Gradle Template" );
                return;
            }

            List<string> gradle = new List<string>( File.ReadAllLines( gradlePath ) );
            gradle = UpdateGradlePluginVersion( gradle );
#endif

            int line = 0;
            // Find Dependency
            while (line < gradle.Count && !gradle[line++].Contains( dependencies )) { }

            if (line >= gradle.Count)
                Utils.StopBuildWithMessage( "Not found Dependencies scope in Gradle template. " +
                    "Please try delete and create new mainTemplate.gradle", BuildTarget.NoTarget );

            // chek exist
            bool multidexExist = false;
            while (line < gradle.Count && !gradle[line].Contains( "}" ))
            {
                if (gradle[line].Contains( multidexObsoleted ))
                {
                    gradle[line] = "    " + multidexImplementation;
                    Debug.Log( Utils.logTag + "Replace dependency " + multidexObsoleted +
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
                if (EditorUtility.DisplayDialog( "CAS Preprocess Build",
                        "Including the CAS SDK may cause the 64K limit on methods that can be packaged in an Android dex file to be breached" +
                        "Do you want to use the MultiDex support library to building your app correctly?",
                        "Enable MultiDex", "Continue" ))
                {
                    gradle.Insert( line, "    " + multidexImplementation );
                    Debug.Log( Utils.logTag + "Append dependency " + multidexImplementation );
                    line++;
                    multidexExist = true;
                }
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
                Utils.StopBuildWithMessage( "Not found Default Config scope in Gradle template. " +
                    "Please try delete and create new mainTemplate.gradle", BuildTarget.NoTarget );

            if (!sourceCompatibilityExist)
            {
                if (EditorUtility.DisplayDialog( "CAS Preprocess Build",
                        "CAS SDK requires for correct operation to determine the Java version in Gradle." +
                        "Do you want to use Java 1.8?",
                        "Set Java version", "Continue" ))
                {
                    gradle.InsertRange( line, new[] {
                        "	compileOptions {",
                        "        " + sourceCompatibility,
                        "        " + targetCompatibility,
                        "	}",
                        ""
                    } );
                    Debug.Log( Utils.logTag + "Append Compile options to use Java Version 1.8." );
                    line += 5;
                }
            }

            if (!packagingOptExist)
            {
                gradle.InsertRange( line, new[] {
                    "	packagingOptions {",
                    "        " + excludeOption,
                    "	}",
                    ""
                } );
                Debug.Log( Utils.logTag + "Append Packaging options to exclude duplicate files." );
                line += 4;
            }

            // Find multidexEnable
            if (multidexExist)
            {
                var defaultConfigLine = line;
                var multidexEnabled = false;
                while (line < gradle.Count)
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
                    gradle.Insert( defaultConfigLine + 1, "        " + multidexConfig );
                    Debug.Log( Utils.logTag + "Enable Multidex in Default Config" );
                }
            }

            File.WriteAllLines( gradlePath, gradle.ToArray() );
            AssetDatabase.ImportAsset( gradlePath );
        }

        private static List<string> UpdateGradlePluginVersion( List<string> gradle )
        {
            const string gradlePluginVersion = "classpath 'com.android.tools.build:gradle:";
            int line = 0;
            // Find Gradle Plugin Version
            while (line < gradle.Count && !gradle[line].Contains( gradlePluginVersion ))
            {
                line++;
            }

            if (line < gradle.Count)
            {
                var versionLine = gradle[line];
                var index = versionLine.IndexOf( gradlePluginVersion );
                if (index > 0)
                {
                    var verStr = versionLine.Substring( index + gradlePluginVersion.Length, 5 );
                    try
                    {
                        System.Version version = new System.Version( verStr );
                        if (version.Major == 4)
                        {
                            if (version.Minor == 0 & version.Build < 1 && RequestUpdateGradleVersion( version.ToString() ))
                                gradle[line] = versionLine.Remove( versionLine.Length - 2 ) + "1'";
                        }
                        else
                        {
                            switch (version.Minor)
                            {
                                case 3:
                                case 4:
                                    if (version.Build < 3 && RequestUpdateGradleVersion( version.ToString() ))
                                        gradle[line] = versionLine.Remove( versionLine.Length - 2 ) + "3'";
                                    break;
                                case 5:
                                case 6:
                                    if (version.Build < 4 && RequestUpdateGradleVersion( version.ToString() ))
                                        gradle[line] = versionLine.Remove( versionLine.Length - 2 ) + "4'";
                                    break;
                            }
                        }
                    }
                    catch { }
                }
            }
            return gradle;
        }

        private static bool RequestUpdateGradleVersion( string version = "" )
        {
            return EditorUtility.DisplayDialog( "CAS Preprocess Build",
                        "Android Gradle plugin " + version + " are not supports targeting Android 11. " +
                        "Do you want to upgrade to version with fix?",
                        "Update", "Continue" );
        }

        private static void CopyTemplateIfNeedToAndroidLib( string template, string path )
        {
            if (!File.Exists( path ))
            {
                if (!AssetDatabase.IsValidFolder( Utils.androidLibFolderPath ))
                {
                    Directory.CreateDirectory( Utils.androidLibFolderPath );
                    AssetDatabase.ImportAsset( Utils.androidLibFolderPath );
                }
                var templateFile = Utils.GetTemplatePath( template );
                if (templateFile == null || !Utils.TryCopyFile( templateFile, path ))
                    Utils.StopBuildWithMessage( "Build failed", BuildTarget.NoTarget );
            }
        }

        private static void SetAdmobAppIdToAndroidManifest( string admobAppId, bool newFile )
        {
            const string metaAdmobApplicationID = "com.google.android.gms.ads.APPLICATION_ID";
            XNamespace ns = "http://schemas.android.com/apk/res/android";

            string manifestPath = Path.GetFullPath( Utils.androidLibManifestPath );

            CopyTemplateIfNeedToAndroidLib(
                    Utils.androidLibManifestTemplateFile, Utils.androidLibManifestPath );

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
                Utils.StopBuildWithMessage(
                    "AndroidManifest.xml is not valid. Try re-importing the plugin.", BuildTarget.Android );

            Debug.LogWarning( Utils.logTag + "AndroidManifest.xml is not valid. Created new file by template." );
            AssetDatabase.DeleteAsset( Utils.androidLibManifestPath );
            SetAdmobAppIdToAndroidManifest( admobAppId, true );
        }

        public static string DownloadRemoteSettings( string managerID, string country, BuildTarget platform )
        {
            string title = "Download CAS settings for " + platform.ToString();
            string url = Utils.BuildRemoteUrl( managerID, country, platform );
            string message = null;

            using (var loader = UnityWebRequest.Get( url ))
            {
                loader.SendWebRequest();
                while (!loader.isDone)
                {
                    if (EditorUtility.DisplayCancelableProgressBar( casTitle, title,
                        Mathf.Repeat( ( float )EditorApplication.timeSinceStartup, 1.0f ) ))
                    {
                        loader.Dispose();
                        message = "Update CAS Settings canceled";
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();

                if (message == null)
                {
                    if (string.IsNullOrEmpty( loader.error ))
                    {
                        EditorUtility.DisplayProgressBar( casTitle, "Write CAS settings", 0.7f );
                        var content = loader.downloadHandler.text.Trim();
                        if (string.IsNullOrEmpty( content ))
                            Utils.StopBuildWithMessage( "Server have no settings for " + managerID +
                                " Please try using a different identifier in the first place or contact support." +
                                " To test build please use Test Ad Mode in settings.", platform );

                        return ApplySettingsContent( content, platform );
                    }
                    else
                    {
                        message = "Server response " + loader.responseCode + ": " + loader.error;
                    }
                }
            }
            if (EditorUtility.DisplayDialog( casTitle, message, "Select settings file", "Cancel Build" ))
            {
                var filePath = EditorUtility.OpenFilePanelWithFilters(
                    "Select CAS Settings file for build", "", new[] { "json" } );
                if (!string.IsNullOrEmpty( filePath ))
                    return ApplySettingsContent( File.ReadAllText( filePath ), platform );
            }
            Utils.StopBuildWithMessage( message, BuildTarget.NoTarget );
            return null;
        }

        private static string ApplySettingsContent( string content, BuildTarget target )
        {
            if (target == BuildTarget.Android)
                Utils.WriteToFile( content, Utils.androidResSettingsPath );
            else
                Utils.WriteToFile( content, Utils.iosResSettingsPath );
            return Utils.GetAdmobAppIdFromJson( content );
        }

        private static void DialogOrCancel( string message, BuildTarget target, string btn = "Continue" )
        {
            if (!EditorUtility.DisplayDialog( casTitle, message, btn, "Cancel build" ))
                Utils.StopBuildWithMessage( "Cancel build: " + message, target );
        }

        private static void StopBuildIDNotFound( BuildTarget target )
        {
            Utils.StopBuildWithMessage( "Settings not found manager ids for " + target.ToString() +
                        " platform. For a successful build, you need to specify at least one ID" +
                        " that you use in the project. To test integration, you can use test mode.", target );
        }
    }
}
#endif