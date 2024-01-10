//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;

namespace CAS.iOS
{
    internal class CASSettingsClient : IAdsSettings, ITargetingOptions
    {
        public bool analyticsCollectionEnabled { get; set; }

        public int trialAdFreeInterval
        {
            get { return CASExterns.CASUGetTrialAdFreeInterval(); }
            set { CASExterns.CASUSetTrialAdFreeInterval(value); }
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
            get { return CASExterns.CASUGetDebugMode(); }
            set { CASExterns.CASUSetDebugMode(value); }
        }

        public bool isMutedAdSounds
        {
            get { return CASExterns.CASUGetMuteAdSounds(); }
            set { CASExterns.CASUSetMuteAdSounds(value); }
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
            get { return CASExterns.CASUGetInterstitialAdsWhenVideoCostAreLower(); }
            set { CASExterns.CASUSetInterstitialAdsWhenVideoCostAreLower(value); }
        }

        public bool trackLocationEnabled
        {
            get { return CASExterns.CASUGetTrackLocationEnabled(); }
            set { CASExterns.CASUSetTrackLocationEnabled(value); }
        }

        public Gender gender
        {
            get { return (Gender)CASExterns.CASUGetUserGender(); }
            set { CASExterns.CASUSetUserGender((int)value); }
        }

        public int age
        {
            get { return CASExterns.CASUGetUserAge(); }
            set { CASExterns.CASUSetUserAge(value); }
        }

        public string contentURL
        {
            get { return CASExterns.CASUGetContentURL(); }
            set { CASExterns.CASUSetContentURL(value); }
        }

        public void SetKeywords(IList<string> keywords)
        {
            string[] tempArray = new string[keywords.Count];
            keywords.CopyTo(tempArray, 0);
            CASExterns.CASUSetKeywords(tempArray, keywords.Count);
        }
    }
}
#endif