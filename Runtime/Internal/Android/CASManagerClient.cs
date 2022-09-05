//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;
using System.Collections.Generic;

namespace CAS.Android
{
    internal class CASManagerClient : CASViewFactory, IMediationManager
    {
        private AdEventsProxy _interstitialProxy;
        private AdEventsProxy _rewardedProxy;
        private AdEventsProxy _returnAdProxy;
        private AndroidJavaObject _managerBridge;
        private LastPageAdContent _lastPageAdContent = null;

        private InitCompleteAction _initCompleteAction;
        private InitCallbackProxy _initListener;

        public string managerID { get; private set; }
        public bool isTestAdMode { get; private set; }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd;
        public event CASTypedEventWithError OnFailedToLoadAd;

        public event Action OnInterstitialAdLoaded
        {
            add { _interstitialProxy.OnAdLoaded += value; }
            remove { _interstitialProxy.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnInterstitialAdFailedToLoad
        {
            add { _interstitialProxy.OnAdFailed += value; }
            remove { _interstitialProxy.OnAdFailed -= value; }
        }
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


        public event Action OnRewardedAdLoaded
        {
            add { _rewardedProxy.OnAdLoaded += value; }
            remove { _rewardedProxy.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnRewardedAdFailedToLoad
        {
            add { _rewardedProxy.OnAdFailed += value; }
            remove { _rewardedProxy.OnAdFailed -= value; }
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

        internal CASManagerClient() { }

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

        internal CASManagerClient Init( CASInitSettings initData )
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
            _initCompleteAction = initData.initListener;
            _initListener = new InitCallbackProxy( this );
            _interstitialProxy = new AdEventsProxy( AdType.Interstitial );
            _interstitialProxy.OnAdLoaded += CallbackOnInterLoaded;
            _interstitialProxy.OnAdFailed += CallbackOnInterFailed;
            _rewardedProxy = new AdEventsProxy( AdType.Rewarded );
            _rewardedProxy.OnAdLoaded += CallbackOnRewardLoaded;
            _rewardedProxy.OnAdFailed += CallbackOnRewardFailed;
            _returnAdProxy = new AdEventsProxy( AdType.Interstitial );

            using (var builder = new AndroidJavaObject( CASJavaBridge.bridgeBuilderClass ))
            {
                if (initData.IsTestAdMode())
                    builder.Call( "enableTestMode" );

                if (!string.IsNullOrEmpty( initData.userID ))
                    builder.Call( "setUserId", initData.userID );

                CASJavaBridge.RepeatCall( "addExtras", builder, initData.extras, false );


                builder.Call( "setCallbacks", _initListener, _interstitialProxy, _rewardedProxy );

                _managerBridge = builder.Call<AndroidJavaObject>( "build",
                    initData.targetId, Application.unityVersion, (int)initData.allowedAdFlags );
            }
            return this;
        }

        public void OnInitializationCallback( string error, bool testMode )
        {
            CASFactory.UnityLog( "OnInitialization " + error );
            isTestAdMode = testMode;
            if (_initCompleteAction != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (string.IsNullOrEmpty( error ))
                        _initCompleteAction( true, null );
                    else
                        _initCompleteAction( false, error );

                    _initCompleteAction = null;
                } );
            }
            _initListener = null;
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
            return string.Empty;
        }

        public bool IsEnabledAd( AdType adType )
        {
            return _managerBridge.Call<bool>( "isEnabled", (int)adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    return globalView != null && globalView.isReady;
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
                    LoadGlobalBanner();
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
            _managerBridge.Call( "enableAd", (int)adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    ShowGlobalBanner();
                    break;
                case AdType.Interstitial:
                    _managerBridge.Call( "showInterstitial" );
                    break;
                case AdType.Rewarded:
                    _managerBridge.Call( "showRewarded" );
                    break;
            }
        }

        [UnityEngine.Scripting.Preserve]
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
            var bridge = _managerBridge.Call<AndroidJavaObject>( "createAdView", callback, (int)size );
            return new CASViewClient( this, size, bridge, callback );
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
