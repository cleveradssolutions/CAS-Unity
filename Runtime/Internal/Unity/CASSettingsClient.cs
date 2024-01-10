//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Unity
{
    internal class CASSettingsClient : IAdsSettings, ITargetingOptions
    {
        public bool analyticsCollectionEnabled { get; set; }
        public int trialAdFreeInterval { get; set; }
        public int bannerRefreshInterval { get; set; }
        public int interstitialInterval { get; set; }
        public ConsentStatus userConsent { get; set; }
        public CCPAStatus userCCPAStatus { get; set; }
        public Audience taggedAudience { get; set; }
        public bool isDebugMode { get; set; }
        public bool isMutedAdSounds { get; set; }
        public LoadingManagerMode loadingMode { get; set; }
        public bool iOSAppPauseOnBackground { get; set; }
        public bool allowInterstitialAdsWhenVideoCostAreLower { get; set; }
        public bool trackLocationEnabled { get; set; }

        public Gender gender { get; set; }
        public int age { get; set; }
        public string contentURL { get; set; }


        public float lastInterImpressionTimestamp = float.MinValue;

        internal CASSettingsClient()
        {
            iOSAppPauseOnBackground = true;
            allowInterstitialAdsWhenVideoCostAreLower = true;
        }

        public void RestartInterstitialInterval()
        {
            lastInterImpressionTimestamp = Time.time;
        }

        public void SetTestDeviceIds(IList<string> testDeviceIds)
        {
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.IsExecuteEventsOnUnityThread(); }
            set { CASFactory.SetExecuteEventsOnUnityThread(value); }
        }

        public void SetKeywords(IList<string> keywords) { }
    }
}
