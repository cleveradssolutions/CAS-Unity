using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CAS
{
    internal static class CASFactory
    {
        internal static volatile bool isExecuteEventsOnUnityThread = false;

        internal static IMediationManager manager;
        internal static IAdsSettings _settings;

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
                settings.isDebugMode = initSettings.debugMode;
                settings.loadingMode = initSettings.loadingMode;
                settings.bannerRefreshInterval = initSettings.bannerRefresh;
                settings.interstitialInterval = initSettings.interstitialInterval;
                settings.taggedAudience = initSettings.audienceTagged;
            }
            return settings;
        }

        internal static IAdsSettings GetAdsSettings()
        {
            if (_settings == null)
                _settings = CreateSettigns( LoadInitSettingsFromResources() );
            return _settings;
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
            if (manager != null && manager.managerID == initSettings.targetId)
            {
                if (initSettings.initListener != null)
                    initSettings.initListener( true, null );
                manager.bannerSize = initSettings.bannerSize;
                return manager;
            }
            if (_settings == null)
                _settings = CreateSettigns( initSettings );

            EventExecutor.Initialize();
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
            return manager;
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
            // TODO: Implementation editor 
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
                if (pattern[i] == '1')
                    result.Add( ( AdNetwork )i );
            }
            return result.ToArray();
        }

        internal static bool IsActiveNetwork( AdNetwork network )
        {
#if UNITY_EDITOR
            // TODO: Implementation editor 
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

        #region Execute events wrapper
        internal static void ExecuteEvent( Action action )
        {
            if (action == null)
                return;
            if (isExecuteEventsOnUnityThread)
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

        internal static void ExecuteEvent( CASEventWithError action, string error )
        {
            if (action == null)
                return;
            if (isExecuteEventsOnUnityThread)
            {
                EventExecutor.Add( () => action( error ) );
                return;
            }
            try
            {
                action( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        internal static void ExecuteEvent( CASTypedEvent action, int typeId )
        {
            if (action == null)
                return;
            if (isExecuteEventsOnUnityThread)
            {
                EventExecutor.Add( () => action( ( AdType )typeId ) );
                return;
            }
            try
            {
                action( ( AdType )typeId );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        internal static void ExecuteEvent( CASTypedEventWithError action, int typeId, string error )
        {
            if (action == null)
                return;
            if (isExecuteEventsOnUnityThread)
            {
                EventExecutor.Add( () => action( ( AdType )typeId, error ) );
                return;
            }
            try
            {
                action( ( AdType )typeId, error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        internal static void ExecuteEvent( InitCompleteAction action, bool success, string error )
        {
            if (action == null)
                return;
            if (isExecuteEventsOnUnityThread)
            {
                EventExecutor.Add( () => action( success, error ) );
                return;
            }
            try
            {
                action( success, error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion
    }
}