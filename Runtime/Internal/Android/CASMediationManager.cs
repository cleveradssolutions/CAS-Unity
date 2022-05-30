//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;
using System.Collections.Generic;

namespace CAS.Android
{
    internal class CASMediationManager : CASViewFactory, IMediationManager
    {
        private AdEventsProxy _interstitialProxy;
        private AdEventsProxy _rewardedProxy;
        private AdEventsProxy _returnAdProxy;
        private AndroidJavaObject _managerBridge;
        private LastPageAdContent _lastPageAdContent = null;

        public InitializationListenerProxy initializationListener;

        public string managerID { get; private set; }
        public bool isTestAdMode { get; private set; }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd;
        public event CASTypedEventWithError OnFailedToLoadAd;

        public event Action OnInterstitialAdShown
        {
            add { _interstitialProxy.OnAdShown += value; }
            remove { _interstitialProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdOpening
        {
            add { _interstitialProxy.OnAdOpening += value; }
            remove { _interstitialProxy.OnAdOpening -= value; }
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
        public event CASEventWithMeta OnRewardedAdOpening
        {
            add { _rewardedProxy.OnAdOpening += value; }
            remove { _rewardedProxy.OnAdOpening -= value; }
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

        public event Action OnAppReturnAdShown
        {
            add { _returnAdProxy.OnAdShown += value; }
            remove { _returnAdProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnAppReturnAdOpening
        {
            add { _returnAdProxy.OnAdOpening += value; }
            remove { _returnAdProxy.OnAdOpening -= value; }
        }
        public event CASEventWithError OnAppReturnAdFailedToShow
        {
            add { _returnAdProxy.OnAdFailedToShow += value; }
            remove { _returnAdProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnAppReturnAdClicked
        {
            add { _returnAdProxy.OnAdClicked += value; }
            remove { _returnAdProxy.OnAdClicked -= value; }
        }
        public event Action OnAppReturnAdClosed
        {
            add { _returnAdProxy.OnAdClosed += value; }
            remove { _returnAdProxy.OnAdClosed -= value; }
        }
        #endregion

        public CASMediationManager( CASInitSettings initData )
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
        }

#if false // Manager store in Static memory and disposed only on application destroed
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
#endif

        public void CreateManager( CASInitSettings initData )
        {
            _interstitialProxy = new AdEventsProxy( AdType.Interstitial );
            _interstitialProxy.OnAdLoaded += CallbackOnInterLoaded;
            _interstitialProxy.OnAdFailed += CallbackOnInterFailed;
            _rewardedProxy = new AdEventsProxy( AdType.Rewarded );
            _rewardedProxy.OnAdLoaded += CallbackOnRewardLoaded;
            _rewardedProxy.OnAdFailed += CallbackOnRewardFailed;
            _returnAdProxy = new AdEventsProxy( AdType.Interstitial );

            AndroidJavaObject activity = CASJavaProxy.GetUnityActivity();
            _managerBridge = new AndroidJavaObject( CASJavaProxy.NativeBridgeClassName, activity );

            var builder = _managerBridge.Call<AndroidJavaObject>(
                "createBuilder",
                managerID,
                Application.unityVersion,
                ( int )initData.allowedAdFlags );

            using (builder)
            {
                if (isTestAdMode)
                    builder.Call<AndroidJavaObject>( "withTestAdMode", true );

                if (!string.IsNullOrEmpty( initData.userID ))
                    builder.Call<AndroidJavaObject>( "withUserID", initData.userID );

                if (initData.initListener != null)
                {
                    initializationListener = new InitializationListenerProxy( this, initData.initListener );
                    builder.Call<AndroidJavaObject>( "withInitListener", initializationListener );
                }

                if (initData.extras != null && initData.extras.Count != 0)
                {
                    var extrasParams = CASFactory.SerializeParametersString( initData.extras );
                    _managerBridge.Call( "setMediationExtras", extrasParams );
                }

                _managerBridge.Call( "initialize", _interstitialProxy, _rewardedProxy );
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

        public bool IsEnabledAd( AdType adType )
        {
            return _managerBridge.Call<bool>( "isEnabled", ( int )adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    return IsGlobalViewReady();
                case AdType.Interstitial:
                    return _managerBridge.Call<bool>( "isInterstitialAdReady" );
                case AdType.Rewarded:
                    return _managerBridge.Call<bool>( "isRewardedAdReady" );
                default:
                    return false;
            }
        }

        public void LoadAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    GetOrCreateGlobalView().Load();
                    break;
                case AdType.Interstitial:
                    _managerBridge.Call( "loadInterstitial" );
                    break;
                case AdType.Rewarded:
                    _managerBridge.Call( "loadRewarded" );
                    break;
            }
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            _managerBridge.Call( "enableAd", ( int )adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    ShowBanner();
                    break;
                case AdType.Interstitial:
                    _managerBridge.Call( "showInterstitial" );
                    break;
                case AdType.Rewarded:
                    _managerBridge.Call( "showRewarded" );
                    break;
            }
        }

        public bool TryOpenDebugger()
        {
            return _managerBridge.Call<bool>( "tryOpenDebugger" );
        }

        public void SetAppReturnAdsEnabled( bool enable )
        {
            if (enable)
                _managerBridge.Call( "enableReturnAds", _returnAdProxy );
            else
                _managerBridge.Call( "disableReturnAds" );
        }

        public void SkipNextAppReturnAds()
        {
            _managerBridge.Call( "skipNextReturnAds" );
        }

        protected override IAdView CreateAdView( AdSize size )
        {
            var callback = new AdEventsProxy( AdType.Banner );
            var bridge = _managerBridge.Call<AndroidJavaObject>( "createAdView", callback, ( int )size );
            return new CASView( this, size, bridge, callback );
        }


        #region Ad load callback wrapping
        public override void OnLoadedCallback( AdType type )
        {
            if (OnLoadedAd != null)
                OnLoadedAd( type );
        }

        public override void OnFailedCallback( AdType type, AdError error )
        {
            if (OnFailedToLoadAd != null)
                OnFailedToLoadAd( type, error.GetMessage() );
        }

        private void CallbackOnRewardFailed( AdError error )
        {
            OnFailedCallback( AdType.Rewarded, error );
        }

        private void CallbackOnRewardLoaded()
        {
            OnLoadedCallback( AdType.Rewarded );
        }

        private void CallbackOnInterFailed( AdError error )
        {
            OnFailedCallback( AdType.Interstitial, error );
        }

        private void CallbackOnInterLoaded()
        {
            OnLoadedCallback( AdType.Interstitial );
        }
        #endregion
    }
}
#endif
