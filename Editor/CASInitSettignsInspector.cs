//  Copyright © 2024 CAS.AI. All rights reserved.

#pragma warning disable 649
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using Utils = CAS.UEditor.CASEditorUtils;

namespace CAS.UEditor
{
    [CustomEditor(typeof(CASInitSettings))]
    internal class CASInitSettignsInspector : Editor
    {
        #region Init settings properties
        private SerializedProperty testAdModeProp;
        private SerializedProperty managerIdsProp;
        private SerializedProperty allowedAdFlagsProp;
        private SerializedProperty audienceTaggedProp;
        private SerializedProperty debugModeProp;
        private SerializedProperty bannerRefreshProp;
        private SerializedProperty interstitialIntervalProp;
        private SerializedProperty loadingModeProp;
        private SerializedProperty bannerSizeProp;
        private SerializedProperty trackLocationEnabledProp;
        private SerializedProperty interWhenNoRewardedAdProp;
        #endregion

        #region Editor settings properties
        private SerializedObject editorSettingsObj;
        private SerializedProperty autoCheckForUpdatesEnabledProp;
        private SerializedProperty buildPreprocessEnabledProp;
        private SerializedProperty delayAppMeasurementGADInitProp;
        private SerializedProperty optimizeGADLoadingProp;
#if !UNITY_2022_2_OR_NEWER
        private SerializedProperty updateGradlePluginVersionProp;
#endif
        private SerializedProperty permissionAdIdProp;
        private SerializedProperty attributionReportEndpointProp;
        private SerializedProperty userTrackingUsageDescriptionProp;
        private SerializedProperty includeAdDependencyVersionsProp;
        #endregion

        #region Utility fields
        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private ReorderableList userTrackingList;
        private BuildTarget platform = BuildTarget.NoTarget;
        private string newCASVersion = null;
        private Version edmVersion;
        private string environmentDetails;
        private bool allowedPackageUpdate;
        private bool legacyUnityAdsPackageInstalled;
        private bool edmRequiredNewer = false;

        private bool adsManagerExist = false;
        private int editorRuntimeActiveAdFlags;

        private AnimBool iOSLocationDescriptionFoldout = null;
        private AnimBool otherSettingsFoldout = null;
        #endregion

        #region Initialize logic
        private void OnEnable()
        {
            try
            {
                SetSettingsPlatform();
            }
            catch (MissingReferenceException)
            {
                // The variable m_Targets of CASInitSettignsInspector doesn't exist anymore.
            }

            InitMainProperties(serializedObject);
            InitEditorSettingsProperties();

            iOSLocationDescriptionFoldout = new AnimBool(false, Repaint);
            otherSettingsFoldout = new AnimBool(false, Repaint);

            allowedPackageUpdate = Utils.IsPackageExist(Utils.packageName);
            legacyUnityAdsPackageInstalled = Utils.IsPackageExist(Utils.legacyUnityAdsPackageName);

            dependencyManager = DependencyManager.Create(platform, (Audience)audienceTaggedProp.enumValueIndex, true);

            InitEDM4U();
            InitEnvironmentDetails();

            newCASVersion = Utils.GetNewVersionOrNull(Utils.gitUnityRepo, MobileAds.wrapperVersion, false, OnRemoteCASVersionRecieved);
        }

        private void OnRemoteCASVersionRecieved(string version)
        {
            newCASVersion = version;
            if (version != null)
                Repaint();
        }

        private void InitMainProperties(SerializedObject props)
        {
            testAdModeProp = props.FindProperty("testAdMode");
            managerIdsProp = props.FindProperty("managerIds");
            allowedAdFlagsProp = props.FindProperty("allowedAdFlags");
            audienceTaggedProp = props.FindProperty("audienceTagged");
            debugModeProp = props.FindProperty("debugMode");
            bannerRefreshProp = props.FindProperty("bannerRefresh");
            interstitialIntervalProp = props.FindProperty("interstitialInterval");
            loadingModeProp = props.FindProperty("loadingMode");
            bannerSizeProp = props.FindProperty("bannerSize");
            trackLocationEnabledProp = props.FindProperty("trackLocationEnabled");
            interWhenNoRewardedAdProp = props.FindProperty("interWhenNoRewardedAd");

            editorRuntimeActiveAdFlags = PlayerPrefs.GetInt(Utils.editorRuntimeActiveAdPrefs, -1);
            adsManagerExist = Type.GetType("PSV.AdsManager, PSV.ADS", false) != null;

            managerIdsList = new ReorderableList(props, managerIdsProp, true, true, true, true)
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement,
                onCanRemoveCallback = DisabledRemoveLastItemFromList,
            };
            props.ApplyModifiedProperties();
        }

        private void InitEditorSettingsProperties()
        {
            editorSettingsObj = new SerializedObject(CASEditorSettings.Load(true));
            autoCheckForUpdatesEnabledProp = editorSettingsObj.FindProperty("autoCheckForUpdatesEnabled");
            delayAppMeasurementGADInitProp = editorSettingsObj.FindProperty("delayAppMeasurementGADInit");
            optimizeGADLoadingProp = editorSettingsObj.FindProperty("optimizeGADLoading");
            buildPreprocessEnabledProp = editorSettingsObj.FindProperty("buildPreprocessEnabled");
#if !UNITY_2022_2_OR_NEWER
            updateGradlePluginVersionProp = editorSettingsObj.FindProperty("updateGradlePluginVersion");
#endif
            permissionAdIdProp = editorSettingsObj.FindProperty("permissionAdId");

            attributionReportEndpointProp = editorSettingsObj.FindProperty("attributionReportEndpoint");
            includeAdDependencyVersionsProp = editorSettingsObj.FindProperty("includeAdDependencyVersions");

            userTrackingUsageDescriptionProp = editorSettingsObj.FindProperty("userTrackingUsageDescription");

            userTrackingList = new ReorderableList(editorSettingsObj, userTrackingUsageDescriptionProp, true, true, true, true)
            {
                drawHeaderCallback = DrawNSTrackingListHeader,
                drawElementCallback = DrawNSTrackingListElement,
                onCanRemoveCallback = DisabledRemoveLastItemFromList,
            };
        }

        private void SetSettingsPlatform()
        {
            string assetName = target.name;
            if (assetName.EndsWith(BuildTarget.Android.ToString()))
                platform = BuildTarget.Android;
            else if (assetName.EndsWith(BuildTarget.iOS.ToString()))
                platform = BuildTarget.iOS;
        }

        private void InitEDM4U()
        {
            edmVersion = Utils.GetEDM4UVersion(platform);
            if (edmVersion != null)
                edmRequiredNewer = edmVersion < Utils.minEDM4UVersion;
        }

        private void InitEnvironmentDetails()
        {
            var environmentBuilder = new StringBuilder("Environment Details: ")
                            .Append("Unity ").Append(Application.unityVersion).Append("; ")
                            .Append(Application.platform).Append("; ");
            if (edmVersion != null)
                environmentBuilder.Append("EDM4U ").Append(edmVersion).Append("; ");
#if UNITY_ANDROID || CASDeveloper
            if (platform == BuildTarget.Android)
            {
                var gradleWrapperVersion = CASPreprocessGradle.GetGradleWrapperVersion();
                if (gradleWrapperVersion != null)
                    environmentBuilder.Append("Gradle Wrapper - ").Append(gradleWrapperVersion).Append("; ");
                var targetSDK = (int)PlayerSettings.Android.targetSdkVersion;
                if (targetSDK == 0)
                    environmentBuilder.Append("Target API Auto; ");
                else
                    environmentBuilder.Append("Target API ").Append(targetSDK).Append("; ");
            }
#endif
            if (platform == BuildTarget.iOS)
            {
                environmentBuilder.Append("Target iOS ").Append(PlayerSettings.iOS.targetOSVersionString);
            }
            environmentDetails = environmentBuilder.ToString();
        }
        #endregion

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("CAS.AI", HelpStyles.largeTitleStyle);
            GUILayout.Label(platform.ToString(), HelpStyles.largeTitleStyle, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            Utils.OnHeaderGUI(Utils.gitUnityRepo, allowedPackageUpdate, MobileAds.wrapperVersion, ref newCASVersion);
            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            editorSettingsObj.Update();

            if (managerIdsList.count > 1)
            {
                var appId = managerIdsProp.GetArrayElementAtIndex(0).stringValue;
                if (!string.IsNullOrEmpty(appId))
                    EditorGUILayout.LabelField("Application id in CAS:", appId);
            }
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            GUILayout.Space(5);

            DrawTestAdMode();

            if (DrawAdFlagToggle(AdFlags.Banner))
                DrawBannerScope();
            var inter = DrawAdFlagToggle(AdFlags.Interstitial);
            if (inter)
                DrawInterstitialScope();
            if (DrawAdFlagToggle(AdFlags.Rewarded))
                DrawRewardedScope(inter);

            IsAdFormatsNotUsed();
            DrawSeparator();
            OnEditroRuntimeActiveAdGUI();
            OnAudienceGUI();

            if (dependencyManager == null)
            {
                EditorGUILayout.HelpBox("The integrity of CAS Unity package is broken. " +
                    "Please try to reimport the package or contact support.", MessageType.Error);
            }
            else
            {
                OnWarningsAreaGUI();
                dependencyManager.OnGUI(platform, this);
                OnEDMAreaGUI();
            }

            OnUserTrackingDesctiptionGUI();
            OnOtherSettingsGUI();

            HelpStyles.BeginBoxScope();
            GUILayout.TextField(environmentDetails, EditorStyles.wordWrappedMiniLabel);
            HelpStyles.EndBoxScope();

            OnAppAdsTxtGUI();
            editorSettingsObj.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        #region Draw list implementation
        private void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "CAS Manager ID's " + (platform == BuildTarget.iOS ? "(iTunes ID)" : "(Bundle ID)"));
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = managerIdsProp.GetArrayElementAtIndex(index);
            rect.yMin += 1;
            rect.yMax -= 1;
            item.stringValue = EditorGUI.TextField(rect, item.stringValue).Trim();
        }

        private bool DisabledRemoveLastItemFromList(ReorderableList list)
        {
            return list.count > 1;
        }

        private void DrawNSTrackingListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "(ISO-639) : NSUserTrackingUsageDescription");
        }

        private void DrawNSTrackingListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = userTrackingUsageDescriptionProp.GetArrayElementAtIndex(index);
            rect.yMin += 1;
            rect.yMax -= 1;
            var maxX = rect.xMax;
            rect.xMax = rect.xMin + 40;

            item.Next(true);
            var langCode = item.stringValue;
            EditorGUI.BeginChangeCheck();
            langCode = EditorGUI.TextField(rect, langCode);
            if (EditorGUI.EndChangeCheck())
            {
                if (langCode.Length > 2)
                    langCode = langCode.Substring(0, 2);
                item.stringValue = langCode.ToLower();
            }
            rect.xMin = rect.xMax + 5;
            rect.xMax = maxX;
            item.Next(false);
            item.stringValue = EditorGUI.TextField(rect, item.stringValue);
        }
        #endregion

        private void OnOtherSettingsGUI()
        {
            HelpStyles.BeginBoxScope();
            otherSettingsFoldout.target = GUILayout.Toggle(otherSettingsFoldout.target, "Other settings", EditorStyles.foldout);
            if (!EditorGUILayout.BeginFadeGroup(otherSettingsFoldout.faded))
            {
                EditorGUILayout.EndFadeGroup();
                HelpStyles.EndBoxScope();
                return;
            }

            if (platform == BuildTarget.Android)
            {
                EditorGUILayout.PropertyField(permissionAdIdProp,
                    HelpStyles.GetContent("Advertiser ID permission"));
            }
            else
            {
                trackLocationEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Location collection when user allowed", trackLocationEnabledProp.boolValue);
            }

            OnLoadingModeGUI();

            debugModeProp.boolValue = EditorGUILayout.ToggleLeft(
                "Verbose Debug logging", debugModeProp.boolValue);
            if (debugModeProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "The enabled Verbose Debug Mode will display a lot of useful information for debugging about the states of the sdk with tag 'CAS.AI'. " +
                    "Enabled debug mode affects application performanc and should be disabled after debugging.",
                    MessageType.Warning);
                EditorGUI.indentLevel--;
            }

            if (platform == BuildTarget.Android)
            {
#if !UNITY_2022_2_OR_NEWER
                updateGradlePluginVersionProp.boolValue = EditorGUILayout.ToggleLeft(
                   HelpStyles.GetContent("Update Gradle Plugin enabled", null,
                       "The Gradle plugin version will be updated during build to be optimal " +
                       "for the current Gradle Wrapper version."),
                    updateGradlePluginVersionProp.boolValue);
#endif
                optimizeGADLoadingProp.boolValue = EditorGUILayout.ToggleLeft(
                    HelpStyles.GetContent("Optimize Google Ad loading", null,
                        "Google Ad loading tasks will be offloaded to a background thread"),
                    optimizeGADLoadingProp.boolValue);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                var reportEndpointEnabled = attributionReportEndpointProp.stringValue.Length > 0;
                if (reportEndpointEnabled != EditorGUILayout.ToggleLeft(
                    "Set Attribution Report endpoint", reportEndpointEnabled))
                {
                    reportEndpointEnabled = !reportEndpointEnabled;
                    if (reportEndpointEnabled)
                        attributionReportEndpointProp.stringValue = Utils.attributionReportEndPoint;
                    else
                        attributionReportEndpointProp.stringValue = string.Empty;
                }
                HelpStyles.HelpButton(Utils.gitUnityRepoURL + "/wiki/Include-iOS#ios-15-global-skadnetwork-reporting");
                EditorGUILayout.EndHorizontal();
                if (reportEndpointEnabled)
                {
                    EditorGUI.indentLevel++;
                    attributionReportEndpointProp.stringValue = EditorGUILayout.TextField(
                        attributionReportEndpointProp.stringValue);
                    EditorGUI.indentLevel--;
                }
            }

            buildPreprocessEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Build preprocess enabled", buildPreprocessEnabledProp.boolValue);
            if (!buildPreprocessEnabledProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Automatic configuration at build time is disabled.\n" +
                    "You must use `Assets > CleverAdsSolutions > Configure project` to call configuration manually.",
                    MessageType.Warning);
                EditorGUI.indentLevel--;
            }

            autoCheckForUpdatesEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                "Auto check for CAS updates enabled",
                autoCheckForUpdatesEnabledProp.boolValue);

            delayAppMeasurementGADInitProp.boolValue = EditorGUILayout.ToggleLeft(
                HelpStyles.GetContent("Delay measurement of the Ad SDK initialization", null,
                    "By default, some Mediated Ads SDK initializes app measurement and begins sending user-level event data immediately when the app starts. " +
                    "This can lead to prolonged app loading times and improper use of user data before obtaining their consent"),
                delayAppMeasurementGADInitProp.boolValue);

            includeAdDependencyVersionsProp.boolValue = EditorGUILayout.ToggleLeft(
                "Include Ads dependency versions",
                includeAdDependencyVersionsProp.boolValue);

            EditorGUILayout.EndFadeGroup();
            HelpStyles.EndBoxScope();
        }

        private void OnUserTrackingDesctiptionGUI()
        {
            if (platform != BuildTarget.iOS)
                return;
            HelpStyles.BeginBoxScope();
            var enabled = userTrackingUsageDescriptionProp.arraySize > 0;
            iOSLocationDescriptionFoldout.target = GUILayout.Toggle(iOSLocationDescriptionFoldout.target,
                "User Tracking Usage description: " + (enabled ? "Used" : "Not used"), EditorStyles.foldout);

            if (!enabled && audienceTaggedProp.intValue != (int)Audience.Children)
            {
                EditorGUILayout.HelpBox("To use the AppTrackingTransparency framework: Configure NSUserTrackingUsageDescription to display a request to allow access to the device's advertising ID.", MessageType.Warning);
            }

            if (EditorGUILayout.BeginFadeGroup(iOSLocationDescriptionFoldout.faded))
            {
                EditorGUILayout.BeginHorizontal();
                if (enabled != EditorGUILayout.ToggleLeft("Set Usage description in Info.plist", enabled))
                {
                    enabled = !enabled;
                    if (enabled)
                    {
                        var defDescr = Utils.DefaultUserTrackingUsageDescription();
                        userTrackingUsageDescriptionProp.arraySize = defDescr.Length;
                        for (int i = 0; i < defDescr.Length; i++)
                        {
                            var pair = userTrackingUsageDescriptionProp.GetArrayElementAtIndex(i);
                            pair.Next(true);
                            pair.stringValue = defDescr[i].key;
                            pair.Next(false);
                            pair.stringValue = defDescr[i].value;
                        }
                    }
                    else
                    {
                        userTrackingUsageDescriptionProp.ClearArray();
                    }
                    iOSLocationDescriptionFoldout = new AnimBool(false, Repaint);
                    iOSLocationDescriptionFoldout.target = true;
                }
                HelpStyles.HelpButton(Utils.gitUnityRepoURL + "/wiki/App-Tracking-Transparency");
                EditorGUILayout.EndHorizontal();
                if (enabled)
                    userTrackingList.DoLayoutList();
            }
            EditorGUILayout.EndFadeGroup();
            HelpStyles.EndBoxScope();
        }

        private bool IsAdFormatsNotUsed()
        {
            if (allowedAdFlagsProp.intValue == 0 || allowedAdFlagsProp.intValue == (int)AdFlags.Native)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.HelpBox("Please include the ad formats that you want to use in your game.", MessageType.Error);
                EditorGUI.indentLevel -= 2;
                return true;
            }
            return false;
        }

        private void DrawTestAdMode()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var enabled = testAdModeProp.boolValue;
            var changed = GUILayout.Toggle(enabled, "", GUILayout.ExpandWidth(false));
            changed = GUILayout.Toggle(changed, "Test ad mode", EditorStyles.toolbarButton);
            if (enabled != changed)
            {
                enabled = !enabled;
                testAdModeProp.boolValue = enabled;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel += 2;
            if (enabled)
                EditorGUILayout.HelpBox("Make sure you disable test ad mode and replace demo manager ID with your own CAS ID before publishing your app!", MessageType.Warning);
            else if (EditorUserBuildSettings.development)
                EditorGUILayout.HelpBox("Development build enabled, only test ads are allowed. " +
                    "\nMake sure you disable Development build and use real ad CAS ID before publishing your app!", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("When testing your app, make sure you use Test Ads mode or Development Build. " +
                "Failure to do so can lead to suspension of your account.", MessageType.None);
            EditorGUI.indentLevel -= 2;
        }

        private void DrawInterstitialScope()
        {
            EditorGUI.indentLevel += 2;
            EditorGUILayout.LabelField("Impression interval(sec):");
            interstitialIntervalProp.intValue = EditorGUILayout.IntSlider(interstitialIntervalProp.intValue, 0, 120);
            if (interstitialIntervalProp.intValue > 0)
                EditorGUILayout.HelpBox("For some time after the ad is closed, new ad impressions will fail.", MessageType.None);
            EditorGUI.indentLevel -= 2;
        }

        private void DrawRewardedScope(bool allowInter)
        {
            EditorGUI.indentLevel += 2;
            EditorGUI.BeginDisabledGroup(!allowInter);
            interWhenNoRewardedAdProp.boolValue = EditorGUILayout.ToggleLeft(
               HelpStyles.GetContent("Increase filling by Interstitial ads", null,
               "Sometimes a situation occurs when filling Rewarded ads is not enough, " +
               "in this case, you can allow the display of Interstitial ads to receiving a reward in any case."),
                allowInter && interWhenNoRewardedAdProp.boolValue);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel -= 2;
        }

        private void DrawBannerScope()
        {
            EditorGUI.indentLevel += 2;
            EditorGUILayout.LabelField("Refresh rate(sec):");
            int refresh = EditorGUILayout.IntSlider(bannerRefreshProp.intValue, 0, 180);
            if (refresh < 10)
                refresh = 0;
            if (refresh == 0)
                EditorGUILayout.HelpBox("Refresh ad view content is disabled. Invoke IAdView.Load() to refresh ad.", MessageType.None);
            bannerRefreshProp.intValue = refresh;

            if (adsManagerExist)
            {
                EditorGUILayout.PropertyField(bannerSizeProp, HelpStyles.GetContent("PSV Ads Manager size", null));
            }
            EditorGUI.indentLevel -= 2;
        }

        private bool DrawAdFlagToggle(AdFlags flag)
        {
            var flagInt = (int)flag;
            var enabled = (allowedAdFlagsProp.intValue & flagInt) == flagInt;
            var icon = HelpStyles.GetFormatIcon(flag, enabled);
            var content = HelpStyles.GetContent("", icon);
            var toggleStyle = EditorStyles.toggle;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var changed = GUILayout.Toggle(enabled, "", GUILayout.ExpandWidth(false));
            changed = GUILayout.Toggle(changed, flag.ToString() + " ad format", EditorStyles.toolbarButton);
            GUILayout.Label(content, EditorStyles.toolbar, GUILayout.ExpandWidth(false));
            if (enabled != changed)
            {
                enabled = !enabled;
                if (enabled)
                    allowedAdFlagsProp.intValue = allowedAdFlagsProp.intValue | flagInt;
                else
                    allowedAdFlagsProp.intValue = allowedAdFlagsProp.intValue ^ flagInt;
            }
            EditorGUILayout.EndHorizontal();
            return enabled;
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space();
        }

        private void OnAppAdsTxtGUI()
        {
            var content = HelpStyles.GetContent(" Don’t forget to implement app-ads.txt", HelpStyles.helpIconContent.image);
            if (GUILayout.Button(content, EditorStyles.label, GUILayout.ExpandWidth(false)))
                Application.OpenURL(Utils.gitAppAdsTxtRepoUrl);
        }

        private void InstallEDM4U()
        {
            Utils.InstallUnityPackagePlugin(Utils.latestEMD4uURL)
                 .WithProgress("Download External Dependency Manager");
        }

        private void OnWarningsAreaGUI()
        {
            if (edmVersion == null)
            {
                if (HelpStyles.WarningWithButton(
                    "Mediation requires External Dependency Manager to resolve native dependencies",
                    "Install", MessageType.Error))
                    InstallEDM4U();
                return;
            }
            if (edmRequiredNewer)
            {
                if (HelpStyles.WarningWithButton(
                    "The External Dependency Manager version is outdated, required " + Utils.minEDM4UVersion + " or later.",
                    "Update"))
                    InstallEDM4U();
            }

            if (legacyUnityAdsPackageInstalled)
            {
                if (HelpStyles.WarningWithButton("Legacy Unity Ads package is installed. " +
                    "This package is not used and causes a build error due to native library duplication.",
                    "Remove",
                    MessageType.Error))
                    Utils.RemovePackage(Utils.legacyUnityAdsPackageName);
            }

            if (platform == BuildTarget.Android)
            {
#if UNITY_2019_3_OR_NEWER
                OnGradleTemplateDisabledGUI("Main Gradle", Utils.mainGradlePath);
                OnGradleTemplateDisabledGUI("Gradle Properties", Utils.propertiesGradlePath);
#else
                OnGradleTemplateDisabledGUI("Gradle", Utils.mainGradlePath);
#endif
#if UNITY_2022_2_OR_NEWER
                OnGradleTemplateDisabledGUI("Settings Gradle", Utils.settingsGradlePath);
#endif
#if UNITY_2019_3_OR_NEWER && !UNITY_2022_2_OR_NEWER
                OnGradleTemplateDisabledGUI("Base Gradle", Utils.projectGradlePath);
#endif
            }
            else if (platform == BuildTarget.iOS)
            {
                if (PlayerSettings.muteOtherAudioSources)
                {
                    if (HelpStyles.WarningWithButton("Known issue with muted all sounds in Unity Game " +
                        "after closing interstitial ads when 'Mute Other AudioSources' option enabled in PlayerSettings.",
                        "Disable"))
                        PlayerSettings.muteOtherAudioSources = false;
                }
            }
        }

        private void OnEDMAreaGUI()
        {
            if (platform == BuildTarget.Android)
            {
                if (edmVersion == null)
                    return;
                if (HelpStyles.WarningWithButton(
                    "Changes to solutions/adapters must be resolved in project dependencies.",
                    "Resolve", DependencyManager.isDirt ? MessageType.Warning : MessageType.None))
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        DependencyManager.isDirt = false;
                        var succses = Utils.TryResolveAndroidDependencies();
                        EditorUtility.DisplayDialog("Android Dependencies",
                            succses ? "Resolution Succeeded" : "Resolution Failed! See the log for details.",
                            "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Android Dependencies",
                            "Resolution Failed! Android resolver not enabled. Unity Android platform target must be selected.",
                            "OK");
                    }
                }
            }
        }

        private void OnGradleTemplateDisabledGUI(string prefix, string path)
        {
            if (File.Exists(path))
                return;
#if UNITY_ANDROID || CASDeveloper
            var msg = prefix + " template feature is disabled!\n" +
                "A successful build requires do modifications to " + prefix + " template.";
            if (HelpStyles.WarningWithButton(msg, "Enable", MessageType.Error))
                CASPreprocessGradle.TryEnableGradleTemplate(path);
#endif
        }

        private void OnManagerIDVerificationGUI()
        {
            if (managerIdsProp.arraySize == 0)
                EditorGUILayout.HelpBox("Build is not supported without a manager ID.", MessageType.Error);
            else if (string.IsNullOrEmpty(managerIdsProp.GetArrayElementAtIndex(0).stringValue))
                EditorGUILayout.HelpBox("The ID of the first manager cannot be empty!", MessageType.Error);
            else
                return;
            EditorGUILayout.HelpBox("If you haven't created an CAS account and registered an manager yet, " +
                "now's a great time to do so at cleveradssolutions.com. " +
                "If you're just looking to experiment with the SDK, though, " +
                "you can use the Test Ad Mode below with any manager ID.",
                MessageType.Info);
        }

        private void OnAudienceGUI()
        {
            var targetAudience = (Audience)EditorGUILayout.EnumPopup("Audience Tagged",
                (Audience)audienceTaggedProp.enumValueIndex);
            if (audienceTaggedProp.enumValueIndex != (int)targetAudience)
            {
                if (dependencyManager != null)
                    dependencyManager.SetAudience(targetAudience);
                audienceTaggedProp.enumValueIndex = (int)targetAudience;
            }

            EditorGUI.indentLevel++;
            switch (targetAudience)
            {
                case Audience.Mixed:
                    EditorGUILayout.HelpBox("Game target age groups include both children and older audiences.\n" +
                        "A neutral age screen must be implemented so that any ads not suitable " +
                        "for children are only shown to older audiences.\n" +
                        "You could change the audience at runtime.",
                        MessageType.None);
                    break;
                case Audience.Children:
                    EditorGUILayout.HelpBox("Audiences under the age of 13 who subject of COPPA.",
                        MessageType.None);
                    break;
                case Audience.NotChildren:
                    EditorGUILayout.HelpBox("Audiences over the age of 13 NOT subject to the restrictions of child protection laws.",
                        MessageType.None);
                    break;
            }
            EditorGUI.indentLevel--;
        }

        private void OnLoadingModeGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(loadingModeProp, HelpStyles.GetContent("Loading mode", null));
            HelpStyles.HelpButton(Utils.gitUnityRepoURL + "/wiki/Other-Options#loading-mode");
            EditorGUILayout.EndHorizontal();
        }

        private void OnEditroRuntimeActiveAdGUI()
        {
            if (editorRuntimeActiveAdFlags > -1)
            {
                DrawSeparator();
                EditorGUI.BeginChangeCheck();
                editorRuntimeActiveAdFlags = Convert.ToInt32(
                    EditorGUILayout.EnumFlagsField("Editor runtime Active ad", (AdFlags)editorRuntimeActiveAdFlags));
                if (EditorGUI.EndChangeCheck())
                    PlayerPrefs.SetInt(Utils.editorRuntimeActiveAdPrefs, editorRuntimeActiveAdFlags);
            }
        }
    }
}
#pragma warning restore 649