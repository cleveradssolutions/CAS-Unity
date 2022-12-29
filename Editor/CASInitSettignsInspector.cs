//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#pragma warning disable 649
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Utils = CAS.UEditor.CASEditorUtils;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor.AnimatedValues;

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
        private SerializedProperty multiDexEnabledProp;
        private SerializedProperty updateGradlePluginVersionProp;
        private SerializedProperty permissionAdIdRemovedProp;
        private SerializedProperty mostPopularCountryOfUsersProp;
        private SerializedProperty attributionReportEndpointProp;
        private SerializedProperty userTrackingUsageDescriptionProp;
        #endregion

        #region Utility fields
        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private ReorderableList userTrackingList;
        private BuildTarget platform;
        private bool allowedPackageUpdate;
        private string newCASVersion = null;
        private bool deprecateDependenciesExist;
        private Version edmVersion;
        private bool edmRequiredNewer = false;
        private string environmentDetails;

        private string[] deprecatedAssets = null;

        private bool adsManagerExist = false;
        private int editorRuntimeActiveAdFlags;

        private AnimBool iOSLocationDescriptionFoldout = null;
        private AnimBool otherSettingsFoldout = null;
        #endregion

        #region Initialize logic
        private void OnEnable()
        {
            SetSettingsPlatform();

            InitMainProperties(serializedObject);
            InitEditorSettingsProperties();

            iOSLocationDescriptionFoldout = new AnimBool(false, Repaint);
            otherSettingsFoldout = new AnimBool(false, Repaint);

            allowedPackageUpdate = Utils.IsPackageExist(Utils.packageName);

            dependencyManager = DependencyManager.Create(platform, (Audience)audienceTaggedProp.enumValueIndex, true);

            HandleDeprecatedComponents();
            InitEDM4U();
            InitEnvironmentDetails();

            EditorApplication.delayCall += () =>
            {
                newCASVersion = Utils.GetNewVersionOrNull(Utils.gitUnityRepo, MobileAds.wrapperVersion, false);
            };
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
            buildPreprocessEnabledProp = editorSettingsObj.FindProperty("buildPreprocessEnabled");
            updateGradlePluginVersionProp = editorSettingsObj.FindProperty("updateGradlePluginVersion");
            multiDexEnabledProp = editorSettingsObj.FindProperty("multiDexEnabled");
            permissionAdIdRemovedProp = editorSettingsObj.FindProperty("permissionAdIdRemoved");

            mostPopularCountryOfUsersProp = editorSettingsObj.FindProperty("mostPopularCountryOfUsers");
            attributionReportEndpointProp = editorSettingsObj.FindProperty("attributionReportEndpoint");

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
            else
                platform = BuildTarget.NoTarget;
        }

        private void HandleDeprecatedComponents()
        {
            deprecatedAssets = new string[]{
                Utils.GetDeprecateDependencyName( Utils.generalDeprecateDependency, platform ),
                Utils.GetDeprecateDependencyName( Utils.teenDeprecateDependency, platform ),
                Utils.GetDeprecateDependencyName( Utils.promoDeprecateDependency, platform ),
                Utils.GetDependencyName( "Additional", platform )
            };

            for (int i = 0; i < deprecatedAssets.Length; i++)
            {
                if (deprecateDependenciesExist |= AssetDatabase.FindAssets(deprecatedAssets[i]).Length > 0)
                    break;
            }
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
            GUILayout.Label("CleverAdsSolutions", HelpStyles.largeTitleStyle);
            GUILayout.Label(platform.ToString(), HelpStyles.largeTitleStyle, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            Utils.OnHeaderGUI(Utils.gitUnityRepo, allowedPackageUpdate, MobileAds.wrapperVersion, ref newCASVersion);
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
            DeprecatedDependenciesGUI();

            if (dependencyManager == null)
            {
                EditorGUILayout.HelpBox("The integrity of CAS Unity package is broken. " +
                    "Please try to reimport the package or contact support.", MessageType.Error);
            }
            else
            {
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
            EditorGUI.LabelField(rect, "Manager ID's " + (platform == BuildTarget.iOS ? "(iTunes ID)" : "(Bundle ID)"));
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

        private void OnAndroidAdIdGUI()
        {
            if (platform != BuildTarget.Android)
                return;
            permissionAdIdRemovedProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Remove permission to use Advertising ID (AD_ID)",
                    permissionAdIdRemovedProp.boolValue);
        }

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

            OnLoadingModeGUI();

            debugModeProp.boolValue = EditorGUILayout.ToggleLeft(
                HelpStyles.GetContent("Verbose Debug logging", null,
                   "The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdk with tag CAS. " +
                   "Disabling the Debug Mode may improve application performance."),
                debugModeProp.boolValue);

            buildPreprocessEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Build preprocess enabled",
                    buildPreprocessEnabledProp.boolValue);

            if (!buildPreprocessEnabledProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Automatic configuration at build time is disabled.\n" +
                    "You can use `Assets > CleverAdsSolutions > Configure project` to call configuration manually.",
                    MessageType.None);
                EditorGUI.indentLevel--;
            }

            if (platform == BuildTarget.Android)
            {
                EditorGUI.indentLevel++;
                updateGradlePluginVersionProp.boolValue = EditorGUILayout.ToggleLeft(
                   HelpStyles.GetContent("Update Gradle Plugin enabled", null,
                       "The Gradle plugin version will be updated during build to be optimal " +
                       "for the current Gradle Wrapper version."),
                    updateGradlePluginVersionProp.boolValue);

                EditorGUILayout.BeginHorizontal();
                multiDexEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Multi DEX enabled",
                    multiDexEnabledProp.boolValue);
                HelpStyles.HelpButton(Utils.gitUnityRepoURL + "/wiki/Include-Android#enable-multidex");
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
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

            autoCheckForUpdatesEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                "Auto check for CAS updates enabled",
                autoCheckForUpdatesEnabledProp.boolValue);

            delayAppMeasurementGADInitProp.boolValue = EditorGUILayout.ToggleLeft(
                "Delay measurement of the Google SDK initialization",
                delayAppMeasurementGADInitProp.boolValue);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Most popular country of users (ISO2)", GUILayout.ExpandWidth(false));
            EditorGUI.BeginChangeCheck();
            var countryCode = mostPopularCountryOfUsersProp.stringValue;
            countryCode = EditorGUILayout.TextField(countryCode, GUILayout.Width(25.0f));
            if (EditorGUI.EndChangeCheck())
            {
                if (countryCode.Length > 2)
                    countryCode = countryCode.Substring(0, 2);
                mostPopularCountryOfUsersProp.stringValue = countryCode.ToUpper();
            }
            EditorGUILayout.EndHorizontal();

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
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Please include the ad formats that you want to use in your game.", MessageType.Error);
                EditorGUI.indentLevel--;
                return true;
            }
            return false;
        }

        private void DrawTestAdMode()
        {
            testAdModeProp.boolValue = EditorGUILayout.ToggleLeft("Test ad mode", testAdModeProp.boolValue);
            EditorGUI.indentLevel++;
            if (testAdModeProp.boolValue)
                EditorGUILayout.HelpBox("Make sure you disable test ad mode and replace test manager ID with your own ad manager ID before publishing your app!", MessageType.Warning);
            else if (EditorUserBuildSettings.development)
                EditorGUILayout.HelpBox("Development build enabled, only test ads are allowed. " +
                    "\nMake sure you disable Development build and use real ad manager ID before publishing your app!", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("When testing your app, make sure you use Test Ads mode or Development Build. " +
                "Failure to do so can lead to suspension of your account.", MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawInterstitialScope()
        {
            EditorGUILayout.LabelField("Impression interval(sec):");
            EditorGUI.indentLevel++;
            interstitialIntervalProp.intValue = EditorGUILayout.IntSlider(interstitialIntervalProp.intValue, 0, 120);
            if (interstitialIntervalProp.intValue > 0)
                EditorGUILayout.HelpBox("For some time after the ad is closed, new ad impressions will fail.", MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawRewardedScope(bool allowInter)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!allowInter);
            interWhenNoRewardedAdProp.boolValue = EditorGUILayout.ToggleLeft(
               HelpStyles.GetContent("Increase filling by Interstitial ads", null,
               "Sometimes a situation occurs when filling Rewarded ads is not enough, " +
               "in this case, you can allow the display of Interstitial ads to receiving a reward in any case."),
                allowInter && interWhenNoRewardedAdProp.boolValue);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }

        private void DrawBannerScope()
        {
            EditorGUILayout.LabelField("Refresh rate(sec):");
            EditorGUI.indentLevel++;
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
            EditorGUI.indentLevel--;
        }

        private bool DrawAdFlagToggle(AdFlags flag)
        {
            var flagInt = (int)flag;
            var enabled = (allowedAdFlagsProp.intValue & flagInt) == flagInt;
            var icon = HelpStyles.GetFormatIcon(flag, enabled);
            var content = HelpStyles.GetContent("", icon);
            var toggleStyle = EditorStyles.toggle;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(content, EditorStyles.toolbar, GUILayout.ExpandWidth(false));
            var changed = GUILayout.Toggle(enabled, flag.ToString() + " placement included", EditorStyles.toolbarButton);
            changed = GUILayout.Toggle(changed, "", GUILayout.ExpandWidth(false));
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

        private void OnEDMAreaGUI()
        {
            if (edmVersion == null)
            {
                HelpStyles.BeginBoxScope();
                EditorGUILayout.HelpBox("In order to properly include third party dependencies in your project, " +
                    "an External Dependency Manager is required.", MessageType.Error);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("1. Download latest EDM4U.unitypackage", GUILayout.ExpandWidth(false));
                if (GUILayout.Button("here", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    Application.OpenURL(Utils.latestEMD4uURL);
                EditorGUILayout.EndHorizontal();
                GUILayout.Label("2. Import the EDM4U.unitypackage into your project.");
                HelpStyles.EndBoxScope();
                return;
            }
            if (edmRequiredNewer)
            {
                if (HelpStyles.WarningWithButton("To properly include third-party dependencies in your project, " +
                    "you need an External Dependency Manager version " + Utils.minEDM4UVersion + " or later.",
                    "Download"))
                    Application.OpenURL(Utils.latestEMD4uURL);
            }

            if (platform == BuildTarget.Android)
            {
#if UNITY_2019_3_OR_NEWER
                OnGradleTemplateDisabledGUI( "Main Gradle", Utils.mainGradlePath );
                OnGradleTemplateDisabledGUI( "Base Gradle", Utils.projectGradlePath );
                OnGradleTemplateDisabledGUI( "Launcher Gradle", Utils.launcherGradlePath );
                OnGradleTemplateDisabledGUI( "Gradle Properties", Utils.propertiesGradlePath );
#else
                OnGradleTemplateDisabledGUI("Gradle", Utils.mainGradlePath);
#endif

                if (HelpStyles.WarningWithButton("Changing dependencies will change the project settings. " +
                    "Please use Android Resolver after the change complete.", "Resolve", MessageType.Info))
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        var succses = Utils.TryResolveAndroidDependencies();
                        EditorUtility.DisplayDialog("Android Dependencies",
                            succses ? "Resolution Succeeded" : "Resolution Failed. See the log for details.",
                            "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Android Dependencies",
                            "Android resolver not enabled. Unity Android platform target must be selected.",
                            "OK");
                    }
                }
                return;
            }
            if (platform == BuildTarget.iOS)
            {
                if (PlayerSettings.muteOtherAudioSources)
                {
                    OnWarningGUI("Mute Other AudioSources enabled in PlayerSettings",
                        "Known issue with muted all sounds in Unity Game after closing interstitial ads. " +
                        "We recommend not using 'Mute Other AudioSources'.",
                        MessageType.Warning);
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

        private void OnWarningGUI(string title, string message, MessageType type)
        {
            HelpStyles.BeginBoxScope();
            EditorGUILayout.HelpBox(title, type);
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
            HelpStyles.EndBoxScope();
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
                    if (platform == BuildTarget.Android && !permissionAdIdRemovedProp.boolValue)
                    {
                        EditorGUILayout.HelpBox("Families Policy require that apps not use the Ad ID. " +
                            "We recommend that you remove the permission using the checkbox below.",
                            MessageType.Warning);
                    }
                    break;
                case Audience.NotChildren:
                    EditorGUILayout.HelpBox("Audiences over the age of 13 NOT subject to the restrictions of child protection laws.",
                        MessageType.None);
                    break;
            }
            OnAndroidAdIdGUI();
            OnIOSTrackLocationGUI();
            EditorGUI.indentLevel--;
        }

        private void OnIOSTrackLocationGUI()
        {
            if (platform != BuildTarget.iOS)
                return;

            trackLocationEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                "Location collection when user allowed", trackLocationEnabledProp.boolValue);
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

        private void DeprecatedDependenciesGUI()
        {
            if (!deprecateDependenciesExist)
                return;
            if (HelpStyles.WarningWithButton("Deprecated dependencies found. " +
                "Please remove them and use the new dependencies below.",
                "Remove", MessageType.Error))
            {
                for (int i = 0; i < deprecatedAssets.Length; i++)
                {
                    var assets = AssetDatabase.FindAssets(deprecatedAssets[i]);
                    for (int assetI = 0; assetI < assets.Length; assetI++)
                        AssetDatabase.MoveAssetToTrash(AssetDatabase.GUIDToAssetPath(assets[assetI]));
                }
                deprecateDependenciesExist = false;
            }
        }
    }
}
#pragma warning restore 649