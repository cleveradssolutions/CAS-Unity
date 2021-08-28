#if UNITY_ANDROID || UNITY_IOS || CASDeveloper
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System;
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
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        #endregion

        public static void ValidateIntegration( BuildTarget target )
        {
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
                return;

            var isBatchMode = Application.isBatchMode;
            var settings = Utils.GetSettingsAsset( target );
            if (!settings)
                Utils.StopBuildWithMessage( "Settings not found. Please use menu Assets/CleverAdsSolutions/Settings " +
                    "to create and set settings for build.", target );

            var deps = DependencyManager.Create( target, Audience.Mixed, true );
            if (!isBatchMode)
            {
                var newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, false );
                if (newCASVersion != null)
                    DialogOrCancel( "There is a new version " + newCASVersion + " of the CAS Unity available for update.", target );


                if (deps != null)
                {
                    if (!deps.installedAny)
                        Utils.StopBuildWithMessage( "Dependencies of native SDK were not found. " +
                        "Please use 'Assets/CleverAdsSolutions/Settings' menu to integrate solutions or any SDK separately.", target );

                    bool isNewerVersionExist = false;
                    for (int i = 0; i < deps.solutions.Length; i++)
                    {
                        if (deps.solutions[i].isNewer)
                        {
                            isNewerVersionExist = true;
                            break;
                        }
                    }
                    if (!isNewerVersionExist)
                    {
                        for (int i = 0; i < deps.networks.Length; i++)
                        {
                            if (deps.networks[i].isNewer)
                            {
                                isNewerVersionExist = true;
                                break;
                            }
                        }
                    }

                    if (isNewerVersionExist)
                        DialogOrCancel( "There is a new versions of the native dependencies available for update." +
                            "Please use 'Assets/CleverAdsSolutions/Settings' menu to update.", target );
                }

                if (settings.bannerSize == AdSize.Banner && Utils.IsPortraitOrientation())
                {
                    DialogOrCancel( "For portrait applications, we recommend using the adaptive banner size." +
                            "This will allow you to get more expensive advertising.", target );
                }
            }

            if (settings.managersCount == 0 || string.IsNullOrEmpty( settings.GetManagerId( 0 ) ))
                StopBuildIDNotFound( target );

            string admobAppId = null;
            string updateSettingsError = "";
            for (int i = 0; i < settings.managersCount; i++)
            {
                var managerId = settings.GetManagerId( i );
                if (managerId != null && managerId.Length > 4)
                {
                    string newAppId = null;
                    try
                    {
                        newAppId = Utils.DownloadRemoteSettings( managerId, target, settings, deps );
                    }
                    catch (Exception e)
                    {
                        Debug.LogError( Utils.logTag + e.Message );
                        updateSettingsError = e.Message;
                    }

                    if (newAppId != null && string.IsNullOrEmpty( admobAppId ) && newAppId.Contains( '~' ))
                        admobAppId = newAppId;
                }
            }

            if (string.IsNullOrEmpty( admobAppId ) && !settings.IsTestAdMode())
                admobAppId = Utils.ReadAppIdFromCache( settings.GetManagerId( 0 ), target, updateSettingsError );

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

                HashSet<string> promoAlias = new HashSet<string>();
                for (int i = 0; i < settings.managersCount; i++)
                {
                    Utils.GetCrossPromoAlias( target, settings.GetManagerId( i ), promoAlias );
                }
                SetAdmobAppIdToAndroidManifest( admobAppId, false, promoAlias );

                ConfigurateGradleSettings();

                if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel19)
                {
                    DialogOrCancel( "CAS required minimum SDK API level 19 (KitKat).", BuildTarget.NoTarget, "Set API 19" );
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
                }
            }
            else if (target == BuildTarget.iOS)
            {
                if (PlayerSettings.muteOtherAudioSources)
                {
                    if (isBatchMode || EditorUtility.DisplayDialog( casTitle, "Mute Other AudioSources are not supported. " +
                        "Is a known issue with disabling sounds in Unity after closing interstitial ads.",
                        "Disable Mute AudioSources", "Continue" ))
                        PlayerSettings.muteOtherAudioSources = false;
                }

                try
                {
                    if (new Version( PlayerSettings.iOS.targetOSVersionString ) < new Version( 10, 0 ))
                    {
                        if (!isBatchMode)
                            DialogOrCancel( "CAS required a higher minimum deployment target. Set iOS 10.0 and continue?", BuildTarget.NoTarget );
                        PlayerSettings.iOS.targetOSVersionString = "10.0";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
            }
            if (settings.IsTestAdMode())
                Debug.LogWarning( Utils.logTag + "Test Ads Mode enabled! Make sure the build is for testing purposes only!" +
                    "\nUse 'Assets/CleverAdsSolutions/Settings' menu to disable Test Ad Mode." );
            else
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

            bool multidexRequired = true; //TODO: Open API change state

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
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
            }

            if (!multidexRequired)
                return;

            const string gradlePath = Utils.launcherGradlePath;
            if (!File.Exists( gradlePath ))
                Utils.StopBuildWithMessage(
                    "Build failed because CAS could not enable MultiDEX in Unity 2019.3 without " +
                    "Custom Launcher Gradle Template. Please enable 'Custom Launcher Gradle Template' found under " +
                    "'Player Settings -> Settings for Android -> Publishing Settings' menu.", BuildTarget.NoTarget );
            List<string> gradle = new List<string>( File.ReadAllLines( gradlePath ) );

#else
            const string gradlePath = Utils.mainGradlePath;
            if (!File.Exists( gradlePath ))
            {
                Debug.LogWarning( Utils.logTag + "We recommended enable 'Custom Launcher Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu to allow CAS update Gradle plugin version." );
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

            bool multidexExist = !multidexRequired;
            if (multidexRequired)
            {
                // chek exist
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
                    if (Application.isBatchMode || EditorUtility.DisplayDialog( "CAS Preprocess Build",
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
                if (Application.isBatchMode || EditorUtility.DisplayDialog( "CAS Preprocess Build",
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
            return Application.isBatchMode || EditorUtility.DisplayDialog( "CAS Preprocess Build",
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

        private static void SetAdmobAppIdToAndroidManifest( string admobAppId, bool newFile, HashSet<string> queries )
        {
            const string metaAdmobApplicationID = "com.google.android.gms.ads.APPLICATION_ID";
            XNamespace ns = "http://schemas.android.com/apk/res/android";

            string manifestPath = Path.GetFullPath( Utils.androidLibManifestPath );

            if (string.IsNullOrEmpty( admobAppId ))
                admobAppId = Utils.androidAdmobSampleAppID;

            CopyTemplateIfNeedToAndroidLib(
                    Utils.androidLibManifestTemplateFile, Utils.androidLibManifestPath );

            bool appIdUpdated = false;
            XElement elemManifest = null;
            try
            {
                XDocument manifest = XDocument.Load( manifestPath );
                elemManifest = manifest.Element( "manifest" );
                XElement elemApplication = elemManifest.Element( "application" );
                IEnumerable<XElement> metas = elemApplication.Descendants()
                    .Where( elem => elem.Name.LocalName.Equals( "meta-data" ) );

                foreach (XElement elem in metas)
                {
                    if (appIdUpdated)
                        break;
                    IEnumerable<XAttribute> attrs = elem.Attributes();
                    foreach (XAttribute attr in attrs)
                    {
                        if (attr.Name.Namespace.Equals( ns )
                                && attr.Name.LocalName.Equals( "name" ) && attr.Value.Equals( metaAdmobApplicationID ))
                        {
                            elem.SetAttributeValue( ns + "value", admobAppId );
                            appIdUpdated = true;
                            break;
                        }
                    }
                }

                var elemQueries = elemManifest.Element( "queries" );
                if (elemQueries != null)
                    elemQueries.Remove();

                if (queries.Count > 0)
                {
                    elemQueries = new XElement( "queries" );
                    elemQueries.Add( new XComment( "CAS Cross promotion" ) );
                    foreach (var item in queries)
                    {
                        elemQueries.Add( new XElement( "package",
                            new XAttribute( ns + "name", item ) ) );
                    }
                    elemManifest.Add( elemQueries );
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            if (appIdUpdated)
            {
                if (elemManifest != null)
                    elemManifest.Save( manifestPath );
            }
            else
            {
                if (newFile)
                    Utils.StopBuildWithMessage(
                        "AndroidManifest.xml is not valid. Try re-importing the plugin.", BuildTarget.Android );

                Debug.LogWarning( Utils.logTag + "AndroidManifest.xml is not valid. Created new file by template." );
                AssetDatabase.DeleteAsset( Utils.androidLibManifestPath );
                SetAdmobAppIdToAndroidManifest( admobAppId, true, queries );
            }
        }

        private static void DialogOrCancel( string message, BuildTarget target, string btn = "Continue" )
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog( casTitle, message, btn, "Cancel build" ))
                Utils.StopBuildWithMessage( "Cancel build: " + message, target );
        }

        private static void StopBuildIDNotFound( BuildTarget target )
        {
            Utils.StopBuildWithMessage( "Settings not found manager ids for " + target.ToString() +
                        " platform. For a successful build, you need to specify at least one ID" +
                        " that you use in the project. To test integration, you can use test mode with 'demo' manager id.", target );
        }
    }
}
#endif