//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CAS.Android
{
    internal class CASSettingsClient : IAdsSettings, ITargetingOptions
    {
        private bool _analyticsCollectionEnabled = false;
        private bool _isDebugMode = false;
        private bool _isMutedAdSounds = false;
        private LoadingManagerMode _loadingMode = LoadingManagerMode.Optimal;
        private List<string> _testDeviceIds = new List<string>();
        private bool _allowInterstitialAdsWhenVideoCostAreLower = false;

        private Gender _gender = Gender.Unknown;
        private int _age = 0;

        private AndroidJavaClass settingsBridge;

        public CASSettingsClient()
        {
            settingsBridge = new AndroidJavaClass( CASJavaBridge.settingsClass );
        }

        public string GetSDKVersion()
        {
            return settingsBridge.CallStatic<string>( "getSDKVersion" );
        }

        public void ValidateIntegration()
        {
            settingsBridge.CallStatic( "validateIntegration" );
        }

        public string GetActiveMediationPattern()
        {
            return settingsBridge.CallStatic<string>( "getActiveMediationPattern" );
        }

        public bool IsActiveMediationNetwork( AdNetwork net )
        {
            return settingsBridge.CallStatic<bool>( "isActiveMediationNetwork", (int)net );
        }

        public bool analyticsCollectionEnabled
        {
            get { return _analyticsCollectionEnabled; }
            set
            {
                _analyticsCollectionEnabled = value;
                settingsBridge.CallStatic( "setAnalyticsCollectionEnabled", value );
            }
        }

        public int bannerRefreshInterval
        {
            get { return settingsBridge.CallStatic<int>( "getBannerRefreshDelay" ); }
            set { settingsBridge.CallStatic( "setRefreshBannerDelay", value ); }
        }

        public int interstitialInterval
        {
            get { return settingsBridge.CallStatic<int>( "getInterstitialInterval" ); }
            set { settingsBridge.CallStatic( "setInterstitialInterval", value ); }
        }

        public ConsentStatus userConsent
        {
            get { return (ConsentStatus)settingsBridge.CallStatic<int>( "getUserConsent" ); }
            set { settingsBridge.CallStatic( "setUserConsent", (int)value ); }
        }

        public CCPAStatus userCCPAStatus
        {
            get { return (CCPAStatus)settingsBridge.CallStatic<int>( "getCcpaStatus" ); }
            set { settingsBridge.CallStatic( "setCcpaStatus", (int)value ); }
        }

        public Audience taggedAudience
        {
            get { return (Audience)settingsBridge.CallStatic<int>( "getTaggedAudience" ); }
            set { settingsBridge.CallStatic( "setTaggedAudience", (int)value ); }
        }

        public bool isDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                _isDebugMode = value;
                settingsBridge.CallStatic( "setNativeDebug", value );
            }
        }

        public bool isMutedAdSounds
        {
            get { return _isMutedAdSounds; }
            set
            {
                _isMutedAdSounds = value;
                settingsBridge.CallStatic( "setMutedAdSounds", value );
            }
        }

        public LoadingManagerMode loadingMode
        {
            get { return _loadingMode; }
            set
            {
                _loadingMode = value;
                settingsBridge.CallStatic( "setLoadingMode", (int)value );
            }
        }

        public void SetTestDeviceIds( List<string> testDeviceIds )
        {
            settingsBridge.CallStatic( "clearTestDeviceIds" );
            _testDeviceIds = testDeviceIds;
            for (int i = 0; i < testDeviceIds.Count; i++)
            {
                settingsBridge.CallStatic( "addTestDeviceId", testDeviceIds[i] );
            }
        }

        public List<string> GetTestDeviceIds()
        {
            return _testDeviceIds;
        }

        public void RestartInterstitialInterval()
        {
            settingsBridge.CallStatic( "restartInterstitialInterval" );
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.IsExecuteEventsOnUnityThread(); }
            set { CASFactory.SetExecuteEventsOnUnityThread( value ); }
        }

        public bool iOSAppPauseOnBackground
        {
            get { return false; }
            set {  /*Only for iOS*/ }
        }

        public bool trackLocationEnabled
        {
            get { return false; }
            set {  /*Only for iOS*/ }
        }

        public bool allowInterstitialAdsWhenVideoCostAreLower
        {
            get { return _allowInterstitialAdsWhenVideoCostAreLower; }
            set
            {
                _allowInterstitialAdsWhenVideoCostAreLower = value;
                settingsBridge.CallStatic( "allowInterInsteadOfRewarded", value );
            }
        }

        public Gender gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                settingsBridge.CallStatic( "setUserGender", (int)value );
            }
        }

        public int age
        {
            get { return _age; }
            set
            {
                _age = value;
                settingsBridge.CallStatic( "setUserAge", value );
            }
        }
    }
}
#endif