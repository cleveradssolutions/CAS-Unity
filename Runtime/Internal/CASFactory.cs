//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//
#if UNITY_ANDROID || ( CASDeveloper && UNITY_EDITOR )
#define PlatformAndroid
#endif
#if UNITY_IOS || ( CASDeveloper && UNITY_EDITOR )
#define PlatformIOS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CAS
{
    internal static class CASFactory
    {
        private static volatile bool executeEventsOnUnityThread = true;

        private static IAdsSettings settings;
        private static List<IMediationManager> managers;
        private static List<Action<IMediationManager>> initCallback = new List<Action<IMediationManager>>();
        private static Dictionary<string, string> globalExtras;

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
            return Resources.Load<CASInitSettings>( "CASSettingsiOS" );
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
            if (managers == null)
            {
                if (initSettings.managersCount == 0)
                {
                    managers = new List<IMediationManager>();
                }
                else
                {
                    managers = new List<IMediationManager>(initSettings.managersCount);
                    for (int i = 0; i < initSettings.managersCount; i++)
                        managers.Add(null);
                }
            }
            else
            {
                for (int i = 0; i < managers.Count; i++)
                {
                    var readyManager = managers[i];
                    if (readyManager != null && readyManager.managerID == initSettings.targetId)
                    {
                        if (initSettings.initListener != null)
                            initSettings.initListener(true, null);
                        return readyManager;
                    }
                }
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

            IMediationManager manager = null;
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

            var managerIndex = initSettings.IndexOfManagerId(initSettings.targetId);
            if (managerIndex < 0)
            {
                managerIndex = managers.Count;
                managers.Add(manager);
            }
            else
            {
                managers[managerIndex] = manager;
            }
            if (managerIndex < initCallback.Count)
            {
                var onInitManager = initCallback[managerIndex];
                if (onInitManager != null)
                {
                    initCallback[managerIndex] = null;
                    try
                    {
                        onInitManager(manager);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            return manager;
        }

        internal static bool TryGetManagerByIndexAsync(Action<IMediationManager> callback, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Manager index cannot be less than 0");

            if (managers != null && index < managers.Count && managers[index] != null)
            {
                callback(managers[index]);
                return true;
            }

            if (index != 0)
            {
                var initSettings = LoadInitSettingsFromResources();
                if (initSettings && initSettings.managersCount > 0 && initSettings.managersCount - 1 < index)
                    throw new ArgumentOutOfRangeException("index",
                        "Manager with index " + index + " not found in settings." +
                        "\nUse 'Assets > CleverAdsSolutions > Settings' menu to set all used Manager Ids.");
            }
            for (int i = initCallback.Count; i <= index; i++)
                initCallback.Add(null);
            initCallback[index] += callback;
            return false;
        }

        internal static void UnsubscribeReadyManagerAsync(Action<IMediationManager> callback, int index)
        {
            if (initCallback != null && index < initCallback.Count)
                initCallback[index] -= callback;
        }

        internal static void ValidateIntegration()
        {
#if UNITY_EDITOR
            // TODO: Implementation editor validation
            if (Application.isEditor)
                return;
#endif
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
#if UNITY_IOS
                Debug.Log( "[CAS:Unity] " + message );
#else
                Debug.Log("[CAS:] " + message);
#endif
        }

        internal static void UnityLogException(Exception e)
        {
#if CASDeveloper
            Debug.LogException(e);
#endif
        }
    }
}