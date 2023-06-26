//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

//#define GenerateAndroidQuerriesForCASPromo

#if UNITY_ANDROID || UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Networking;
using Utils = CAS.UEditor.CASEditorUtils;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace CAS.UEditor
{
#if UNITY_2018_1_OR_NEWER
    public class CASPreprocessBuild : IPreprocessBuildWithReport
#else
    public class CASPreprocessBuild : IPreprocessBuild
#endif
    {
        private const string casTitle = "CAS Configure project";
        #region IPreprocessBuild
        public int callbackOrder { get { return -25000; } }

#if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild(BuildReport report)
        {
            BuildTarget target = report.summary.platform;
#else
        public void OnPreprocessBuild( BuildTarget target, string path )
        {
#endif
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
                return;
            try
            {
                var editorSettings = CASEditorSettings.Load();
                if (editorSettings.buildPreprocessEnabled)
                {
                    ConfigureProject(target, editorSettings);
#if UNITY_2019_1_OR_NEWER
                }
            }
            finally
            {
                // Unity 2020 does not replace progress bars at the start of a build.
                EditorUtility.ClearProgressBar();
            }
#else
                    EditorUtility.DisplayProgressBar("Hold on", "Prepare components...", 0.95f);
                }
            }
            catch (Exception e)
            {
                // If no errors are found then there is no need to clear the progress for the user.
                EditorUtility.ClearProgressBar();
                throw e;
            }
#endif
        }
        #endregion

        internal static void ConfigureProject(BuildTarget target, CASEditorSettings editorSettings)
        {
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
                return;

            var settings = Utils.GetSettingsAsset(target, false);
            if (!settings)
                Utils.StopBuildWithMessage("Settings asset not found. Please use menu Assets > CleverAdsSolutions > Settings " +
                    "to create and set settings for build.", target);

            var deps = DependencyManager.Create(target, Audience.Mixed, true);
            if (!Utils.IsBatchMode())
            {
                var newCASVersion = Utils.GetNewVersionOrNull(Utils.gitUnityRepo, MobileAds.wrapperVersion, false);
                if (newCASVersion != null)
                    Debug.LogWarning(Utils.logTag + "There is a new version " + newCASVersion + " of the CAS Unity plugin available for update.");

                if (deps != null && deps.IsNewerVersionFound())
                    Utils.DialogOrCancelBuild("There is a new versions of the native dependencies available for update." +
                        "Please use 'Assets > CleverAdsSolutions >Settings' menu to update.", target);
            }

            if (settings.managersCount == 0 || string.IsNullOrEmpty(settings.GetManagerId(0)))
                StopBuildIDNotFound(target);

            string admobAppId = UpdateRemoteSettingsAndGetAppId(settings, target, deps);

            RemoveDeprecatedAssets();

            if (target == BuildTarget.Android)
                ConfigureAndroid(settings, editorSettings, admobAppId);
            else if (target == BuildTarget.iOS)
                ConfigureIOS();

            if (settings.IsTestAdMode() && !EditorUserBuildSettings.development)
                Debug.LogWarning(Utils.logTag + "Test Ads Mode enabled! Make sure the build is for testing purposes only!\n" +
                    "Use 'Assets > CleverAdsSolutions > Settings' menu to disable Test Ad Mode.");
            else
                Debug.Log(Utils.logTag + "Project configuration completed");
        }

        private static void RemoveDeprecatedAssets()
        {
#if UNITY_ANDROID || CASDeveloper
            const string deprecatedPluginPath = "Assets/Plugins/CAS";
            if (Directory.Exists(deprecatedPluginPath))
                AssetDatabase.MoveAssetToTrash(deprecatedPluginPath);

            var androidRes = Path.GetDirectoryName(Utils.androidResSettingsPath);
            if (Directory.Exists(androidRes))
            {
                var androidConfig = Directory.GetFiles(androidRes, "cas_settings*.json", SearchOption.TopDirectoryOnly);
                var currentTime = DateTime.Now;
                for (int i = 0; i < androidConfig.Length; i++)
                {
                    if (File.GetLastWriteTime(androidConfig[i]).AddHours(12) < currentTime)
                    {
                        File.Delete(androidConfig[i]);
                        if (File.Exists(androidConfig[i] + ".meta"))
                            File.Delete(androidConfig[i] + ".meta");
                    }
                }
            }
#endif
#if UNITY_IOS || CASDeveloper
            var iosConfigDeprecated = Directory.GetFiles("ProjectSettings", "ios_cas_settings*.json", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < iosConfigDeprecated.Length; i++)
            {
                File.Delete(iosConfigDeprecated[i]);
            }
#endif

            var removeAssets = AssetDatabase.FindAssets("l:Cas-remove", new[] { "Assets" });
            for (int i = 0; i < removeAssets.Length; i++)
            {
#if !CASDeveloper
                AssetDatabase.MoveAssetToTrash(AssetDatabase.GUIDToAssetPath(removeAssets[i]));
#endif
            }
        }

        private static void ConfigureIOS()
        {
#if UNITY_IOS || CASDeveloper
            if (!Utils.GetIOSResolverSetting<bool>("PodfileStaticLinkFrameworks"))
            {
                Debug.LogWarning(Utils.logTag + "Please enable 'Add use_frameworks!' and 'Link frameworks statically' found under " +
                        "'Assets -> External Dependency Manager -> iOS Resolver -> Settings' menu.\n" +
                        "Failing to do this step may result in undefined behavior of the plugin and doubled import of frameworks.");
            }

            // UNITY_2019_4_OR_NEWER - Deprecated iOS 9
            // UNITY_2020_1_OR_NEWER - Deprecated iOS 10
            var iosVersion = PlayerSettings.iOS.targetOSVersionString;
            if (iosVersion.StartsWith("9.") || iosVersion.StartsWith("10.") || iosVersion.StartsWith("11."))
            {
                Utils.DialogOrCancelBuild("CAS required a higher minimum deployment target. Set iOS 12.0 and continue?", BuildTarget.NoTarget);
                PlayerSettings.iOS.targetOSVersionString = "12.0";
            }
#endif
        }

        private static void ConfigureAndroid(CASInitSettings settings, CASEditorSettings editorSettings, string admobAppId)
        {
#if UNITY_ANDROID || CASDeveloper
            EditorUtility.DisplayProgressBar(casTitle, "Validate CAS Android Build Settings", 0.8f);

#if !UNITY_2021_2_OR_NEWER
            if (PlayerSettings.Android.minSdkVersion < (AndroidSdkVersions)21)
            {
                Utils.DialogOrCancelBuild("CAS required a higher minimum SDK API level. Set SDK level 21 and continue?", BuildTarget.NoTarget);
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)21;
            }
#endif

#if !UNITY_2019_1_OR_NEWER
            // 0 = AndroidBuildSystem.Internal
            // Deprecated in Unity 2019
            if (EditorUserBuildSettings.androidBuildSystem == 0)
            {
                Utils.DialogOrCancelBuild("Unity Internal build system no longer supported. Set Gradle build system and continue?", BuildTarget.NoTarget);
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }
#endif

            HashSet<string> promoAlias = new HashSet<string>();
#if GenerateAndroidQuerriesForCASPromo
            if (editorSettings.generateAndroidQuerriesForPromo)
            {
                for (int i = 0; i < settings.managersCount; i++)
                    Utils.GetCrossPromoAlias(BuildTarget.Android, settings.GetManagerId(i), promoAlias);
            }
#endif

            UpdateAndroidPluginManifest(admobAppId, promoAlias, editorSettings, settings.defaultAudienceTagged);

            CASPreprocessGradle.Configure(editorSettings);
#endif
        }

        private static string UpdateRemoteSettingsAndGetAppId(CASInitSettings settings, BuildTarget platform, DependencyManager deps)
        {
            string appId = null;
            string updateSettingsError = "";
            for (int i = 0; i < settings.managersCount; i++)
            {
                var managerId = settings.GetManagerId(i);
                if (managerId == null || managerId.Length < 5)
                    continue;
                try
                {
                    string newAppId = DownloadRemoteSettings(managerId, platform, settings, deps);
                    if (!string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(newAppId))
                        continue;
                    if (newAppId.Contains('~'))
                    {
                        appId = newAppId;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    updateSettingsError = e.Message;
                }
            }
            if (!string.IsNullOrEmpty(appId) || settings.IsTestAdMode() || !deps.Find(AdNetwork.GoogleAds).IsInstalled())
                return appId;

            const string title = "Update CAS remote settings";
            int dialogResponse = 0;
            var targetId = settings.GetManagerId(0);

            var message = updateSettingsError +
                "\nPlease try using a real CAS identifier in the first place CAS Settings Window else contact support." +
                "\nIf you haven't created an CAS account and registered an app yet, use Test Ads mode to continue build.";

            Debug.LogError(Utils.logTag + message);
            if (!Utils.IsBatchMode())
                dialogResponse = EditorUtility.DisplayDialogComplex(title, message,
                    "Continue", "Cancel Build", "Select configuration");

            if (dialogResponse == 0)
            {
                var cachePath = Path.GetFullPath(Utils.GetNativeSettingsPath(platform, targetId));
                if (File.Exists(cachePath))
                    return Utils.GetAdmobAppIdFromJson(File.ReadAllText(cachePath));
                return null;
            }
            if (dialogResponse == 1)
            {
                Utils.StopBuildWithMessage("Build canceled", BuildTarget.NoTarget);
                return null;
            }
            return Utils.SelectSettingsFileAndGetAppId(targetId, platform);
        }

        private static void UpdateAndroidPluginManifest(string admobAppId, HashSet<string> queries, CASEditorSettings settings, Audience audience)
        {
            const string metaAdmobApplicationID = "com.google.android.gms.ads.APPLICATION_ID";
            const string metaAdmobDelayInit = "com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT";

            XNamespace ns = "http://schemas.android.com/apk/res/android";
            XNamespace nsTools = "http://schemas.android.com/tools";
            XName nameAttribute = ns + "name";
            XName valueAttribute = ns + "value";

            string manifestPath = Path.GetFullPath(Utils.androidLibManifestPath);
            var manifestExist = File.Exists(manifestPath);

            CreateAndroidLibIfNedded();

            if (string.IsNullOrEmpty(admobAppId))
                admobAppId = Utils.androidAdmobSampleAppID;

            try
            {
                var document = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XComment("This file is automatically generated by CAS Unity plugin from `Assets > CleverAdsSolutions > Android Settings`"),
                    new XComment("Do not modify this file. YOUR CHANGES WILL BE ERASED!"));
                var elemManifest = new XElement("manifest",
                    new XAttribute(XNamespace.Xmlns + "android", ns),
                    new XAttribute(XNamespace.Xmlns + "tools", nsTools),
                    new XAttribute("package", "com.cleversolutions.ads.unitycas"),
                    new XAttribute(ns + "versionName", MobileAds.wrapperVersion),
                    new XAttribute(ns + "versionCode", 1));
                document.Add(elemManifest);

                var delayInitState = settings.delayAppMeasurementGADInit ? "true" : "false";

                var elemApplication = new XElement("application");

                var elemAppIdMeta = new XElement("meta-data",
                        new XAttribute(nameAttribute, metaAdmobApplicationID),
                        new XAttribute(valueAttribute, admobAppId));
                elemApplication.Add(elemAppIdMeta);

                var elemDelayInitMeta = new XElement("meta-data",
                        new XAttribute(nameAttribute, metaAdmobDelayInit),
                        new XAttribute(valueAttribute, delayInitState));
                elemApplication.Add(elemDelayInitMeta);

                var elemUsesLibrary = new XElement("uses-library",
                    new XAttribute(ns + "required", "false"),
                    new XAttribute(nameAttribute, "org.apache.http.legacy"));
                elemApplication.Add(elemUsesLibrary);
                elemManifest.Add(elemApplication);

                var elemInternetPermission = new XElement("uses-permission",
                    new XAttribute(nameAttribute, "android.permission.INTERNET"));
                elemManifest.Add(elemInternetPermission);

                var elemNetworkPermission = new XElement("uses-permission",
                    new XAttribute(nameAttribute, "android.permission.ACCESS_NETWORK_STATE"));
                elemManifest.Add(elemNetworkPermission);

                var elemWIFIPermission = new XElement("uses-permission",
                    new XAttribute(nameAttribute, "android.permission.ACCESS_WIFI_STATE"));
                elemManifest.Add(elemWIFIPermission);

                var elemAdIDPermission = new XElement("uses-permission",
                    new XAttribute(nameAttribute, "com.google.android.gms.permission.AD_ID"));
                if (settings.isUseAdvertiserIdLimited(audience))
                    elemAdIDPermission.SetAttributeValue(nsTools + "node", "remove");
                elemManifest.Add(elemAdIDPermission);

                if (queries.Count > 0)
                {
                    var elemQueries = new XElement("queries");
                    elemQueries.Add(new XComment("CAS Cross promotion"));
                    foreach (var item in queries)
                    {
                        elemQueries.Add(new XElement("package",
                            new XAttribute(nameAttribute, item)));
                    }
                    elemManifest.Add(elemQueries);
                }

                // XDocument required absolute path
                document.Save(manifestPath);
                // But Unity not support absolute path
                if (!manifestExist)
                    AssetDatabase.ImportAsset(Utils.androidLibManifestPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void CreateAndroidLibIfNedded()
        {
            Utils.WriteToAsset(Utils.androidLibPropertiesPath, false,
                "# This file is automatically generated by CAS Unity plugin.",
                "# Do not modify this file -- YOUR CHANGES WILL BE ERASED!",
                "android.library=true",
                "target=android-31");

            Utils.WriteToAsset(Utils.androidLibNetworkConfigPath, false,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                "<network-security-config>",
                "    <!-- The Meta AN SDK use 127.0.0.1 as a caching proxy to cache media files in the SDK -->",
                "    <domain-config cleartextTrafficPermitted=\"true\">",
                "        <domain includeSubdomains=\"true\">127.0.0.1</domain>",
                "    </domain-config>",
                "</network-security-config>");
        }

        private static string DownloadRemoteSettings(string managerID, BuildTarget platform, CASInitSettings settings, DependencyManager deps)
        {
            const string title = "Update CAS remote configuration";

            var editorSettings = CASEditorSettings.Load();

            var cachePath = Utils.GetNativeSettingsPath(platform, managerID);
            try
            {
                var fullPath = Path.GetFullPath(cachePath);
                if (File.Exists(fullPath) && File.GetLastWriteTime(fullPath).AddHours(12) > DateTime.Now)
                {
                    var content = File.ReadAllText(fullPath);
                    var data = JsonUtility.FromJson<AdmobAppIdData>(content);
                    return data.admob_app_id;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Utils.logTag + e.Message);
            }

            var urlBuilder = new StringBuilder("https://psvpromo.psvgamestudio.com/cas-settings.php?apply=config&platform=")
                .Append(platform == BuildTarget.Android ? 0 : 1)
                .Append("&bundle=").Append(UnityWebRequest.EscapeURL(managerID));
            if (!string.IsNullOrEmpty(editorSettings.mostPopularCountryOfUsers))
                urlBuilder.Append("&country=").Append(editorSettings.mostPopularCountryOfUsers);

            using (var request = new EditorWebRequest(urlBuilder.ToString())
                .WithProgress(title)
                .StartSync())
            {
                if (request.responseCode == 204)
                    throw new Exception("'" + managerID + "' is not registered in CAS.");

                var content = request.ReadContent();
                if (string.IsNullOrEmpty(content))
                    throw new Exception("Connect to server for '" + managerID +
                        "' is failed with error: " + request.responseCode + " - " + request.error);

                var data = JsonUtility.FromJson<AdmobAppIdData>(content);
                Utils.WriteToAsset(cachePath, content);
                return data.admob_app_id;
            }
        }

        private static void StopBuildIDNotFound(BuildTarget target)
        {
            Utils.StopBuildWithMessage("Settings not found manager ids for " + target.ToString() +
                " platform. For a successful build, you need to specify at least one ID" +
                " that you use in the project. To test integration, you can use test mode with 'demo' manager id.", target);
        }
    }
}
#endif