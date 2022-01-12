#define AppendPackagingOptions

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
            var settings = Utils.GetSettingsAsset( target, false );
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

                var editorSettings = CASEditorSettings.Load();

                const string deprecatedPluginPath = "Assets/Plugins/CAS";
                if (AssetDatabase.IsValidFolder( deprecatedPluginPath ))
                {
                    AssetDatabase.DeleteAsset( deprecatedPluginPath );
                    Debug.Log( "Removed deprecated plugin: " + deprecatedPluginPath );
                }

                HashSet<string> promoAlias = new HashSet<string>();
                // TODO: Create option to disable CrossPromo Querries generator
                for (int i = 0; i < settings.managersCount; i++)
                    Utils.GetCrossPromoAlias( target, settings.GetManagerId( i ), promoAlias );

                UpdateAndroidPluginManifest( admobAppId, promoAlias, editorSettings );

                ConfigurateGradleSettings( editorSettings );

#if !UNITY_2021_2_OR_NEWER
                // 19 - AndroidSdkVersions.AndroidApiLevel19
                // Deprecated in Unity 2021.2
                if (PlayerSettings.Android.minSdkVersion < ( AndroidSdkVersions )19)
                {
                    DialogOrCancel( "CAS required a higher minimum SDK API level. Set SDK level 19 (KitKat) and continue?", BuildTarget.NoTarget );
                    PlayerSettings.Android.minSdkVersion = ( AndroidSdkVersions )19;
                }
#endif
            }
            else if (target == BuildTarget.iOS)
            {
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
#pragma warning disable CS0618 // Type or member is obsolete
            if (settings.testAdMode && !EditorUserBuildSettings.development) // Use directrly property to avoid Debug build
#pragma warning restore CS0618 // Type or member is obsolete
                Debug.LogWarning( Utils.logTag + "Test Ads Mode enabled! Make sure the build is for testing purposes only!" +
                    "\nUse 'Assets/CleverAdsSolutions/Settings' menu to disable Test Ad Mode." );
            else
                Debug.Log( Utils.logTag + "Preprocess Build done." );
            EditorUtility.DisplayProgressBar( "Hold on", "Prepare components...", 0.95f );
        }

        private static void ConfigurateGradleSettings( CASEditorSettings settings )
        {
            //const string enabledAndroidX = "\"android.useAndroidX\", true";
            //const string enabledResolveInGradle = "// Android Resolver Dependencies";

            //string gradle = File.ReadAllText( Utils.mainGradlePath );

            //if (gradle.Contains( enabledResolveInGradle ) && !gradle.Contains( enabledAndroidX ))
            //{
            //    StopBuildWithMessage( "For successful build need enable Jetfier by Android Resolver Settings and do Android Resolve again. " +
            //        "Project should use Gradle tools version 3.4+." );
            //}

            bool multidexRequired = settings.multiDexEnabled;

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

            bool multidexExist = false;

            // chek exist
            while (line < gradle.Count && !gradle[line].Contains( "}" ))
            {
                if (gradle[line].Contains( multidexObsoleted ))
                {
                    if (multidexRequired)
                    {
                        gradle[line] = "    " + multidexImplementation;

                        Debug.Log( Utils.logTag + "Replace dependency " + multidexObsoleted +
                            " to " + multidexImplementation );
                    }
                    else
                    {
                        gradle[line] = "";
                    }
                    multidexExist = true;
                    break;
                }
                if (gradle[line].Contains( multidexImplementation ))
                {
                    if (!multidexRequired)
                        gradle[line] = "";
                    multidexExist = true;
                    break;
                }

                line++;
            }

            // write dependency
            if (!multidexExist && multidexRequired)
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

            var sourceCompatibilityExist = false;
#if AppendPackagingOptions
            var packagingOptExist = false;
#endif

            // Find Default Config
            while (line < gradle.Count && !gradle[line].Contains( defaultConfig ))
            {
                if (!sourceCompatibilityExist && gradle[line].Contains( sourceCompatibility ))
                    sourceCompatibilityExist = true;
                if (!sourceCompatibilityExist && gradle[line].Contains( targetCompatibility ))
                    sourceCompatibilityExist = true;
#if AppendPackagingOptions
                if (!packagingOptExist && gradle[line].Contains( excludeOption ))
                    packagingOptExist = true;
#endif
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

#if AppendPackagingOptions
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
#endif

            // Find multidexEnable
            if (multidexExist)
            {
                var defaultConfigLine = line;
                var multidexEnabled = false;
                while (line < gradle.Count)
                {
                    if (gradle[line].Contains( multidexConfig ))
                    {
                        if (!multidexRequired)
                            gradle[line] = "";
                        multidexEnabled = true;
                        break;
                    }
                    line++;
                }

                if (!multidexEnabled && multidexRequired)
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
                        var version = new Version( verStr );
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
                    "Android Gradle Plugin " + version + " are not supports targeting Android 11. " +
                    "Do you want to upgrade Gradle Plugin Version?",
                    "Upgrade", "Continue" );
        }

        private static void CreateAndroidLibIfNedded()
        {
            if (!AssetDatabase.IsValidFolder( Utils.androidLibFolderPath ))
            {
                Directory.CreateDirectory( Utils.androidLibFolderPath );
                AssetDatabase.ImportAsset( Utils.androidLibFolderPath );
            }
        }

        [MenuItem( "Test/ManifestUpdate" )]
        private static void TestManifestUpdate()
        {
            UpdateAndroidPluginManifest( "admobID", new HashSet<string>(), CASEditorSettings.Load() );
        }

        private static void UpdateAndroidPluginManifest( string admobAppId, HashSet<string> queries, CASEditorSettings settings, bool firstTry = true )
        {
            const string metaAdmobApplicationID = "com.google.android.gms.ads.APPLICATION_ID";
            const string metaAdmobDelayInit = "com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT";

            XNamespace ns = "http://schemas.android.com/apk/res/android";
            XNamespace nsTools = "http://schemas.android.com/tools";
            XName nameAttribute = ns + "name";
            XName valueAttribute = ns + "value";

            string manifestPath = Path.GetFullPath( Utils.androidLibManifestPath );

            if (string.IsNullOrEmpty( admobAppId ))
                admobAppId = Utils.androidAdmobSampleAppID;

            CreateAndroidLibIfNedded();

            if (!File.Exists( Utils.androidLibPropertiesPath ))
            {
                const string pluginProperties =
                    "# This file is automatically generated by CAS Unity plugin.\n" +
                    "# Do not modify this file -- YOUR CHANGES WILL BE ERASED!\n" +
                    "android.library=true\n" +
                    "target=android-29\n";
                File.WriteAllText( Utils.androidLibPropertiesPath, pluginProperties );
                AssetDatabase.ImportAsset( Utils.androidLibPropertiesPath );
            }

            try
            {
                var document = new XDocument(
                    new XDeclaration( "1.0", "utf-8", null ),
                    new XComment( "This file is automatically generated by CAS Unity plugin from `Assets > CleverAdsSolutions > Android Settings`" ),
                    new XComment( "Do not modify this file -- YOUR CHANGES WILL BE ERASED!" ) );
                var elemManifest = new XElement( "manifest",
                    new XAttribute( XNamespace.Xmlns + "android", ns ),
                    new XAttribute( XNamespace.Xmlns + "tools", nsTools ),
                    new XAttribute( "package", "com.cleversolutions.ads.unitycas" ),
                    new XAttribute( ns + "versionName", MobileAds.wrapperVersion ),
                    new XAttribute( ns + "versionCode", 1 ) );
                document.Add( elemManifest );

                var delayInitState = settings.delayAppMeasurementGADInit ? "true" : "false";

                var elemApplication = new XElement( "application" );

                var elemAppIdMeta = new XElement( "meta-data",
                        new XAttribute( nameAttribute, metaAdmobApplicationID ),
                        new XAttribute( valueAttribute, admobAppId ) );
                elemApplication.Add( elemAppIdMeta );

                var elemDelayInitMeta = new XElement( "meta-data",
                        new XAttribute( nameAttribute, metaAdmobDelayInit ),
                        new XAttribute( valueAttribute, delayInitState ) );
                elemApplication.Add( elemDelayInitMeta );

                var elemUsesLibrary = new XElement( "uses-library",
                    new XAttribute( ns + "required", "false" ),
                    new XAttribute( nameAttribute, "org.apache.http.legacy" ) );
                elemApplication.Add( elemUsesLibrary );
                elemManifest.Add( elemApplication );

                var elemInternetPermission = new XElement( "uses-permission",
                    new XAttribute( nameAttribute, "android.permission.INTERNET" ) );
                elemManifest.Add( elemInternetPermission );

                var elemNetworkPermission = new XElement( "uses-permission",
                    new XAttribute( nameAttribute, "android.permission.ACCESS_NETWORK_STATE" ) );
                elemManifest.Add( elemNetworkPermission );

                var elemWIFIPermission = new XElement( "uses-permission",
                    new XAttribute( nameAttribute, "android.permission.ACCESS_WIFI_STATE" ) );
                elemManifest.Add( elemWIFIPermission );

                var elemAdIDPermission = new XElement( "uses-permission",
                    new XAttribute( nameAttribute, "com.google.android.gms.permission.AD_ID" ) );
                if (settings.permissionAdIdRemoved)
                    elemAdIDPermission.SetAttributeValue( nsTools + "node", "remove" );
                elemManifest.Add( elemAdIDPermission );

                if (queries.Count > 0)
                {
                    var elemQueries = new XElement( "queries" );
                    elemQueries.Add( new XComment( "CAS Cross promotion" ) );
                    foreach (var item in queries)
                    {
                        elemQueries.Add( new XElement( "package",
                            new XAttribute( nameAttribute, item ) ) );
                    }
                    elemManifest.Add( elemQueries );
                }
                
                var exist = File.Exists( Utils.androidLibManifestPath );
                // XDocument required absolute path
                document.Save( manifestPath );
                // But Unity not support absolute path
                if (!exist)
                    AssetDatabase.ImportAsset( Utils.androidLibManifestPath );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
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