#if UNITY_ANDROID || CASDeveloper
using System;
using UnityEngine;

namespace CAS.Android
{
    internal class CASMediationManager : IMediationManager
    {
        private AdCallbackProxy bannerProxy;
        private AdCallbackProxy interstitialProxy;
        private AdCallbackProxy rewardedProxy;
        private AdLoadCallbackProxy adLoadProxy;
        public InitializationListenerProxy initializationListener;

        private AndroidJavaObject managerBridge;

        private AdSize _bannerSize = AdSize.Banner;
        private AdPosition _bannerPosition = AdPosition.Undefined;

        public string managerID { get; }
        public bool isTestAdMode { get; }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd
        {
            add { adLoadProxy.OnLoadedAd += value; }
            remove { adLoadProxy.OnLoadedAd -= value; }
        }
        public event CASTypedEventWithError OnFailedToLoadAd
        {
            add { adLoadProxy.OnFailedToLoadAd += value; }
            remove { adLoadProxy.OnFailedToLoadAd -= value; }
        }

        public event Action OnBannerAdShown
        {
            add { bannerProxy.OnAdShown += value; }
            remove { bannerProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnBannerAdFailedToShow
        {
            add { bannerProxy.OnAdFailedToShow += value; }
            remove { bannerProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnBannerAdClicked
        {
            add { bannerProxy.OnAdClicked += value; }
            remove { bannerProxy.OnAdClicked -= value; }
        }
        public event Action OnBannerAdHidden
        {
            add { bannerProxy.OnAdClosed += value; }
            remove { bannerProxy.OnAdClosed -= value; }
        }

        public event Action OnInterstitialAdShown
        {
            add { interstitialProxy.OnAdShown += value; }
            remove { interstitialProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnInterstitialAdFailedToShow
        {
            add { interstitialProxy.OnAdFailedToShow += value; }
            remove { interstitialProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnInterstitialAdClicked
        {
            add { interstitialProxy.OnAdClicked += value; }
            remove { interstitialProxy.OnAdClicked -= value; }
        }
        public event Action OnInterstitialAdClosed
        {
            add { interstitialProxy.OnAdClosed += value; }
            remove { interstitialProxy.OnAdClosed -= value; }
        }

        public event Action OnRewardedAdShown
        {
            add { rewardedProxy.OnAdShown += value; }
            remove { rewardedProxy.OnAdShown -= value; }
        }
        public event CASEventWithError OnRewardedAdFailedToShow
        {
            add { rewardedProxy.OnAdFailedToShow += value; }
            remove { rewardedProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnRewardedAdClicked
        {
            add { rewardedProxy.OnAdClicked += value; }
            remove { rewardedProxy.OnAdClicked -= value; }
        }
        public event Action OnRewardedAdCompleted
        {
            add { rewardedProxy.OnAdCompleted += value; }
            remove { rewardedProxy.OnAdCompleted -= value; }
        }
        public event Action OnRewardedAdClosed
        {
            add { rewardedProxy.OnAdClosed += value; }
            remove { rewardedProxy.OnAdClosed -= value; }
        }
        #endregion

        public CASMediationManager( string managerID, bool isDemoAdMode )
        {
            this.managerID = managerID;
            this.isTestAdMode = isDemoAdMode;
        }

        ~CASMediationManager() {
            managerBridge.Call( "freeManager" );
        }

        public void CreateManager( AdFlags enableAd, InitCompleteAction initCompleteAction )
        {
            bannerProxy = new AdCallbackProxy();
            interstitialProxy = new AdCallbackProxy();
            rewardedProxy = new AdCallbackProxy();
            adLoadProxy = new AdLoadCallbackProxy();

            if (initCompleteAction != null)
                initializationListener = new InitializationListenerProxy( this, initCompleteAction );

            var androidSettings = MobileAds.settings as CASSettings;
            androidSettings.SetUnityVersion();

            AndroidJavaClass playerClass = new AndroidJavaClass( CASJavaProxy.UnityActivityClassName );
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>( "currentActivity" );

            managerBridge = new AndroidJavaObject( CASJavaProxy.NativeBridgeClassName,
                activity,
                managerID,
                ( int )enableAd,
                isTestAdMode,
                initializationListener );

            managerBridge.Call( "setListeners",
                bannerProxy,
                interstitialProxy,
                rewardedProxy );

            managerBridge.Call( "addAdLoadListener", adLoadProxy );
        }

        public AdSize bannerSize
        {
            get { return _bannerSize; }
            set
            {
                if (_bannerSize != value && _bannerSize != 0)
                {
                    _bannerSize = value;
                    managerBridge.Call( "setBannerSizeId", ( int )value );
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
                    managerBridge.Call( "setBannerPositionId", ( int )value );
                }
            }
        }

        public string GetLastActiveMediation( AdType adType )
        {
            return managerBridge.Call<string>( "getLastActiveMediation", ( int )adType );
        }

        public void HideBanner()
        {
            managerBridge.Call( "hideBanner" );
        }

        public bool IsEnabledAd( AdType adType )
        {
            return managerBridge.Call<bool>( "isEnabled", ( int )adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            return managerBridge.Call<bool>( "isAdReady", ( int )adType );
        }

        public void LoadAd( AdType adType )
        {
            managerBridge.Call( "loadAd", ( int )adType );
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            managerBridge.Call( "enableAd", ( int )adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
            managerBridge.Call( "showAd", ( int )adType );
        }
    }
}
#endif