using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CAS
{
    internal static class CASFactory
    {
        private static volatile bool executeEventsOnUnityThread = false;

        private static IAdsSettings settings;
        private static List<IMediationManager> managers;
        private static List<Action<IMediationManager>> initCallback = new List<Action<IMediationManager>>();
        private static Dictionary<string, string> globalExtras;

        internal static bool isDebug { get { return GetAdsSettings().isDebugMode; } }

        internal static IMediationManager GetMainManagerOrNull()
        {
            return managers == null || managers.Count < 1 ? null : managers[0];
        }

        internal static void SetGlobalMediationExtras( Dictionary<string, string> extras )
        {
            globalExtras = extras;
        }

        internal static bool IsExecuteEventsOnUnityThread()
        {
            return executeEventsOnUnityThread;
        }

        internal static void SetExecuteEventsOnUnityThread( bool enable )
        {
            executeEventsOnUnityThread = enable;
            if (enable)
                EventExecutor.Initialize();
        }

        internal static CASInitSettings LoadInitSettingsFromResources()
        {
#if UNITY_ANDROID
            return Resources.Load<CASInitSettings>( "CASSettingsAndroid" );
#elif UNITY_IOS
            return Resources.Load<CASInitSettings>( "CASSettingsiOS" );
#else
            return null;
#endif
        }

        internal static IAdsSettings CreateSettigns( CASInitSettings initSettings )
        {
            IAdsSettings settings = null;
#if !TARGET_OS_SIMULATOR
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
                settings = new CAS.Android.CASSettings();
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                settings = new CAS.iOS.CASSettings();
#endif
#endif
            if (settings == null)
                settings = new CAS.Unity.CASSettings();

            if (initSettings)
            {
                settings.isDebugMode = initSettings.defaultDebugModeEnabled;
                settings.loadingMode = initSettings.defaultLoadingMode;
                settings.bannerRefreshInterval = initSettings.defaultBannerRefresh;
                settings.interstitialInterval = initSettings.defaultInterstitialInterval;
                settings.taggedAudience = initSettings.defaultAudienceTagged;
                settings.analyticsCollectionEnabled = initSettings.defaultAnalyticsCollectionEnabled;
                settings.allowInterstitialAdsWhenVideoCostAreLower = initSettings.defaultInterstitialWhenNoRewardedAd;
            }
            return settings;
        }

        internal static IAdsSettings GetAdsSettings()
        {
            if (settings == null)
                settings = CreateSettigns( LoadInitSettingsFromResources() );
            return settings;
        }

        internal static ITargetingOptions GetTargetingOptions()
        {
            return ( ITargetingOptions )GetAdsSettings();
        }

        internal static string GetSDKVersion()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettings;
                return androidSettings.GetSDKVersion();
            }
#elif UNITY_IOS && !TARGET_OS_SIMULATOR
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return CAS.iOS.CASExterns.CASUGetSDKVersion();
#endif
            return MobileAds.wrapperVersion;
        }

        internal static IMediationManager CreateManager( CASInitSettings initSettings )
        {
            if (managers == null)
            {
                if (initSettings.managersCount == 0)
                {
                    managers = new List<IMediationManager>();
                }
                else
                {
                    managers = new List<IMediationManager>( initSettings.managersCount );
                    for (int i = 0; i < initSettings.managersCount; i++)
                        managers.Add( null );
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
                            initSettings.initListener( true, null );
                        return readyManager;
                    }
                }
            }

            if (settings == null)
                settings = CreateSettigns( initSettings );

            if (initSettings.extras == null)
            {
                initSettings.extras = globalExtras;
            }
            else if (globalExtras != null)
            {
                var mergeExtras = new Dictionary<string, string>( globalExtras );
                foreach (var extra in initSettings.extras)
                    mergeExtras[extra.Key] = extra.Value;
                initSettings.extras = mergeExtras;
            }

            IMediationManager manager = null;
#if UNITY_EDITOR || TARGET_OS_SIMULATOR
            manager = CAS.Unity.CASMediationManager.CreateManager( initSettings );
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var android = new CAS.Android.CASMediationManager( initSettings );
                android.CreateManager( initSettings );
                manager = android;
            }
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var ios = new CAS.iOS.CASMediationManager( initSettings );
                ios.CreateManager( initSettings );
                manager = ios;
            }
#endif
            if (manager == null)
                throw new NotSupportedException( "Current platform: " + Application.platform.ToString() );

#pragma warning disable CS0618 // Type or member is obsolete
            if (initSettings.bannerSize != 0) // Before onInitManager callback
                manager.bannerSize = initSettings.bannerSize;
#pragma warning restore CS0618 // Type or member is obsolete

            var managerIndex = initSettings.IndexOfManagerId( initSettings.targetId );
            if (managerIndex < 0)
            {
                managerIndex = managers.Count;
                managers.Add( manager );
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
                        onInitManager( manager );
                    }
                    catch (Exception e)
                    {
                        Debug.LogException( e );
                    }
                }
            }
            return manager;
        }

        internal static bool TryGetManagerByIndexAsync( Action<IMediationManager> callback, int index )
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException( "index", "Manager index cannot be less than 0" );

            if (managers != null && index < managers.Count && managers[index] != null)
            {
                callback( managers[index] );
                return true;
            }

            if (index != 0)
            {
                var initSettings = LoadInitSettingsFromResources();
                if (initSettings && initSettings.managersCount > 0 && initSettings.managersCount - 1 < index)
                    throw new ArgumentOutOfRangeException( "index",
                        "Manager with index " + index + " not found in settings." );
            }
            for (int i = initCallback.Count; i <= index; i++)
                initCallback.Add( null );
            initCallback[index] += callback;
            return false;
        }

        internal static void UnsubscribeReadyManagerAsync( Action<IMediationManager> callback, int index )
        {
            if (initCallback != null && index < initCallback.Count)
                initCallback[index] -= callback;
        }

        internal static void ValidateIntegration()
        {
#if UNITY_EDITOR
            // TODO: Implementation editor 
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettings;
                androidSettings.ValidateIntegration();
            }
#elif UNITY_IOS
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
                return ( string )Type.GetType( "CAS.UEditor.DependencyManager, CleverAdsSolutions-Editor", true )
                    .GetMethod( "GetActiveMediationPattern",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[0], null )
                    .Invoke( null, null );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettings;
                return androidSettings.GetActiveMediationPattern();
            }
#elif UNITY_IOS
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
                    result.Add( ( AdNetwork )i );
            }
            return result.ToArray();
        }

        internal static bool IsActiveNetwork( AdNetwork network )
        {
#if UNITY_EDITOR
            var pattern = GetActiveMediationPattern();
            if (( int )network < pattern.Length)
                return pattern[( int )network] != '0';
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = GetAdsSettings() as CAS.Android.CASSettings;
                return androidSettings.IsActiveMediationNetwork( network );
            }
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return CAS.iOS.CASExterns.CASUIsActiveMediationNetwork( ( int )network );
#endif
            return false;
        }
        #endregion

        internal static void ExecuteEvent( Action action )
        {
            if (action == null)
                return;
            if (executeEventsOnUnityThread)
            {
                EventExecutor.Add( action );
                return;
            }
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        internal static void UnityLog( string message )
        {
            if (GetAdsSettings().isDebugMode)
                Debug.Log( "[CleverAdsSolutions] " + message );
        }
    }
}