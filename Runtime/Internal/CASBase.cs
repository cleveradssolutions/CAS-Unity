using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS
{
    internal static class AdTypeCode
    {
        internal const int BANNER = 0;
        internal const int INTER = 1;
        internal const int REWARD = 2;
        internal const int APP_OPEN = 3;
        internal const int APP_RETURN = 5;
    }

    internal static class AdActionCode
    {
        internal const int LOADED = 1;
        internal const int FAILED = 2;
        internal const int SHOWN = 3;
        internal const int IMPRESSION = 4;
        internal const int SHOW_FAILED = 5;
        internal const int CLICKED = 6;
        internal const int COMPLETED = 7;
        internal const int CLOSED = 8;
        internal const int VIEW_RECT = 9;
        internal const int INIT = 10;
    }

    internal abstract class CASManagerBase : IMediationManager
    {
        private AdFlags enabledFormats;
        private readonly List<CASViewBase> adViews = new List<CASViewBase>();

        public string managerID { get; private set; }
        public bool isTestAdMode { get; private set; }
        public CASInitCompleteEvent initCompleteEvent { get; set; }
        public InitialConfiguration initialConfig { get; set; }

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

        public abstract void EnableAd(AdType adType);
        protected abstract void LoadAdNetive(AdType adType);
        public abstract bool IsReadyAd(AdType adType);
        public abstract void ShowAd(AdType adType);
        public abstract void DisposeAd(AdType adType);
        public abstract void SetAppReturnAdsEnabled(bool enable);
        public abstract void SkipNextAppReturnAds();

        protected abstract void SetLastPageAdContentNative(string json);
        protected abstract CASViewBase CreateAdView(AdSize size);
        public abstract AdMetaData WrapImpression(AdType adType, object impression);

        internal virtual void Init(CASInitSettings initSettings)
        {
            managerID = initSettings.targetId;
            isTestAdMode = initSettings.IsTestAdMode();
            enabledFormats = initSettings.allowedAdFlags;
            initCompleteEvent = initSettings.initListener;
        }

        private LastPageAdContent _lastPageAdContent = null;

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                if (_lastPageAdContent != value)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? "" : JsonUtility.ToJson(value);
                    SetLastPageAdContentNative(json);
                }
            }
        }

        public bool IsEnabledAd(AdType adType)
        {
            return IsEnabledAdFlag((AdFlags)(1 << (int)adType));
        }

        internal bool IsEnabledAdFlag(AdFlags adFlags)
        {
            return (enabledFormats & adFlags) == adFlags;
        }

        public void SetEnableAd(AdType adType, bool enabled)
        {
            var adFlag = (AdFlags)(1 << (int)adType);
            if (enabled)
            {
                if ((enabledFormats & adFlag) == adFlag) return;

                enabledFormats |= adFlag;
                if (CASFactory.IsAutoload(adType))
                {
                    if (adType == AdType.Banner)
                    {
                        for (int i = 0; i < adViews.Count; i++)
                            adViews[i].Enable();
                    }
                    else
                    {
                        EnableAd(adType);
                    }
                }
            }
            else
            {
                if ((enabledFormats & adFlag) != adFlag) return;
                enabledFormats &= ~adFlag;
                if (adType == AdType.Banner)
                {
                    for (int i = 0; i < adViews.Count; i++)
                        adViews[i].DestroyNative();
                }
                else
                {
                    DisposeAd(adType);
                }
            }
        }

        public void LoadAd(AdType adType)
        {
            if (IsEnabledAd(adType))
                LoadAdNetive(adType);
            else
                HandleCallback(AdActionCode.FAILED, (int)adType, AdError.ManagerIsDisabled, null, null);
        }

        public IAdView GetAdView(AdSize size)
        {
            if (size < AdSize.Banner)
                throw new ArgumentException("Invalid AdSize " + size.ToString());
            for (int i = 0; i < adViews.Count; i++)
            {
                if (adViews[i].size == size)
                    return adViews[i];
            }
            var view = CreateAdView(size);
            adViews.Add(view);
            if (IsEnabledAdFlag(AdFlags.Banner) && CASFactory.IsAutoload(AdType.Banner))
                view.Enable();
            return view;
        }

        public virtual void RemoveAdViewFromFactory(CASViewBase view)
        {
            adViews.Remove(view);
        }

        internal void OnInitialized(string error, string countryCode, bool isConsentRequired, bool testMode, int consentFlowStatus)
        {
            initialConfig = new InitialConfiguration(error, this, countryCode, isConsentRequired, (ConsentFlow.Status)consentFlowStatus);

            isTestAdMode = testMode;
            try
            {
                if (initCompleteEvent != null)
                    initCompleteEvent(initialConfig);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (error != InitializationError.NoConnection)
            {
                initCompleteEvent = null;
            }

            if (MobileAds.settings.loadingMode != LoadingManagerMode.Manual)
            {
                if (IsEnabledAdFlag(AdFlags.Interstitial))
                    LoadAd(AdType.Interstitial);
                if (IsEnabledAdFlag(AdFlags.Rewarded))
                    LoadAd(AdType.Rewarded);
            }

            CASFactory.OnManagerInitialized(this);
        }

        public void HandleCallback(int action, int type, int error, string errorMessage, object impression)
        {
            switch (action)
            {
                case AdActionCode.LOADED:
                    CASFactory.RuntimeLog(type, "Callback Loaded");
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            if (OnInterstitialAdLoaded != null)
                                OnInterstitialAdLoaded();
                            break;
                        case AdTypeCode.REWARD:
                            if (OnRewardedAdLoaded != null)
                                OnRewardedAdLoaded();
                            break;
                        case AdTypeCode.APP_OPEN:
                            if (OnAppOpenAdLoaded != null)
                                OnAppOpenAdLoaded();
                            break;
                    }
                    break;
                case AdActionCode.FAILED:
                    CASFactory.RuntimeLog(type, "Callback Failed ");
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            if (OnInterstitialAdFailedToLoad != null)
                                OnInterstitialAdFailedToLoad(new AdError(error, errorMessage));
                            break;
                        case AdTypeCode.REWARD:
                            if (OnRewardedAdFailedToLoad != null)
                                OnRewardedAdFailedToLoad(new AdError(error, errorMessage));
                            break;
                        case AdTypeCode.APP_OPEN:
                            if (OnAppOpenAdFailedToLoad != null)
                                OnAppOpenAdFailedToLoad(new AdError(error, errorMessage));
                            break;
                        default:
                            return;
                    }
                    break;
                case AdActionCode.SHOWN:
                    CASFactory.RuntimeLog(type, "Callback Shown");
                    Action shownEvent;
                    CASEventWithMeta openingEvent;
                    AdType openingType;
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            shownEvent = OnInterstitialAdShown;
                            openingEvent = OnInterstitialAdOpening;
                            openingType = AdType.Interstitial;
                            break;
                        case AdTypeCode.REWARD:
                            shownEvent = OnRewardedAdShown;
                            openingEvent = OnRewardedAdOpening;
                            openingType = AdType.Rewarded;
                            break;
                        case AdTypeCode.APP_OPEN:
                            shownEvent = OnAppOpenAdShown;
                            openingEvent = null;
                            openingType = AdType.AppOpen;
                            break;
                        case AdTypeCode.APP_RETURN:
                            shownEvent = OnAppReturnAdShown;
                            openingEvent = OnAppReturnAdOpening;
                            openingType = AdType.Interstitial;
                            break;
                        default:
                            return;
                    }

                    if (shownEvent != null)
                        shownEvent();

                    if (openingEvent != null)
                        openingEvent(WrapImpression(openingType, impression));
                    break;
                case AdActionCode.IMPRESSION:
                    CASFactory.RuntimeLog(type, "Callback Impression");
                    CASEventWithMeta impressionEvent;
                    AdType impressionType;
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            impressionEvent = OnInterstitialAdImpression;
                            impressionType = AdType.Interstitial;
                            break;
                        case AdTypeCode.REWARD:
                            impressionEvent = OnRewardedAdImpression;
                            impressionType = AdType.Rewarded;
                            break;
                        case AdTypeCode.APP_OPEN:
                            impressionEvent = OnAppOpenAdImpression;
                            impressionType = AdType.AppOpen;
                            break;
                        case AdTypeCode.APP_RETURN:
                            impressionEvent = OnAppReturnAdImpression;
                            impressionType = AdType.Interstitial;
                            break;
                        default:
                            return;
                    }
                    if (impressionEvent != null)
                        impressionEvent(WrapImpression(impressionType, impression));
                    break;
                case AdActionCode.SHOW_FAILED:
                    CASFactory.RuntimeLog(type, "Callback Show failed");
                    CASEventWithError showFailedEvent;
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            showFailedEvent = OnInterstitialAdFailedToShow;
                            break;
                        case AdTypeCode.REWARD:
                            showFailedEvent = OnRewardedAdFailedToShow;
                            break;
                        case AdTypeCode.APP_OPEN:
                            showFailedEvent = OnAppOpenAdFailedToShow;
                            break;
                        case AdTypeCode.APP_RETURN:
                            showFailedEvent = OnAppReturnAdFailedToShow;
                            break;
                        default: return;
                    }
                    if (showFailedEvent != null)
                    {
                        if (string.IsNullOrEmpty(errorMessage))
                            showFailedEvent(new AdError(error, null).ToString());
                        else
                            showFailedEvent(errorMessage);
                    }
                    break;
                case AdActionCode.CLICKED:
                    CASFactory.RuntimeLog(type, "Callback Clicked");
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            if (OnInterstitialAdClicked != null)
                                OnInterstitialAdClicked();
                            break;
                        case AdTypeCode.REWARD:
                            if (OnRewardedAdClicked != null)
                                OnRewardedAdClicked();
                            break;
                        case AdTypeCode.APP_OPEN:
                            if (OnAppOpenAdClicked != null)
                                OnAppOpenAdClicked();
                            break;
                        case AdTypeCode.APP_RETURN:
                            if (OnAppReturnAdClicked != null)
                                OnAppReturnAdClicked();
                            break;
                    }
                    break;
                case AdActionCode.COMPLETED:
                    CASFactory.RuntimeLog(type, "Callback Completed");
                    if (type == AdTypeCode.REWARD)
                    {
                        if (OnRewardedAdCompleted != null)
                            OnRewardedAdCompleted();
                    }
                    break;
                case AdActionCode.CLOSED:
                    CASFactory.RuntimeLog(type, "Callback Closed");
                    switch (type)
                    {
                        case AdTypeCode.INTER:
                            if (OnInterstitialAdClosed != null)
                                OnInterstitialAdClosed();
                            break;
                        case AdTypeCode.REWARD:
                            if (OnRewardedAdClosed != null)
                                OnRewardedAdClosed();
                            break;
                        case AdTypeCode.APP_OPEN:
                            if (OnAppOpenAdClosed != null)
                                OnAppOpenAdClosed();
                            break;
                        case AdTypeCode.APP_RETURN:
                            if (OnAppReturnAdClosed != null)
                                OnAppReturnAdClosed();
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException("Action " + action);
            }
        }
    }

    internal abstract class CASViewBase : IAdView
    {
        protected CASManagerBase _manager;
        [SerializeField]
        protected AdPosition _position = AdPosition.BottomCenter;
        [SerializeField]
        protected int _positionX = 0;
        [SerializeField]
        protected int _positionY = 0;

        public event CASViewEvent OnLoaded;
        public event CASViewEventWithError OnFailed;
        public event CASViewEventWithMeta OnImpression;
        public event CASViewEvent OnClicked;

        public IMediationManager manager { get { return _manager; } }
        public AdSize size { get; private set; }
        public Rect rectInPixels { get; set; }

        internal CASViewBase(CASManagerBase manager, AdSize size)
        {
            _manager = manager;
            this.size = size;
        }

        public AdPosition position
        {
            get { return _position; }
            set { SetPosition(0, 0, value); }
        }

        internal abstract void Enable();
        public abstract void LoadNative();
        public abstract void SetActive(bool active);
        public abstract int refreshInterval { get; set; }
        public abstract bool isReady { get; }

        internal abstract void DestroyNative();
        protected abstract void SetPositionNative(int position, int x, int y);
        protected abstract void SetPositionPxNative(int position, int x, int y);

        public void Load()
        {
            if (_manager.IsEnabledAdFlag(AdFlags.Banner))
                LoadNative();
            else
                HandleCallback(AdActionCode.FAILED, 0, AdError.ManagerIsDisabled, null, null);
        }

        public virtual void Dispose()
        {
            _manager.RemoveAdViewFromFactory(this);
            DestroyNative();
        }

        public void DisableRefresh()
        {
            refreshInterval = 0;
        }

        public void SetPosition(int x, int y, AdPosition position)
        {
            if (IsValidPosition(x, y, position))
                SetPositionNative((int)position, _positionX, _positionY);
        }

        public void SetPositionPx(int x, int y, AdPosition position)
        {
            if (IsValidPosition(x, y, position))
                SetPositionPxNative((int)position, _positionX, _positionY);
        }

        private bool IsValidPosition(int x, int y, AdPosition position)
        {
            bool isChanged = false;
            if (position < AdPosition.Undefined && position != _position)
            {
                _position = position;
                isChanged = true;
            }
            var absX = Math.Abs(x);
            var absY = Math.Abs(y);
            if (absX != _positionX || absY != _positionY)
            {
                _positionX = absX;
                _positionY = absY;
                isChanged = true;
            }
            return isChanged;
        }


        public void HandleCallback(int action, int type, int error, string errorMessage, object impression)
        {
            switch (action)
            {
                case AdActionCode.LOADED:
                    CASFactory.RuntimeLog(size, "Callback Loaded ");
                    if (OnLoaded != null)
                        OnLoaded(this);
                    break;
                case AdActionCode.FAILED:
                    CASFactory.RuntimeLog(size, "Callback Failed");
                    if (OnFailed != null)
                        OnFailed(this, new AdError(error, errorMessage));
                    break;
                case AdActionCode.IMPRESSION:
                    CASFactory.RuntimeLog(size, "Callback Impression");
                    if (OnImpression != null)
                        OnImpression(this, _manager.WrapImpression(AdType.Banner, impression));
                    break;
                case AdActionCode.CLICKED:
                    CASFactory.RuntimeLog(size, "Callback Clicked");
                    if (OnClicked != null)
                        OnClicked(this);
                    break;
                default:
                    throw new NotImplementedException("Action " + action);
            }
        }
    }
}