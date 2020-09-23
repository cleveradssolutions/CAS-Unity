using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Unity
{
    internal class CASSettings : IAdsSettings
    {
        public bool analyticsCollectionEnabled { get; set; }
        public int bannerRefreshInterval { get; set; }
        public int interstitialInterval { get; set; }
        public ConsentStatus userConsent { get; set; }
        public CCPAStatus userCCPAStatus { get; set; }
        public Audience taggedAudience { get; set; }
        public bool isDebugMode { get; set; }
        public bool isMutedAdSounds { get; set; }
        public LoadingManagerMode loadingMode { get; set; }
        public bool iOSAppPauseOnBackground { get; set; }

        public float lastInterImpressionTimestamp = float.MinValue;

        private List<string> testDeviceIds = new List<string>();

        public List<string> GetTestDeviceIds()
        {
            return testDeviceIds;
        }

        public void RestartInterstitialInterval()
        {
            lastInterImpressionTimestamp = Time.time;
        }

        public void SetTestDeviceIds( List<string> testDeviceIds )
        {
            this.testDeviceIds = testDeviceIds;
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.isExecuteEventsOnUnityThread; }
            set { CASFactory.isExecuteEventsOnUnityThread = value; }
        }
    }
}
