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
        private CASInitCompleteEvent _initComplete;
        private InitCompleteAction _initCompleteDeprecated;
        private LastPageAdContent _lastPageAdContent = null;
        private readonly List<IAdView> _adViews = new List<IAdView>();

        public string managerID { get; private set; }
        public bool isTestAdMode { get; private set; }
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

            if (initData.consentFlow != null)
            {
                if (initData.consentFlow.isEnabled)
                {
                    CASExternCallbacks.consentFlowComplete += initData.consentFlow.OnCompleted;

                    CASExterns.CASUSetConsentFlow(builderRef,
                        initData.consentFlow.isEnabled,
                        initData.consentFlow.privacyPolicyUrl,
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

            _initComplete = initData.initListener;
            _initCompleteDeprecated = initData.initListenerDeprecated;

            _managerRef = CASExterns.CASUInitializeManager(builderRef, _managerClient, InitializationCompleteCallback, managerID);

            CASExterns.CASUSetInterstitialDelegate(_managerRef,
                InterstitialLoadedAdCallback,
                InterstitialFailedAdCallback,
                InterstitialOpeningWithMetaCallback,
                InterstitialImpressionWithMetaCallback,
                InterstitialDidShowAdFailedWithErrorCallback,
                InterstitialDidClickedAdCallback,
                InterstitialDidClosedAdCallback);

            CASExterns.CASUSetRewardedDelegate(_managerRef,
                RewardedLoadedAdCallback,
                RewardedFailedAdCallback,
                RewardedOpeningWithAdCallbackAndMeta,
                RewardedImpressionWithMetaCallback,
                RewardedDidShowAdFailedWithErrorCallback,
                RewardedDidClickedAdCallback,
                RewardedDidCompletedAdCallback,
                RewardedDidClosedAdCallback);

            CASExterns.CASUSetAppReturnDelegate(_managerRef,
                ReturnAdOpeningWithAdCallback,
                ReturnAdImpressionWithMetaCallback,
                ReturnAdDidShowAdFailedWithErrorCallback,
                ReturnAdDidClickedAdCallback,
                ReturnAdDidClosedAdCallback);
            return this;
        }

        public void HandleInitEvent(CASInitCompleteEvent initEvent, InitCompleteAction initAction)
        {
            if (initialConfig != null)
            {
                if (initEvent != null)
                    initEvent(initialConfig);
                if (initAction != null)
                    initAction(initialConfig.error == null, initialConfig.error);
                return;
            }
            _initComplete += initEvent;
            _initCompleteDeprecated += initAction;
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
            switch (adType)
            {
                case AdType.Interstitial:
                    return CASExterns.CASUIsInterstitialReady(_managerRef);
                case AdType.Rewarded:
                    return CASExterns.CASUIsRewardedReady(_managerRef);
                default:
                    return false;
            }
        }

        public void LoadAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                    CASExterns.CASULoadInterstitial(_managerRef);
                    break;
                case AdType.Rewarded:
                    CASExterns.CASULoadReward(_managerRef);
                    break;
            }
        }

        public void ShowAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                    CASExterns.CASUPresentInterstitial(_managerRef);
                    break;
                case AdType.Rewarded:
                    CASExterns.CASUPresentRewarded(_managerRef);
                    break;
            }
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            CASExterns.CASUOpenDebugger(_managerRef);
            return true;
        }

        public void SetAppReturnAdsEnabled(bool enable)
        {
            if (enable)
                CASExterns.CASUEnableAppReturnAds(_managerRef);
            else
                CASExterns.CASUDisableAppReturnAds(_managerRef);
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
        private static CASManagerClient IntPtrToManagerClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASManagerClient;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUInitializationCompleteCallback))]
        private static void InitializationCompleteCallback(IntPtr client, string error, string countryCode, bool withConsent, bool isTestMode)
        {
            try
            {
                CASFactory.UnityLog("InitializationComplete " + error);
                var instance = IntPtrToManagerClient(client);
                if (instance != null)
                {
                    instance.isTestAdMode = isTestMode;
                    instance.initialConfig = new InitialConfiguration(error, instance, countryCode, withConsent);

                    CASFactory.OnManagerInitialized(instance);
                    instance.HandleInitEvent(instance._initComplete, instance._initCompleteDeprecated);

                    if (error != InitializationError.NoConnection)
                    {
                        instance._initComplete = null;
                        instance._initCompleteDeprecated = null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        #region Interstitial Callback
        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidLoadedAdCallback))]
        private static void InterstitialLoadedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Interstitial Loaded");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnInterstitialAdLoaded != null)
                    instance.OnInterstitialAdLoaded();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidFailedAdCallback))]
        private static void InterstitialFailedAdCallback(IntPtr manager, int error)
        {
            try
            {
                var adError = (AdError)error;
                CASFactory.UnityLog("Interstitial Failed with error: " + adError.ToString());
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnInterstitialAdFailedToLoad != null)
                    instance.OnInterstitialAdFailedToLoad(adError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void InterstitialOpeningWithMetaCallback(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Interstitial Will present");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnInterstitialAdShown != null)
                    instance.OnInterstitialAdShown();

                if (instance.OnInterstitialAdOpening != null)
                    instance.OnInterstitialAdOpening(new CASImpressionClient(AdType.Interstitial, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void InterstitialImpressionWithMetaCallback(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Interstitial did impression");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnInterstitialAdImpression != null)
                    instance.OnInterstitialAdImpression(new CASImpressionClient(AdType.Interstitial, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidShowAdFailedWithErrorCallback))]
        private static void InterstitialDidShowAdFailedWithErrorCallback(IntPtr manager, int error)
        {
            try
            {
                CASFactory.UnityLog("Interstitial Show Ad Failed with error: " + error);
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnInterstitialAdFailedToShow != null)
                {
                    var adError = (AdError)error;
                    instance.OnInterstitialAdFailedToShow(adError.GetMessage());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClickedAdCallback))]
        private static void InterstitialDidClickedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Interstitial Clicked");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnInterstitialAdClicked != null)
                    instance.OnInterstitialAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClosedAdCallback))]
        private static void InterstitialDidClosedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Interstitial Closed");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnInterstitialAdClosed != null)
                    instance.OnInterstitialAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        #region Rewarded Callback
        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidLoadedAdCallback))]
        private static void RewardedLoadedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Loaded");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnRewardedAdLoaded != null)
                    instance.OnRewardedAdLoaded();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidFailedAdCallback))]
        private static void RewardedFailedAdCallback(IntPtr manager, int error)
        {
            try
            {
                var adError = (AdError)error;
                CASFactory.UnityLog("Rewarded Failed with error: " + adError.ToString());
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnRewardedAdFailedToLoad != null)
                    instance.OnRewardedAdFailedToLoad(adError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void RewardedOpeningWithAdCallbackAndMeta(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Rewarded will present");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnRewardedAdShown != null)
                    instance.OnRewardedAdShown();
                if (instance.OnRewardedAdOpening != null)
                    instance.OnRewardedAdOpening(new CASImpressionClient(AdType.Rewarded, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void RewardedImpressionWithMetaCallback(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Rewarded did impression");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnRewardedAdImpression != null)
                    instance.OnRewardedAdImpression(new CASImpressionClient(AdType.Rewarded, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidShowAdFailedWithErrorCallback))]
        private static void RewardedDidShowAdFailedWithErrorCallback(IntPtr manager, int error)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Show Ad Failed with error: " + error);
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnRewardedAdFailedToShow != null)
                {
                    var adError = (AdError)error;
                    instance.OnRewardedAdFailedToShow(adError.GetMessage());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClickedAdCallback))]
        private static void RewardedDidClickedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Clicked");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnRewardedAdClicked != null)
                    instance.OnRewardedAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidCompletedAdCallback))]
        private static void RewardedDidCompletedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Completed");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnRewardedAdCompleted != null)
                    instance.OnRewardedAdCompleted();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClosedAdCallback))]
        private static void RewardedDidClosedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Rewarded Closed");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnRewardedAdClosed != null)
                    instance.OnRewardedAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        #region App Return Ads Callback
        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void ReturnAdOpeningWithAdCallback(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Return Ad will present");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnAppReturnAdShown != null)
                    instance.OnAppReturnAdShown();
                if (instance.OnAppReturnAdOpening != null)
                    instance.OnAppReturnAdOpening(new CASImpressionClient(AdType.Interstitial, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUWillPresentAdCallback))]
        private static void ReturnAdImpressionWithMetaCallback(IntPtr manager, IntPtr impression)
        {
            try
            {
                CASFactory.UnityLog("Return Ad did impression");
                var instance = IntPtrToManagerClient(manager);
                if (instance == null)
                    return;
                if (instance.OnAppReturnAdImpression != null)
                    instance.OnAppReturnAdImpression(new CASImpressionClient(AdType.Interstitial, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidShowAdFailedWithErrorCallback))]
        private static void ReturnAdDidShowAdFailedWithErrorCallback(IntPtr manager, int error)
        {
            try
            {
                CASFactory.UnityLog("Return Ad Show Failed " + error);
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnAppReturnAdFailedToShow != null)
                {
                    var adError = (AdError)error;
                    instance.OnAppReturnAdFailedToShow(adError.GetMessage());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClickedAdCallback))]
        private static void ReturnAdDidClickedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Return Ad Clicked");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnAppReturnAdClicked != null)
                    instance.OnAppReturnAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUDidClosedAdCallback))]
        private static void ReturnAdDidClosedAdCallback(IntPtr manager)
        {
            try
            {
                CASFactory.UnityLog("Return Ad Closed");
                var instance = IntPtrToManagerClient(manager);
                if (instance != null && instance.OnAppReturnAdClosed != null)
                    instance.OnAppReturnAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
        #endregion
    }
}
#endif