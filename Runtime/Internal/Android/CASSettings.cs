//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Android
{
    internal class CASSettings : IAdsSettings, ITargetingOptions
    {
        private bool _analyticsCollectionEnabled = false;
        private int _bannerRefreshInterval = 30;
        private int _interstitialInterval = 0;
        private ConsentStatus _userConsent = ConsentStatus.Undefined;
        private CCPAStatus _userCCPAStatus = CCPAStatus.Undefined;
        private Audience _taggedAudience = Audience.Mixed;
        private bool _isDebugMode = false;
        private bool _isMutedAdSounds = false;
        private LoadingManagerMode _loadingMode = LoadingManagerMode.Optimal;
        private List<string> _testDeviceIds = new List<string>();
        private bool _allowInterstitialAdsWhenVideoCostAreLower = false;

        private Gender _gender = Gender.Unknown;
        private int _age = 0;

        private AndroidJavaClass settingsBridge;

        public CASSettings()
        {
            settingsBridge = new AndroidJavaClass( CASJavaProxy.NativeSettingsClassName );
        }

        public string GetSDKVersion()
        {
            return settingsBridge.CallStatic<string>( "getSDKVersion" );
        }

        public void ValidateIntegration()
        {
            settingsBridge.CallStatic( "validateIntegration", CASJavaProxy.GetUnityActivity() );
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
                if (_analyticsCollectionEnabled != value)
                {
                    _analyticsCollectionEnabled = value;
                    settingsBridge.CallStatic( "setAnalyticsCollectionEnabled", value );
                }
            }
        }

        public int bannerRefreshInterval
        {
            get { return _bannerRefreshInterval; }
            set
            {
                if (_bannerRefreshInterval != value)
                {
                    _bannerRefreshInterval = value;
                    settingsBridge.CallStatic( "setRefreshBannerDelay", value );
                }
            }
        }

        public int interstitialInterval
        {
            get { return _interstitialInterval; }
            set
            {
                if (_interstitialInterval != value)
                {
                    _interstitialInterval = value;
                    settingsBridge.CallStatic( "setInterstitialInterval", value );
                }
            }
        }

        public ConsentStatus userConsent
        {
            get { return _userConsent; }
            set
            {
                if (_userConsent != value)
                {
                    _userConsent = value;
                    settingsBridge.CallStatic( "setUserConsentStatus", ( int )value );
                }
            }
        }

        public CCPAStatus userCCPAStatus
        {
            get { return _userCCPAStatus; }
            set
            {
                if (_userCCPAStatus != value)
                {
                    _userCCPAStatus = value;
                    settingsBridge.CallStatic( "setDoNotSellStatus", ( int )value );
                }
            }
        }

        public Audience taggedAudience
        {
            get { return _taggedAudience; }
            set
            {
                if (_taggedAudience != value)
                {
                    _taggedAudience = value;
                    settingsBridge.CallStatic( "setTaggedAudience", ( int )value );
                }
            }
        }

        public bool isDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                if (_isDebugMode != value)
                {
                    _isDebugMode = value;
                    settingsBridge.CallStatic( "setNativeDebug", value );
                }
            }
        }

        public bool isMutedAdSounds
        {
            get { return _isMutedAdSounds; }
            set
            {
                if (_isMutedAdSounds != value)
                {
                    _isMutedAdSounds = value;
                    settingsBridge.CallStatic( "setMutedAdSounds", value );
                }
            }
        }

        public LoadingManagerMode loadingMode
        {
            get { return _loadingMode; }
            set
            {
                if (_loadingMode != value)
                {
                    _loadingMode = value;
                    settingsBridge.CallStatic( "setLoadingMode", ( int )value );
                }
            }
        }

        public void SetTestDeviceIds( List<string> testDeviceIds )
        {
            _testDeviceIds = testDeviceIds;
            var nativeList = CASJavaProxy.GetJavaListObject( testDeviceIds );
            settingsBridge.CallStatic( "setTestDeviceIds", nativeList );
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
                if (_allowInterstitialAdsWhenVideoCostAreLower != value)
                {
                    _allowInterstitialAdsWhenVideoCostAreLower = value;
                    settingsBridge.CallStatic( "allowInterInsteadOfRewarded", value );
                }
            }
        }

        public Gender gender
        {
            get { return _gender; }
            set
            {
                if (_gender != value)
                {
                    _gender = value;
                    settingsBridge.CallStatic( "setUserGender", ( int )value );
                }
            }
        }

        public int age
        {
            get { return _age; }
            set
            {
                if (_age != value)
                {
                    _age = value;
                    settingsBridge.CallStatic( "setUserAge", value );
                }
            }
        }
    }
}
#endif