//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || ( CASDeveloper && UNITY_EDITOR )
#define PlatformAndroid
#endif
#if UNITY_IOS || ( CASDeveloper && UNITY_EDITOR )
#define PlatformIOS
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS
{
    internal delegate void ManagerStateChanges(int index, CASManagerBase manager);

    internal interface IAppStateEventClient
    {
        event Action OnApplicationBackground;
        event Action OnApplicationForeground;
    }

    internal static class CASFactory
    {
        private static IAppStateEventClient appStateEventClient;
        private static IAdsSettings settings;
        private static List<CASManagerBase> managers;

        internal static event ManagerStateChanges OnManagerStateChanged;
        internal static ConsentFlow.Status consentFlowStatus = ConsentFlow.Status.Obtained;


#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload()
        {
            // Read more aboud Domain Reloading in Unity Editor
            // https://docs.unity3d.com/2023.3/Documentation/Manual/DomainReloading.html
            appStateEventClient = null;
            settings = null;
            managers = null;
            OnManagerStateChanged = null;
            consentFlowStatus = ConsentFlow.Status.Obtained;
        }
#endif

        internal static bool isDebug { get { return GetAdsSettings().isDebugMode; } }

        internal static IMediationManager GetMainManagerOrNull()
        {
            return managers == null || managers.Count == 0 ? null : managers[0];
        }

        private static CASInitSettings LoadInitSettingsFromResources()
        {
#if UNITY_ANDROID
            return Resources.Load<CASInitSettings>("CASSettingsAndroid");
#elif UNITY_IOS
            return Resources.Load<CASInitSettings>("CASSettingsiOS");
#else
            return null;
#endif
        }

        private static IAdsSettings CreateSettigns(CASInitSettings initSettings)
        {
            IAdsSettings settings = null;
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
                settings = new CAS.Android.CASSettingsClient();
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                settings = new CAS.iOS.CASSettingsClient();
#endif
            if (settings == null)
                settings = new CAS.Unity.CASSettingsClient();

            if (initSettings)
            {
                settings.isDebugMode = initSettings.defaultDebugModeEnabled;
                settings.loadingMode = initSettings.defaultLoadingMode;
                settings.bannerRefreshInterval = initSettings.defaultBannerRefresh;
                settings.interstitialInterval = initSettings.defaultInterstitialInterval;
                settings.taggedAudience = initSettings.defaultAudienceTagged;
                settings.allowInterstitialAdsWhenVideoCostAreLower = initSettings.defaultInterstitialWhenNoRewardedAd;
                ((ITargetingOptions)settings).locationCollectionEnabled = initSettings.defaultLocationCollectionEnabled;
            }
            return settings;
        }

        internal static CASInitSettings LoadDefaultBuiderFromResources()
        {
            var builder = LoadInitSettingsFromResources();
            if (!builder)
            {
#if UNITY_ANDROID || UNITY_IOS
                Debug.LogWarning("[CAS] No settings asset have been created for the target platform yet." +
                    "\nUse 'Assets > CleverAdsSolutions > Settings' menu to create and modify default settings for each native platform.");
#else
                Debug.LogError( "[CAS] The target platform is not supported." +
                    "\nChoose the target Android or iOS to use CAS." );
#endif
                builder = ScriptableObject.CreateInstance<CASInitSettings>();
                builder.allowedAdFlags = AdFlags.Everything;
                return builder;
            }
            return UnityEngine.Object.Instantiate(builder);
        }

        internal static IAppStateEventClient GetAppStateEventClient()
        {
            if (appStateEventClient == null)
            {
#if PlatformAndroid
                if (Application.platform == RuntimePlatform.Android)
                {
                    appStateEventClient = new CAS.Android.CASAppStateEventClient();
                    return appStateEventClient;
                }
#endif
#if UNITY_EDITOR || PlatformIOS
                appStateEventClient = CAS.Unity.CASAppStateEventClient.Create();
#endif
            }
            return appStateEventClient;
        }

        internal static IAdsSettings GetAdsSettings()
        {
            if (settings == null)
                settings = CreateSettigns(LoadInitSettingsFromResources());
            return settings;
        }

        internal static ITargetingOptions GetTargetingOptions()
        {
            return (ITargetingOptions)GetAdsSettings();
        }

        internal static string GetSDKVersion()
        {
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettingsClient;
                return androidSettings.GetSDKVersion();
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return CAS.iOS.CASExterns.CASUGetSDKVersion();
#endif
            return MobileAds.wrapperVersion;
        }

        internal static IMediationManager CreateManager(CASInitSettings initSettings)
        {
            if (managers != null)
            {
                for (int i = 0; i < managers.Count; i++)
                {
                    var readyManager = managers[i];
                    if (readyManager != null && readyManager.managerID == initSettings.targetId)
                    {
                        var initialConfig = readyManager.initialConfig;
                        if (initialConfig != null)
                        {
                            if (initSettings.initListener != null)
                                initSettings.initListener(initialConfig);
                            if (initSettings.initListenerDeprecated != null)
                                initSettings.initListenerDeprecated(initialConfig.error == null, initialConfig.error);
                        }
                        else
                        {
                            readyManager.initCompleteEvent += initSettings.initListener;
                            readyManager.initCompleteAction += initSettings.initListenerDeprecated;
                        }
                        return readyManager;
                    }
                }
            }
            else
            {
                managers = new List<CASManagerBase>(initSettings.managersCount);
                for (int i = 0; i < initSettings.managersCount; i++)
                    managers.Add(null);
            }

            if (settings == null)
                settings = CreateSettigns(initSettings);

            CASManagerBase manager = null;
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
                manager = new CAS.Android.CASManagerClient();
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                manager = new CAS.iOS.CASManagerClient();
#endif
#if UNITY_EDITOR
            manager = new CAS.Unity.CASManagerClient();
#endif
            if (manager == null)
                throw new NotSupportedException("Platform: " + Application.platform.ToString());

            manager.Init(initSettings);

            var managerIndex = initSettings.IndexOfManagerId(manager.managerID);
            if (managerIndex < 0)
                managers.Add(manager);
            else
                managers[managerIndex] = manager;

            if (OnManagerStateChanged != null && managerIndex >= 0)
                OnManagerStateChanged(managerIndex, manager);
            return manager;
        }

        internal static bool TryGetManagerByIndexAsync(int index, ManagerStateChanges managerStateHandler)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Manager index cannot be less than 0");

            if (managers != null && index < managers.Count)
            {
                var readyManager = managers[index];
                if (readyManager != null)
                {
                    managerStateHandler(index, readyManager);
                    return true;
                }
            }

            OnManagerStateChanged += managerStateHandler;
            return false;
        }

        internal static void OnManagerInitialized(CASManagerBase manager)
        {
            UnityLog("Initialized ads with id: " + manager.managerID);

            if (OnManagerStateChanged != null)
            {
                var managerIndex = managers.IndexOf(manager);
                if (managerIndex >= 0)
                    OnManagerStateChanged(managerIndex, manager);
            }
        }

        internal static void ValidateIntegration()
        {
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettingsClient;
                androidSettings.ValidateIntegration();
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExterns.CASUValidateIntegration();
            }
#endif
        }

        internal static string GetActiveMediationPattern()
        {
#if UNITY_EDITOR
            try
            {
                return (string)Type.GetType("CAS.UEditor.DependencyManager, CleverAdsSolutions-Editor", true)
                    .GetMethod("GetActiveMediationPattern",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[0], null)
                    .Invoke(null, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettingsClient;
                return androidSettings.GetActiveMediationPattern();
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return CAS.iOS.CASExterns.CASUGetActiveMediationPattern();
#endif
            return "";
        }

        internal static AdNetwork[] GetActiveNetworks()
        {
            var pattern = GetActiveMediationPattern();
            var result = new List<AdNetwork>();
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] != '0')
                    result.Add((AdNetwork)i);
            }
            return result.ToArray();
        }

        internal static bool IsActiveNetwork(AdNetwork network)
        {
#if UNITY_EDITOR
            var pattern = GetActiveMediationPattern();
            if ((int)network < pattern.Length)
                return pattern[(int)network] != '0';
#endif
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettingsClient;
                return androidSettings.IsActiveMediationNetwork(network);
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return CAS.iOS.CASExterns.CASUIsActiveMediationNetwork((int)network);
#endif
            return false;
        }

        internal static void ShowConsentFlow(ConsentFlow flow, bool ifRequired)
        {
            var initSettings = LoadInitSettingsFromResources();
            var forceTesting = initSettings && initSettings.IsTestAdMode();
            if (settings == null)
                settings = CreateSettigns(initSettings);
#if UNITY_EDITOR
            UnityLog("Show Consent flow has been called but not supported in Unity Editor.");
            HandleConsentFlow(flow, ConsentFlow.Status.Obtained);
#endif
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var androidFlow = new CAS.Android.CASConsentFlowClient(flow, forceTesting))
                {
                    androidFlow.Show(ifRequired);
                }
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExternCallbacks.ConsentFlow(flow);
                CAS.iOS.CASExterns.CASUShowConsentFlow(
                    ifRequired,
                    forceTesting,
                    (int)flow.debugGeography,
                    flow.privacyPolicyUrl,
                    CAS.iOS.CASExternCallbacks.ConsentFlowCompletion
                );
            }
#endif
        }

        internal static void HandleConsentFlow(ConsentFlow flow, ConsentFlow.Status status)
        {
            UnityLog("Callback Consent Flow status " + status.ToString());
            consentFlowStatus = status;
            if (flow == null) return;
            try
            {
                if (flow.OnResult != null)
                    flow.OnResult(status);
                if (flow.OnCompleted != null)
                    flow.OnCompleted();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void UnityLog(string message)
        {
            if (settings != null && settings.isDebugMode)
                Debug.Log("[CAS.AI] " + message);
        }

        internal static void RuntimeLog(int adType, string message)
        {
#if !UNITY_EDITOR || CASDeveloper
            if (settings != null && settings.isDebugMode)
                Debug.Log("[CAS.AI] " + ((AdType)adType).ToString() + " " + message);
#endif
        }

        internal static void RuntimeLog(AdSize adSize, string message)
        {
#if !UNITY_EDITOR || CASDeveloper
            if (settings != null && settings.isDebugMode)
                Debug.Log("[CAS.AI] " + adSize.ToString() + " " + message);
#endif
        }
    }
}