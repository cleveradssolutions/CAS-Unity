//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Android
{
    internal class CASSettingsClient : IAdsSettings, ITargetingOptions
    {
        private AndroidJavaClass settingsBridge;

        public CASSettingsClient()
        {
            settingsBridge = new AndroidJavaClass(CASJavaBridge.settingsClass);
        }

        public string GetSDKVersion()
        {
            return settingsBridge.CallStatic<string>("getSDKVersion");
        }

        public void ValidateIntegration()
        {
            settingsBridge.CallStatic("validateIntegration");
        }

        public string GetActiveMediationPattern()
        {
            return settingsBridge.CallStatic<string>("getActiveMediationPattern");
        }

        public bool IsActiveMediationNetwork(AdNetwork net)
        {
            return settingsBridge.CallStatic<bool>("isActiveMediationNetwork", (int)net);
        }

        public bool analyticsCollectionEnabled { get; set; }

        public int trialAdFreeInterval
        {
            get { return settingsBridge.CallStatic<int>("getTrialAdFreeInterval"); }
            set { settingsBridge.CallStatic("setTrialAdFreeInterval", value); }
        }

        public int bannerRefreshInterval
        {
            get { return settingsBridge.CallStatic<int>("getBannerRefreshDelay"); }
            set { settingsBridge.CallStatic("setRefreshBannerDelay", value); }
        }

        public int interstitialInterval
        {
            get { return settingsBridge.CallStatic<int>("getInterstitialInterval"); }
            set { settingsBridge.CallStatic("setInterstitialInterval", value); }
        }

        public ConsentStatus userConsent
        {
            get { return (ConsentStatus)settingsBridge.CallStatic<int>("getUserConsent"); }
            set { settingsBridge.CallStatic("setUserConsent", (int)value); }
        }

        public CCPAStatus userCCPAStatus
        {
            get { return (CCPAStatus)settingsBridge.CallStatic<int>("getCcpaStatus"); }
            set { settingsBridge.CallStatic("setCcpaStatus", (int)value); }
        }

        public Audience taggedAudience
        {
            get { return (Audience)settingsBridge.CallStatic<int>("getTaggedAudience"); }
            set { settingsBridge.CallStatic("setTaggedAudience", (int)value); }
        }

        public bool isDebugMode
        {
            get { return settingsBridge.CallStatic<bool>("getNativeDebug"); ; }
            set { settingsBridge.CallStatic("setNativeDebug", value); }
        }

        public bool isMutedAdSounds
        {
            get { return settingsBridge.CallStatic<bool>("getMutedAdSounds"); ; }
            set { settingsBridge.CallStatic("setMutedAdSounds", value); }
        }

        public LoadingManagerMode loadingMode
        {
            get { return (LoadingManagerMode)settingsBridge.CallStatic<int>("getLoadingMode"); }
            set { settingsBridge.CallStatic("setLoadingMode", (int)value); }
        }

        public void SetTestDeviceIds(IList<string> testDeviceIds)
        {
            settingsBridge.CallStatic("clearTestDeviceIds");
            for (int i = 0; i < testDeviceIds.Count; i++)
            {
                settingsBridge.CallStatic("addTestDeviceId", testDeviceIds[i]);
            }
        }

        public void RestartInterstitialInterval()
        {
            settingsBridge.CallStatic("restartInterstitialInterval");
        }

        public bool isExecuteEventsOnUnityThread
        {
            get { return CASFactory.IsExecuteEventsOnUnityThread(); }
            set { CASFactory.SetExecuteEventsOnUnityThread(value); }
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
            get { return settingsBridge.CallStatic<bool>("isAllowInterInsteadOfRewarded"); }
            set { settingsBridge.CallStatic("allowInterInsteadOfRewarded", value); }
        }

        public Gender gender
        {
            get { return (Gender)settingsBridge.CallStatic<int>("getUserGender"); }
            set { settingsBridge.CallStatic("setUserGender", (int)value); }
        }

        public int age
        {
            get { return settingsBridge.CallStatic<int>("getUserAge"); }
            set { settingsBridge.CallStatic("setUserAge", value); }
        }

        public string contentURL
        {
            get { return settingsBridge.CallStatic<string>("getContentURL"); }
            set { settingsBridge.CallStatic("setContentURL", value); }
        }

        public void SetKeywords(IList<string> keywords)
        {
            settingsBridge.CallStatic("clearKeywords");
            for (int i = 0; i < keywords.Count; i++)
            {
                settingsBridge.CallStatic("addKeyword", keywords[i]);
            }
        }
    }
}
#endif