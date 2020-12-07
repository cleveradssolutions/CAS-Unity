using System;
using System.IO;
using UnityEngine;

namespace CAS
{
    internal static class CASFactory
    {
        internal static volatile bool isExecuteEventsOnUnityThread = false;

        internal static IAdsSettings _settings;

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


        internal static IMediationManager Initialize(
            string managerID,
            AdFlags enableAd = AdFlags.Everything,
            bool testAdMode = false,
            InitCompleteAction initCompleteAction = null )
        {
            if (string.IsNullOrEmpty( managerID ))
                throw new ArgumentNullException( "managerID", "Manager ID is empty" );
            if (MobileAds.manager != null && MobileAds.manager.managerID == managerID)
            {
                if (initCompleteAction != null)
                    initCompleteAction( true, null );
                return MobileAds.manager;
            }
            if (_settings == null)
                _settings = CreateSettigns( LoadInitSettingsFromResources() );
            EventExecutor.Initialize();
            return CreateManager( managerID, enableAd, testAdMode, initCompleteAction );
        }

        internal static IMediationManager InitializeFromResources(
            int managerIndex, AdFlags enableAd, InitCompleteAction initCompleteAction = null )
        {
            var initSettings = LoadInitSettingsFromResources();

            if (!initSettings)
                throw new FileNotFoundException( "No settings found in resources. " +
                    "Please use Assets/CleverAdsSolutions/Settings menu for create settings asset." );
            string managerID;
            if (Application.isEditor || initSettings.testAdMode)
            {
                managerID = "demo";
            }
            else
            {
                if (initSettings.managerIds.Length - 1 < managerIndex || string.IsNullOrEmpty( initSettings.managerIds[managerIndex] ))
                    throw new ArgumentNullException( "managerIds", "Manager ID is empty. " +
                        "Please use Assets/CleverAdsSolutions/Settings menu and set manager ID." );
                managerID = initSettings.managerIds[managerIndex];
            }
            if (MobileAds.manager != null && MobileAds.manager.managerID == managerID)
            {
                if (initCompleteAction != null)
                    initCompleteAction( true, null );
                return MobileAds.manager;
            }
            EventExecutor.Initialize();
            var manager = CreateManager( managerID, initSettings.allowedAdFlags & enableAd, initSettings.testAdMode, initCompleteAction );
            manager.bannerSize = initSettings.bannerSize;
            return manager;
        }

        internal static IMediationManager CreateManager(
            string managerID, AdFlags enableAd, bool demoAdMode, InitCompleteAction initCompleteAction )
        {
            IMediationManager manager = null;
#if UNITY_EDITOR || TARGET_OS_SIMULATOR
            manager = CAS.Unity.CASMediationManager.CreateManager( enableAd, initCompleteAction );
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var android = new CAS.Android.CASMediationManager( managerID, demoAdMode );
                android.CreateManager( enableAd, initCompleteAction );
                manager = android;
            }
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var ios = new CAS.iOS.CASMediationManager( managerID, demoAdMode );
                ios.CreateManager( enableAd, initCompleteAction );
                manager = ios;
            }
#endif
            if (manager == null)
                throw new NotSupportedException( "Current platform: " + Application.platform.ToString() );
            return manager;
        }

        internal static void ValidateIntegration()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                var androidSettings = MobileAds.settings as CAS.Android.CASSettings;
                androidSettings.ValidateIntegration();
            }
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExterns.CASUValidateIntegration();
            }
#endif
        }

        internal static CASInitSettings LoadInitSettingsFromResources()
        {
            string assetName;
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.Android:
                    assetName = "CASSettingsAndroid";
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.IPhonePlayer:
                    assetName = "CASSettingsiOS";
                    break;
                default:
                    return null;
            }
            return Resources.Load<CASInitSettings>( assetName );
        }

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
    }
}