//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            if (deps == null)
                Utils.StopBuildWithMessage("Dependency config is missing. Try re-importing the plugin.");

            if (!Utils.IsBatchMode())
            {
                var newCASVersion = Utils.GetNewVersionOrNull(Utils.gitUnityRepo, MobileAds.wrapperVersion, false);
                if (newCASVersion != null)
                    Debug.LogWarning(Utils.logTag + "There is a new version " + newCASVersion + " of the CAS Unity plugin available for update.");

                if (deps.IsNewerVersionFound())
                    Utils.DialogOrCancelBuild("There is a new versions of the native dependencies available for update." +
                        "Please use 'Assets > CleverAdsSolutions > Settings' menu to update.", target);
            }

            RemoveDeprecatedAssets();

            UpdateRemoteConfig(settings, target, deps);

            if (target == BuildTarget.Android)
                ConfigureAndroid(settings, editorSettings);
            else if (target == BuildTarget.iOS)
                ConfigureIOS();

            if (settings.IsTestAdMode() && !EditorUserBuildSettings.development)
                Debug.LogWarning(Utils.logTag + "Test Ads Mode enabled! Make sure the build is for testing purposes only!\n" +
                    "Use 'Assets > CleverAdsSolutions > Settings' menu to disable Test Ad Mode.");
            else
                Utils.Log("Project configuration completed: " + MobileAds.wrapperVersion);
        }

        private static void RemoveDeprecatedAssets()
        {
#if UNITY_ANDROID || CASDeveloper
            // CASPlugin.androidlib migrate from Assets/Plugins to Assets/CleverAdsSolutions/Plugins
            // with CAS 3.5.0 update.
            string plguinInAssets = "Assets/" + Utils.androidLibFolderPath;
            if (Directory.Exists(plguinInAssets))
                AssetDatabase.DeleteAsset(plguinInAssets);
#endif
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


            try
            {
                var iosVersion = int.Parse(PlayerSettings.iOS.targetOSVersionString.Split('.')[0]);
                if (iosVersion < Utils.targetIOSVersion)
                {
                    Utils.DialogOrCancelBuild("CAS required a higher minimum deployment target. Set iOS " +
                        Utils.targetIOSVersion + " and continue?", BuildTarget.NoTarget);
                    PlayerSettings.iOS.targetOSVersionString = Utils.targetIOSVersion + ".0";
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Minimum deployment target check failed: " + e.ToString());
            }
#endif
        }

        private static void ConfigureAndroid(CASInitSettings settings, CASEditorSettings editorSettings)
        {
#if UNITY_ANDROID || CASDeveloper

            // Unity 2021.2 have minimum API 21
#if !UNITY_2021_2_OR_NEWER
            if (PlayerSettings.Android.minSdkVersion < (AndroidSdkVersions)Utils.targetAndroidVersion)
            {
                Utils.DialogOrCancelBuild("CAS required a higher minimum SDK API level. Set SDK level " +
                    Utils.targetAndroidVersion + " and continue?", BuildTarget.NoTarget);
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)Utils.targetAndroidVersion;
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
            CASPreprocessGradle.Configure(editorSettings);
#endif
        }

        private static void UpdateRemoteConfig(CASInitSettings settings, BuildTarget platform, DependencyManager deps)
        {
            if (settings.managersCount == 0 || string.IsNullOrEmpty(settings.GetManagerId(0)))
            {
                Utils.StopBuildWithMessage("Settings not found manager ids for " + platform.ToString() +
                    " platform. For a successful build, you need to specify at least one ID" +
                    " that you use in the project. To test integration, you can use test mode with 'demo' manager id.", platform);
            }

            string appId = null;
            string updateSettingsError = "";
            bool appIdRequired = !settings.IsTestAdMode() && deps.Find(AdNetwork.GoogleAds).IsInstalled();
            for (int i = 0; i < settings.managersCount; i++)
            {
                var casId = settings.GetManagerId(i);
                if (casId == null || casId.Length < 5)
                    continue;
                try
                {
                    AdRemoteConfig data = DownloadRemoteSettings(casId, platform, appIdRequired);
                    if (string.IsNullOrEmpty(appId))
                        appId = data.admob_app_id;
                }
                catch (Exception e)
                {
                    updateSettingsError = e.Message;
                }
            }
            if (!appIdRequired || !string.IsNullOrEmpty(appId))
                return;

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
                return;

            if (dialogResponse == 1)
            {
                Utils.StopBuildWithMessage("Build canceled");
                return;
            }

            string openPath = "";
            try
            {
                var fileName = AdRemoteConfig.GetResourcesFileName(targetId);
                openPath = EditorUtility.OpenFilePanelWithFilters(
                   "Select " + fileName + " file for build", "", new[] { fileName, "json" });
                if (!string.IsNullOrEmpty(openPath))
                {
                    var config = AdRemoteConfig.ReadFromFile(openPath);
                    if (config.IsValid(appIdRequired))
                    {
                        config.Save(AdRemoteConfig.GetCachePath(platform, targetId));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            Utils.StopBuildWithMessage("Open invalid config file: " + openPath);
            return;
        }

        private static AdRemoteConfig DownloadRemoteSettings(string casId, BuildTarget platform, bool appIdRequired)
        {
            const string title = "Update CAS remote configuration";

            var cachePath = AdRemoteConfig.GetCachePath(platform, casId);
            try
            {
                if (File.Exists(cachePath) && File.GetLastWriteTime(cachePath).AddHours(3) > DateTime.Now)
                {
                    var data = AdRemoteConfig.ReadFromFile(cachePath);
                    if (data.IsValid(appIdRequired))
                        return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Utils.logTag + e.Message);
            }

            var urlBuilder = new StringBuilder("https://psvpromo.psvgamestudio.com/cas-settings.php?apply=config&platform=")
                .Append(platform == BuildTarget.Android ? 0 : 1)
                .Append("&bundle=").Append(UnityWebRequest.EscapeURL(casId));

            using (var request = new EditorWebRequest(urlBuilder.ToString())
                .WithProgress(title)
                .StartSync())
            {
                if (request.responseCode == 204)
                    throw new Exception("'" + casId + "' is not registered in CAS.");

                var content = request.ReadContent();
                if (string.IsNullOrEmpty(content))
                    throw new Exception("Connect to server for '" + casId +
                        "' is failed with error: " + request.responseCode + " - " + request.error);

                var data = AdRemoteConfig.ReadFromJson(content);
                if (data.IsValid(appIdRequired))
                {
                    data.Save(cachePath);
                    return data;
                }
            }
            throw new Exception("The configuration for '" + casId + "' is not valid, please contact support for additional information.");
        }
    }
}
#endif