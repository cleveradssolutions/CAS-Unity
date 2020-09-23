#if UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;

namespace CAS.iOS
{
    internal class CASSettings : IAdsSettings
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
        private bool _iOSAppPauseOnBackground = true;
        private List<string> _testDeviceIds = new List<string>();

        public bool analyticsCollectionEnabled
        {
            get { return _analyticsCollectionEnabled; }
            set
            {
                if (_analyticsCollectionEnabled != value)
                {
                    _analyticsCollectionEnabled = value;
                    CASExterns.CASUSetAnalyticsCollectionWithEnabled( value );
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
                    CASExterns.CASUSetBannerRefreshWithInterval( value );
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
                    CASExterns.CASUSetInterstitialWithInterval( value );
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
                    CASExterns.CASUUpdateUserConsent( ( int )value );
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
                    CASExterns.CASUUpdateCCPAWithStatus( ( int )value );
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
                    CASExterns.CASUSetTaggedWithAudience( ( int )value );
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
                    CASExterns.CASUSetDebugMode( value );
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
                    CASExterns.CASUSetMuteAdSoundsTo( value );
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
                    CASExterns.CASUSetLoadingWithMode( ( int )value );
                }
            }
        }
        public bool iOSAppPauseOnBackground
        {
            get { return _iOSAppPauseOnBackground; }
            set
            {
                if (_iOSAppPauseOnBackground != value)
                {
                    _iOSAppPauseOnBackground = value;
                    CASExterns.CASUSetiOSAppPauseOnBackground( value );
                }
            }
        }

        public void SetTestDeviceIds( List<string> testDeviceIds )
        {
            _testDeviceIds = testDeviceIds;
            string[] testDeviceIdsArray = new string[testDeviceIds.Count];
            testDeviceIds.CopyTo( testDeviceIdsArray );
            CASExterns.CASUSetTestDeviceWithIds( testDeviceIdsArray, testDeviceIds.Count );
        }

        public List<string> GetTestDeviceIds()
        {
            return _testDeviceIds;
        }

        public void RestartInterstitialInterval()
        {
            CASExterns.CASURestartInterstitialInterval();
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.isExecuteEventsOnUnityThread; }
            set { CASFactory.isExecuteEventsOnUnityThread = value; }
        }
    }
}
#endif