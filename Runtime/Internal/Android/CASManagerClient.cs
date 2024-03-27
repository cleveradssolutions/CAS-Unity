//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;
using System.Collections.Generic;

namespace CAS.Android
{
    internal sealed class CASManagerClient : AndroidJavaProxy, IInternalManager
    {
        private AndroidJavaObject _managerBridge;
        private LastPageAdContent _lastPageAdContent = null;

        private readonly List<IAdView> _adViews = new List<IAdView>();

        public string managerID { get; private set; }
        public bool isTestAdMode { get; set; }
        public CASInitCompleteEvent initCompleteEvent { get; set; }
        public InitCompleteAction initCompleteAction { get; set; }
        public InitialConfiguration initialConfig { get; set; }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                if (value != _lastPageAdContent)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? null : JsonUtility.ToJson(value);
                    _managerBridge.Call("setLastPageAdContent", json);
                }
            }
        }

        #region Ad Events
        public event Action OnInterstitialAdLoaded;
        public event CASEventWithAdError OnInterstitialAdFailedToLoad;
        public event Action OnInterstitialAdShown;
        public event CASEventWithMeta OnInterstitialAdOpening;
        public event CASEventWithMeta OnInterstitialAdImpression;
        public event CASEventWithError OnInterstitialAdFailedToShow;
        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdClosed;

        public event Action OnRewardedAdLoaded;
        public event CASEventWithAdError OnRewardedAdFailedToLoad;
        public event Action OnRewardedAdShown;
        public event CASEventWithMeta OnRewardedAdOpening;
        public event CASEventWithMeta OnRewardedAdImpression;
        public event CASEventWithError OnRewardedAdFailedToShow;
        public event Action OnRewardedAdClicked;
        public event Action OnRewardedAdCompleted;
        public event Action OnRewardedAdClosed;

        public event Action OnAppReturnAdShown;
        public event CASEventWithMeta OnAppReturnAdOpening;
        public event CASEventWithMeta OnAppReturnAdImpression;
        public event CASEventWithError OnAppReturnAdFailedToShow;
        public event Action OnAppReturnAdClicked;
        public event Action OnAppReturnAdClosed;

        public event Action OnAppOpenAdLoaded;
        public event CASEventWithAdError OnAppOpenAdFailedToLoad;
        public event Action OnAppOpenAdShown;
        public event CASEventWithMeta OnAppOpenAdImpression;
        public event CASEventWithError OnAppOpenAdFailedToShow;
        public event Action OnAppOpenAdClicked;
        public event Action OnAppOpenAdClosed;
        #endregion

        #region Initialization
        internal CASManagerClient() : base(CASJavaBridge.AdCallbackClass) { }

        internal IInternalManager Init(CASInitSettings initData)
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
            EventExecutor.Initialize();

            using (var builder = new AndroidJavaObject(CASJavaBridge.BridgeBuilderClass))
            {
                if (isTestAdMode)
                    builder.Call("enableTestMode");

                if (!string.IsNullOrEmpty(initData.userID))
                    builder.Call("setUserId", initData.userID);

                if (initData.consentFlow != null)
                {
                    if (!initData.consentFlow.isEnabled)
                        builder.Call("disableConsentFlow");
                    else
                        builder.Call("withConsentFlow", new CASConsentFlowClient(initData.consentFlow, false));
                }
                if (initData.extras != null)
                {
                    foreach (var extra in initData.extras)
                    {
                        builder.Call("addExtras", extra.Key, extra.Value);
                    }
                }

                _managerBridge = builder.Call<AndroidJavaObject>("build",
                    initData.targetId, Application.unityVersion, this, (int)initData.allowedAdFlags);
            }
            return this;
        }
        #endregion

        public bool IsEnabledAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isEnabled", (int)adType);
        }

        public void SetEnableAd(AdType adType, bool enabled)
        {
            _managerBridge.Call("enableAd", (int)adType, enabled);
        }

        public bool IsReadyAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isAdReady", (int)adType);
        }

        public void LoadAd(AdType adType)
        {
            _managerBridge.Call("loadAd", (int)adType);
        }

        public void ShowAd(AdType adType)
        {
            _managerBridge.Call("showAd", (int)adType);
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            return _managerBridge.Call<bool>("tryOpenDebugger");
        }

        public void SetAppReturnAdsEnabled(bool enable)
        {
            _managerBridge.Call("setAutoShowAdOnAppReturn", enable);
        }

        public void SkipNextAppReturnAds()
        {
            _managerBridge.Call("skipNextReturnAds");
        }

        public IAdView GetAdView(AdSize size)
        {
            if (size < AdSize.Banner)
                throw new ArgumentException("Invalid AdSize " + size.ToString());
            for (int i = 0; i < _adViews.Count; i++)
            {
                if (_adViews[i].size == size)
                    return _adViews[i];
            }
            var view = new CASViewClient(this, size, _managerBridge);
            _adViews.Add(view);
            return view;
        }

        public void RemoveAdViewFromFactory(IAdView view)
        {
            _adViews.Remove(view);
        }

        #region Android Native callbacks

        public void onCASInitialized(string error,
                                string countryCode,
                                bool isConsentRequired,
                                bool isTestMode)
        {
            if (string.IsNullOrEmpty(error))
                error = null;
            if (string.IsNullOrEmpty(countryCode))
                countryCode = null;

            CASJavaBridge.ExecuteEvent(() =>
            {
                CASFactory.OnManagerInitialized(this, error, countryCode, isConsentRequired, isTestMode);
            });
        }

        public void onAdLoaded(int type)
        {
            CASFactory.UnityLog("Callback Loaded " + type);
            Action adEvent;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdLoaded;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdLoaded;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdLoaded;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(adEvent);
        }

        public void onAdFailed(int type, int error)
        {
            CASFactory.UnityLog("Callback Failed " + type);
            CASEventWithAdError adEvent;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdFailedToLoad;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdFailedToLoad;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdFailedToLoad;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(adEvent, error);
        }

        public void onAdOpening(int type, AndroidJavaObject impression)
        {
            CASFactory.UnityLog("Callback Shown " + type);
            CASEventWithMeta adEvent;
            Action shownEvent;
            AdType adType;
            switch (type)
            {
                case AdTypeCode.INTER:
                    shownEvent = OnInterstitialAdShown;
                    adEvent = OnInterstitialAdOpening;
                    adType = AdType.Interstitial;
                    break;
                case AdTypeCode.REWARD:
                    shownEvent = OnRewardedAdShown;
                    adEvent = OnRewardedAdOpening;
                    adType = AdType.Rewarded;
                    break;
                case AdTypeCode.APP_OPEN:
                    shownEvent = OnAppOpenAdShown;
                    adEvent = null;
                    adType = AdType.AppOpen;
                    break;
                case AdTypeCode.APP_RETURN:
                    shownEvent = OnAppReturnAdShown;
                    adEvent = OnAppReturnAdOpening;
                    adType = AdType.Interstitial;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(shownEvent);
            CASJavaBridge.ExecuteEvent(adEvent, adType, impression);
        }

        public void onAdImpression(int type, AndroidJavaObject impression)
        {
            CASFactory.UnityLog("Callback Impression " + type);
            CASEventWithMeta adEvent;
            AdType adType;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdImpression;
                    adType = AdType.Interstitial;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdImpression;
                    adType = AdType.Rewarded;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdImpression;
                    adType = AdType.AppOpen;
                    break;
                case AdTypeCode.APP_RETURN:
                    adEvent = OnAppReturnAdImpression;
                    adType = AdType.Interstitial;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(adEvent, adType, impression);
        }

        public void onAdShowFailed(int type, int error)
        {
            CASFactory.UnityLog("Callback Show failed " + type);
            CASEventWithError adEvent;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdFailedToShow;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdFailedToShow;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdFailedToShow;
                    break;
                case AdTypeCode.APP_RETURN:
                    adEvent = OnAppReturnAdFailedToShow;
                    break;
                default:
                    return;
            }
            if (adEvent == null) return;
            if (CASJavaBridge.executeEventsOnUnityThread)
            {
                EventExecutor.Add(() =>
                {
                    AdError adError = (AdError)error;
                    adEvent(adError.GetMessage());
                });
                return;
            }
            try
            {
                AdError adError = (AdError)error;
                adEvent(adError.GetMessage());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void onAdClicked(int type)
        {
            CASFactory.UnityLog("Callback Clicked " + type);
            Action adEvent;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdClicked;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdClicked;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdClicked;
                    break;
                case AdTypeCode.APP_RETURN:
                    adEvent = OnAppReturnAdClicked;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(adEvent);
        }

        public void onAdComplete(int type)
        {
            CASFactory.UnityLog("Callback Completed " + type);
            if (type == AdTypeCode.REWARD)
            {
                CASJavaBridge.ExecuteEvent(OnRewardedAdCompleted);
            }
        }

        public void onAdClosed(int type)
        {
            CASFactory.UnityLog("Callback Closed " + type);
            Action adEvent;
            switch (type)
            {
                case AdTypeCode.INTER:
                    adEvent = OnInterstitialAdClosed;
                    break;
                case AdTypeCode.REWARD:
                    adEvent = OnRewardedAdClosed;
                    break;
                case AdTypeCode.APP_OPEN:
                    adEvent = OnAppOpenAdClosed;
                    break;
                case AdTypeCode.APP_RETURN:
                    adEvent = OnAppReturnAdClosed;
                    break;
                default:
                    return;
            }
            CASJavaBridge.ExecuteEvent(adEvent);
        }
        #endregion
    }
}
#endif
