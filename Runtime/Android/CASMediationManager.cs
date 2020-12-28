#if UNITY_ANDROID || CASDeveloper
using System;
using UnityEngine;

namespace CAS.Android
{
    internal class CASMediationManager : IMediationManager
    {
        private AdCallbackProxy _bannerProxy;
        private AdCallbackProxy _interstitialProxy;
        private AdCallbackProxy _rewardedProxy;
        private AdLoadCallbackProxy _adLoadProxy;
        private AndroidJavaObject _managerBridge;

        private AdSize _bannerSize = AdSize.Banner;
        private AdPosition _bannerPosition = AdPosition.BottomCenter;
        private LastPageAdContent _lastPageAdContent = null;

        public InitializationListenerProxy initializationListener;
        public string managerID { get; }
        public bool isTestAdMode { get; }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd
        {
            add { _adLoadProxy.OnLoadedAd += value; }
            remove { _adLoadProxy.OnLoadedAd -= value; }
        }
        public event CASTypedEventWithError OnFailedToLoadAd
        {
            add { _adLoadProxy.OnFailedToLoadAd += value; }
            remove { _adLoadProxy.OnFailedToLoadAd -= value; }
        }

        public event Action OnBannerAdShown
        {
            add { _bannerProxy.OnAdShown += value; }
            remove { _bannerProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnBannerAdFailedToShow
        {
            add { _bannerProxy.OnAdFailedToShow += value; }
            remove { _bannerProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnBannerAdClicked
        {
            add { _bannerProxy.OnAdClicked += value; }
            remove { _bannerProxy.OnAdClicked -= value; }
        }
        public event Action OnBannerAdHidden
        {
            add { _bannerProxy.OnAdClosed += value; }
            remove { _bannerProxy.OnAdClosed -= value; }
        }

        public event Action OnInterstitialAdShown
        {
            add { _interstitialProxy.OnAdShown += value; }
            remove { _interstitialProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnInterstitialAdFailedToShow
        {
            add { _interstitialProxy.OnAdFailedToShow += value; }
            remove { _interstitialProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnInterstitialAdClicked
        {
            add { _interstitialProxy.OnAdClicked += value; }
            remove { _interstitialProxy.OnAdClicked -= value; }
        }
        public event Action OnInterstitialAdClosed
        {
            add { _interstitialProxy.OnAdClosed += value; }
            remove { _interstitialProxy.OnAdClosed -= value; }
        }

        public event Action OnRewardedAdShown
        {
            add { _rewardedProxy.OnAdShown += value; }
            remove { _rewardedProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnRewardedAdFailedToShow
        {
            add { _rewardedProxy.OnAdFailedToShow += value; }
            remove { _rewardedProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnRewardedAdClicked
        {
            add { _rewardedProxy.OnAdClicked += value; }
            remove { _rewardedProxy.OnAdClicked -= value; }
        }
        public event Action OnRewardedAdCompleted
        {
            add { _rewardedProxy.OnAdCompleted += value; }
            remove { _rewardedProxy.OnAdCompleted -= value; }
        }
        public event Action OnRewardedAdClosed
        {
            add { _rewardedProxy.OnAdClosed += value; }
            remove { _rewardedProxy.OnAdClosed -= value; }
        }
        #endregion

        public CASMediationManager( CASInitSettings initData )
        {
            managerID = initData.targetId;
            isTestAdMode = initData.testAdMode;
        }

        ~CASMediationManager()
        {
            try
            {
                _managerBridge.Call( "freeManager" );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        public void CreateManager( CASInitSettings initData )
        {
            _bannerProxy = new AdCallbackProxy();
            _interstitialProxy = new AdCallbackProxy();
            _rewardedProxy = new AdCallbackProxy();
            _adLoadProxy = new AdLoadCallbackProxy();

            if (initData.initListener != null)
                initializationListener = new InitializationListenerProxy( this, initData.initListener );

            AndroidJavaObject activity = CASJavaProxy.GetUnityActivity();

            if (initData.extrasKeys != null && initData.extrasValues != null
                && initData.extrasKeys.Count != 0 && initData.extrasValues.Count != 0)
            {
                _managerBridge = new AndroidJavaObject( CASJavaProxy.NativeBridgeClassName,
                    activity,
                    managerID,
                    ( int )initData.allowedAdFlags,
                    isTestAdMode,
                    initializationListener,
                    CASJavaProxy.GetJavaListObject( initData.extrasKeys ),
                    CASJavaProxy.GetJavaListObject( initData.extrasValues )
                );
            }
            else
            {
                _managerBridge = new AndroidJavaObject( CASJavaProxy.NativeBridgeClassName,
                    activity,
                    managerID,
                    ( int )initData.allowedAdFlags,
                    isTestAdMode,
                    initializationListener );
            }

            _managerBridge.Call( "setListeners",
                _bannerProxy,
                _interstitialProxy,
                _rewardedProxy );

            _managerBridge.Call( "addAdLoadListener", _adLoadProxy );
        }

        public AdSize bannerSize
        {
            get { return _bannerSize; }
            set
            {
                if (_bannerSize != value && _bannerSize != 0)
                {
                    _bannerSize = value;
                    _managerBridge.Call( "setBannerSizeId", ( int )value );
                }
            }
        }

        public AdPosition bannerPosition
        {
            get { return _bannerPosition; }
            set
            {
                if (_bannerPosition != value && value != AdPosition.Undefined)
                {
                    _bannerPosition = value;
                    _managerBridge.Call( "setBannerPositionId", ( int )value );
                }
            }
        }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                if (value != _lastPageAdContent)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? null : JsonUtility.ToJson( value );
                    _managerBridge.Call( "setLastPageAdContent", json );
                }
            }
        }

        public string GetLastActiveMediation( AdType adType )
        {
            return _managerBridge.Call<string>( "getLastActiveMediation", ( int )adType );
        }

        public void HideBanner()
        {
            _managerBridge.Call( "hideBanner" );
        }

        public bool IsEnabledAd( AdType adType )
        {
            return _managerBridge.Call<bool>( "isEnabled", ( int )adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            return _managerBridge.Call<bool>( "isAdReady", ( int )adType );
        }

        public void LoadAd( AdType adType )
        {
            _managerBridge.Call( "loadAd", ( int )adType );
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            _managerBridge.Call( "enableAd", ( int )adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
            _managerBridge.Call( "showAd", ( int )adType );
        }

        public float GetBannerHeightInPixels()
        {
            return _managerBridge.Call<int>( "getBannerHeightInPixels" );
        }

        public float GetBannerWidthInPixels()
        {
            return _managerBridge.Call<int>( "getBannerWidthInPixels" );
        }
    }
}
#endif