//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;

namespace CAS.iOS
{
    internal class CASSettingsClient : IAdsSettings, ITargetingOptions
    {
        private bool _analyticsCollectionEnabled = false;
        private bool _isDebugMode = false;
        private bool _isMutedAdSounds = false;
        private bool _allowInterstitialAdsWhenVideoCostAreLower = true;
        private bool _trackLocationEnabled = false;

        private Gender _gender = Gender.Unknown;
        private int _age = 0;

        public bool analyticsCollectionEnabled
        {
            get { return _analyticsCollectionEnabled; }
            set
            {
                _analyticsCollectionEnabled = value;
                CASExterns.CASUSetAnalyticsCollectionWithEnabled(value);
            }
        }

        public int bannerRefreshInterval
        {
            get { return CASExterns.CASUGetBannerRefreshRate(); }
            set { CASExterns.CASUSetBannerRefreshRate(value); }
        }

        public int interstitialInterval
        {
            get { return CASExterns.CASUGetInterstitialInterval(); }
            set { CASExterns.CASUSetInterstitialInterval(value); }
        }

        public ConsentStatus userConsent
        {
            get { return (ConsentStatus)CASExterns.CASUGetUserConsent(); }
            set { CASExterns.CASUSetUserConsent((int)value); }
        }

        public CCPAStatus userCCPAStatus
        {
            get { return (CCPAStatus)CASExterns.CASUGetCCPAStatus(); }
            set { CASExterns.CASUSetCCPAStatus((int)value); }
        }
        public Audience taggedAudience
        {
            get { return (Audience)CASExterns.CASUGetAudienceTagged(); }
            set { CASExterns.CASUSetAudienceTagged((int)value); }
        }
        public bool isDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                _isDebugMode = value;
                CASExterns.CASUSetDebugMode(value);
            }
        }
        public bool isMutedAdSounds
        {
            get { return _isMutedAdSounds; }
            set
            {
                _isMutedAdSounds = value;
                CASExterns.CASUSetMuteAdSoundsTo(value);
            }
        }
        public LoadingManagerMode loadingMode
        {
            get { return (LoadingManagerMode)CASExterns.CASUGetLoadingMode(); }
            set { CASExterns.CASUSetLoadingWithMode((int)value); }
        }
        public bool iOSAppPauseOnBackground
        {
            get { return CASExterns.CASUGetiOSAppPauseOnBackground(); }
            set { CASExterns.CASUSetiOSAppPauseOnBackground(value); }
        }

        public void SetTestDeviceIds(IList<string> testDeviceIds)
        {
            string[] testDeviceIdsArray = new string[testDeviceIds.Count];
            testDeviceIds.CopyTo(testDeviceIdsArray, 0);
            CASExterns.CASUSetTestDeviceWithIds(testDeviceIdsArray, testDeviceIds.Count);
        }

        public void RestartInterstitialInterval()
        {
            CASExterns.CASURestartInterstitialInterval();
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.IsExecuteEventsOnUnityThread(); }
            set { CASFactory.SetExecuteEventsOnUnityThread(value); }
        }

        public bool allowInterstitialAdsWhenVideoCostAreLower
        {
            get { return _allowInterstitialAdsWhenVideoCostAreLower; }
            set
            {
                _allowInterstitialAdsWhenVideoCostAreLower = value;
                CASExterns.CASUSetInterstitialAdsWhenVideoCostAreLower(value);
            }
        }

        public bool trackLocationEnabled
        {
            get { return _trackLocationEnabled; }
            set
            {
                _trackLocationEnabled = value;
                CASExterns.CASUSetTrackLocationEnabled(value);
            }
        }

        public Gender gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                CASExterns.CASUSetUserGender((int)value);
            }
        }

        public int age
        {
            get { return _age; }
            set
            {
                _age = value;
                CASExterns.CASUSetUserAge(value);
            }
        }
    }
}
#endif