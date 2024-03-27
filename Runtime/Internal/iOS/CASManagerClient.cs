//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal sealed class CASManagerClient : IInternalManager
    {
        private IntPtr _managerRef;
        private IntPtr _managerClient;
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
                if (_lastPageAdContent != value)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? "" : JsonUtility.ToJson(value);
                    CASExterns.CASUSetLastPageAdContent(_managerRef, json);
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
        internal CASManagerClient() { }

        ~CASManagerClient()
        {
            try
            {
                CASExterns.CASUFreeManager(_managerRef);
                _managerRef = IntPtr.Zero;
                ((GCHandle)_managerClient).Free();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal IInternalManager Init(CASInitSettings initData)
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
            _managerClient = (IntPtr)GCHandle.Alloc(this);

            if (initData.userID == null)
                initData.userID = string.Empty; // Null string not supported

            var builderRef = CASExterns.CASUCreateBuilder(
                (int)initData.allowedAdFlags,
                isTestAdMode,
                Application.unityVersion,
                initData.userID
            );

            var consentFlow = initData.consentFlow;
            if (consentFlow != null)
            {
                if (consentFlow.isEnabled)
                {
                    CASExternCallbacks.consentFlowComplete += consentFlow.OnResult;
                    CASExternCallbacks.consentFlowSimpleComplete += consentFlow.OnCompleted;

                    CASExterns.CASUSetConsentFlow(builderRef,
                        consentFlow.isEnabled,
                        (int)consentFlow.debugGeography,
                        consentFlow.privacyPolicyUrl,
                        CASExternCallbacks.OnConsentFlowCompletion);
                }
                else
                {
                    CASExterns.CASUDisableConsentFlow(builderRef);
                }
            }

            if (initData.extras != null && initData.extras.Count != 0)
            {
                var extrasKeys = new string[initData.extras.Count];
                var extrasValues = new string[initData.extras.Count];
                int extraI = 0;
                foreach (var extra in initData.extras)
                {
                    extrasKeys[extraI] = extra.Key;
                    extrasValues[extraI] = extra.Value;
                    extraI++;
                }
                CASExterns.CASUSetMediationExtras(builderRef, extrasKeys, extrasValues, extrasKeys.Length);
            }

            _managerRef = CASExterns.CASUInitializeManager(builderRef, _managerClient, InitializationCompleteCallback, managerID);

            CASExterns.CASUSetDelegates(_managerRef,
                AdLoadedCallback,
                AdLoadFailedCallback,
                AdOpeningCallback,
                AdImpressionCallback,
                AdShowFailedCallback,
                AdClickedCallback,
                AdCompletedCallback,
                AdClosedCallback);
            return this;
        }
        #endregion

        public bool IsEnabledAd(AdType adType)
        {
            return CASExterns.CASUIsAdEnabledType(_managerRef, (int)adType);
        }

        public void SetEnableAd(AdType adType, bool enabled)
        {
            CASExterns.CASUEnableAdType(_managerRef, (int)adType, enabled);
        }

        public bool IsReadyAd(AdType adType)
        {
            return CASExterns.CASUIsAdReady(_managerRef, (int)adType);
        }

        public void LoadAd(AdType adType)
        {
            CASExterns.CASULoadAd(_managerRef, (int)adType);
        }

        public void ShowAd(AdType adType)
        {
            CASExterns.CASUShowAd(_managerRef, (int)adType);
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            CASExterns.CASUOpenDebugger(_managerRef);
            return true;
        }

        public void SetAppReturnAdsEnabled(bool enable)
        {
            CASExterns.CASUSetAutoShowAdOnAppReturn(_managerRef, enable);
        }

        public void SkipNextAppReturnAds()
        {
            CASExterns.CASUSkipNextAppReturnAds(_managerRef);
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
            var view = new CASViewClient(this, size);
            var viewClient = (IntPtr)GCHandle.Alloc(view);
            view.Attach(CASExterns.CASUCreateAdView(_managerRef, viewClient, (int)size), viewClient);
            _adViews.Add(view);
            return view;
        }

        public void RemoveAdViewFromFactory(IAdView view)
        {
            _adViews.Remove(view);
        }

        #region Callback methods
        private static CASManagerClient GetClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASManagerClient;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUInitializationCompleteCallback))]
        private static void InitializationCompleteCallback(IntPtr manager, string error, string countryCode, bool withConsent, bool isTestMode)
        {
            try
            {
                CASFactory.OnManagerInitialized(GetClient(manager), error, countryCode, withConsent, isTestMode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidLoadedAdCallback))]
        private static void AdLoadedCallback(IntPtr manager, int type)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Loaded " + type);
                Action adEvent;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdLoaded;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdLoaded;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdLoaded;
                        break;
                    default:
                        return;
                }
                if (adEvent != null)
                    adEvent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidFailedAdCallback))]
        private static void AdLoadFailedCallback(IntPtr manager, int type, int error)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Failed " + type);
                CASEventWithAdError adEvent;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdFailedToLoad;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdFailedToLoad;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdFailedToLoad;
                        break;
                    default:
                        return;
                }
                if (adEvent != null)
                    adEvent((AdError)error);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void AdOpeningCallback(IntPtr manager, int type, IntPtr impression)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Shown " + type);
                CASEventWithMeta adEvent;
                Action shownEvent;
                AdType adType;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        shownEvent = client.OnInterstitialAdShown;
                        adEvent = client.OnInterstitialAdOpening;
                        adType = AdType.Interstitial;
                        break;
                    case AdTypeCode.REWARD:
                        shownEvent = client.OnRewardedAdShown;
                        adEvent = client.OnRewardedAdOpening;
                        adType = AdType.Rewarded;
                        break;
                    case AdTypeCode.APP_OPEN:
                        shownEvent = client.OnAppOpenAdShown;
                        adEvent = null;
                        adType = AdType.AppOpen;
                        break;
                    case AdTypeCode.APP_RETURN:
                        shownEvent = client.OnAppReturnAdShown;
                        adEvent = client.OnAppReturnAdOpening;
                        adType = AdType.Interstitial;
                        break;
                    default:
                        return;
                }

                if (shownEvent != null)
                    shownEvent();

                if (adEvent != null)
                    adEvent(new CASImpressionClient(adType, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void AdImpressionCallback(IntPtr manager, int type, IntPtr impression)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Impression " + type);
                CASEventWithMeta adEvent;
                AdType adType;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdImpression;
                        adType = AdType.Interstitial;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdImpression;
                        adType = AdType.Rewarded;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdImpression;
                        adType = AdType.AppOpen;
                        break;
                    case AdTypeCode.APP_RETURN:
                        adEvent = client.OnAppReturnAdImpression;
                        adType = AdType.Interstitial;
                        break;
                    default:
                        return;
                }
                if (adEvent != null)
                    adEvent(new CASImpressionClient(adType, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidShowAdFailedWithErrorCallback))]
        private static void AdShowFailedCallback(IntPtr manager, int type, int error)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Show failed " + type);
                CASEventWithError adEvent;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdFailedToShow;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdFailedToShow;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdFailedToShow;
                        break;
                    case AdTypeCode.APP_RETURN:
                        adEvent = client.OnAppReturnAdFailedToShow;
                        break;
                    default:
                        return;
                }

                if (adEvent != null)
                {
                    var adError = (AdError)error;
                    adEvent(adError.GetMessage());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClickedAdCallback))]
        private static void AdClickedCallback(IntPtr manager, int type)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Clicked " + type);
                Action adEvent;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdClicked;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdClicked;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdClicked;
                        break;
                    case AdTypeCode.APP_RETURN:
                        adEvent = client.OnAppReturnAdClicked;
                        break;
                    default:
                        return;
                }
                if (adEvent != null)
                    adEvent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClosedAdCallback))]
        private static void AdClosedCallback(IntPtr manager, int type)
        {
            try
            {
                var client = GetClient(manager);
                CASFactory.UnityLog("Callback Closed " + type);
                Action adEvent;
                switch (type)
                {
                    case AdTypeCode.INTER:
                        adEvent = client.OnInterstitialAdClosed;
                        break;
                    case AdTypeCode.REWARD:
                        adEvent = client.OnRewardedAdClosed;
                        break;
                    case AdTypeCode.APP_OPEN:
                        adEvent = client.OnAppOpenAdClosed;
                        break;
                    case AdTypeCode.APP_RETURN:
                        adEvent = client.OnAppReturnAdClosed;
                        break;
                    default:
                        return;
                }
                if (adEvent != null)
                    adEvent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidCompletedAdCallback))]
        private static void AdCompletedCallback(IntPtr manager, int type)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Completed");
                if (type == AdTypeCode.REWARD)
                {
                    var client = GetClient(manager);
                    if (client.OnRewardedAdCompleted != null)
                        client.OnRewardedAdCompleted();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
    }
}
#endif