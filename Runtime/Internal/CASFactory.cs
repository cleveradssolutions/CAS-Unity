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
    internal interface IInternalManager : IMediationManager
    {
        InitialConfiguration initialConfig { get; }
        void HandleInitEvent(CASInitCompleteEvent initEvent, InitCompleteAction initAction);
    }

    internal interface IInternalAdObject
    {
        void OnManagerReady(InitialConfiguration config);
    }

    internal static class CASFactory
    {
        private static volatile bool executeEventsOnUnityThread = true;

        private static IAdsSettings settings;
        private static List<IInternalManager> managers;
        private static List<List<IInternalAdObject>> initCallback = new List<List<IInternalAdObject>>();
        private static Dictionary<string, string> globalExtras;

#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void DomainReloading()
        {
            // Read more aboud Domain Reloading in Unity Editor
            // https://docs.unity3d.com/2023.3/Documentation/Manual/DomainReloading.html
            executeEventsOnUnityThread = true;
            settings = null;
            managers = null;
            initCallback = new List<List<IInternalAdObject>>();
            globalExtras = null;
        }
#endif

        internal static bool isDebug { get { return GetAdsSettings().isDebugMode; } }

        internal static IMediationManager GetMainManagerOrNull()
        {
            return managers == null || managers.Count < 1 ? null : managers[0];
        }

        internal static void SetGlobalMediationExtras(Dictionary<string, string> extras)
        {
            globalExtras = extras;
        }

        internal static bool IsExecuteEventsOnUnityThread()
        {
            return executeEventsOnUnityThread;
        }

        internal static void SetExecuteEventsOnUnityThread(bool enable)
        {
            executeEventsOnUnityThread = enable;
            if (enable)
                EventExecutor.Initialize();
        }

        internal static CASInitSettings LoadInitSettingsFromResources()
        {
#if UNITY_ANDROID
            return Resources.Load<CASInitSettings>("CASSettingsAndroid");
#elif UNITY_IOS
            return Resources.Load<CASInitSettings>("CASSettingsiOS");
#else
            return null;
#endif
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

        internal static IAdsSettings CreateSettigns(CASInitSettings initSettings)
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
            }
            return settings;
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
                        readyManager.HandleInitEvent(initSettings.initListener, initSettings.initListenerDeprecated);
                        return readyManager;
                    }
                }
            }
            else
            {
                managers = new List<IInternalManager>(initSettings.managersCount);
                for (int i = 0; i < initSettings.managersCount; i++)
                    managers.Add(null);
            }

            if (settings == null)
                settings = CreateSettigns(initSettings);

            if (initSettings.extras == null)
            {
                initSettings.extras = globalExtras;
            }
            else if (globalExtras != null)
            {
                var mergeExtras = new Dictionary<string, string>(globalExtras);
                foreach (var extra in initSettings.extras)
                    mergeExtras[extra.Key] = extra.Value;
                initSettings.extras = mergeExtras;
            }

            IInternalManager manager = null;
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
                manager = new CAS.Android.CASManagerClient().Init(initSettings);
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                manager = new CAS.iOS.CASManagerClient().Init(initSettings);
#endif
#if UNITY_EDITOR
            manager = CAS.Unity.CASManagerClient.Create(initSettings);
#endif
            if (manager == null)
                throw new NotSupportedException("Platform: " + Application.platform.ToString());

            if (executeEventsOnUnityThread)
                EventExecutor.Initialize();

            var managerIndex = initSettings.IndexOfManagerId(manager.managerID);
            if (managerIndex < 0)
                managers.Add(manager);
            else
                managers[managerIndex] = manager;
            return manager;
        }

        internal static bool TryGetManagerByIndexAsync(IInternalAdObject adObject, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Manager index cannot be less than 0");

            if (managers != null && index < managers.Count)
            {
                var readyManager = managers[index];
                if (readyManager != null && readyManager.initialConfig != null)
                {
                    adObject.OnManagerReady(readyManager.initialConfig);
                    return true;
                }
            }

            for (int i = initCallback.Count; i <= index; i++)
                initCallback.Add(null);
            if (initCallback[index] == null)
                initCallback[index] = new List<IInternalAdObject>();

            initCallback[index].Add(adObject);
            return false;
        }

        internal static void UnsubscribeReadyManagerAsync(IInternalAdObject callback, int index)
        {
            if (index < initCallback.Count && initCallback[index] != null)
                initCallback[index].Remove(callback);
        }

        internal static void OnManagerInitialized(IInternalManager manager)
        {
            if (managers == null) return;
            var managerIndex = managers.IndexOf(manager);
            if (managerIndex != -1 && managerIndex < initCallback.Count)
            {
                var initList = initCallback[managerIndex];
                if (initList == null)
                    return;
                initCallback[managerIndex] = null;
                for (int i = 0; i < initList.Count; i++)
                {
                    try
                    {
                        // Check out MonoBehaviour which is still alive
                        if ((MonoBehaviour)initList[i])
                            initList[i].OnManagerReady(manager.initialConfig);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
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

        #region Mediation network states
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

        internal static void ShowConsentFlow(ConsentFlow flow)
        {
#if UNITY_EDITOR
            UnityLog("Show Consent flow has been called but not supported in Unity Editor.");
#endif
#if PlatformAndroid
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var androidFlow = new CAS.Android.CASConsentFlowClient(flow))
                {
                    androidFlow.show();
                }
            }
#endif
#if PlatformIOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExternCallbacks.consentFlowComplete += flow.OnCompleted;
                CAS.iOS.CASExterns.CASUShowConsentFlow(
                    flow.isEnabled,
                    flow.privacyPolicyUrl,
                    CAS.iOS.CASExternCallbacks.OnConsentFlowCompletion
                );
            }
#endif
        }
        #endregion

        internal static void ExecuteEvent(Action action)
        {
            if (action == null)
                return;
            if (executeEventsOnUnityThread)
            {
                EventExecutor.Add(action);
                return;
            }
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void UnityLog(string message)
        {
            if (GetAdsSettings().isDebugMode)
                Debug.Log("[CAS.AI] " + message);
        }

        internal static void UnityLogException(Exception e)
        {
#if CASDeveloper
            Debug.LogException(e);
#endif
        }
    }
}