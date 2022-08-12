//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu( "CleverAdsSolutions/Return To Play Ad Object" )]
    [DisallowMultipleComponent]
    [HelpURL( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Return-To-Play-Ad-Object" )]
    public class ReturnToPlayAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;
        [SerializeField]
        private bool _allowReturnToPlayAd = true;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        private IMediationManager manager;

        public bool allowReturnToPlayAd
        {
            get { return _allowReturnToPlayAd; }
            set
            {
                if (_allowReturnToPlayAd != value)
                {
                    _allowReturnToPlayAd = value;
                    if (manager != null)
                        manager.SetAppReturnAdsEnabled( _allowReturnToPlayAd );
                    else
                        CASFactory.TryGetManagerByIndexAsync( OnManagerReady, managerId.index );
                }
            }
        }

        #region MonoBehaviour
        private void Start()
        {
            MobileAds.settings.isExecuteEventsOnUnityThread = true;
            if (!CASFactory.TryGetManagerByIndexAsync( OnManagerReady, managerId.index ))
                OnAdFailedToLoad.Invoke( "Manager not initialized yet" );
        }

        private void OnManagerReady( IMediationManager manager )
        {
            manager.SetAppReturnAdsEnabled( _allowReturnToPlayAd );

            if (!this) // When object are destroyed
                return;

            this.manager = manager;
            manager.OnLoadedAd += OnInterstitialAdLoaded;
            manager.OnFailedToLoadAd += OnInterstitialAdFailedToLoad;
            manager.OnAppReturnAdFailedToShow += OnAdFailedToShow.Invoke;
            manager.OnAppReturnAdShown += OnAdShown.Invoke;
            manager.OnAppReturnAdClicked += OnAdClicked.Invoke;
            manager.OnAppReturnAdClosed += OnAdClosed.Invoke;

            try
            {
                if (manager.IsReadyAd( AdType.Interstitial ))
                    OnAdLoaded.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnLoadedAd -= OnInterstitialAdLoaded;
                manager.OnFailedToLoadAd -= OnInterstitialAdFailedToLoad;
                manager.OnAppReturnAdFailedToShow -= OnAdFailedToShow.Invoke;
                manager.OnAppReturnAdShown -= OnAdShown.Invoke;
                manager.OnAppReturnAdClicked -= OnAdClicked.Invoke;
                manager.OnAppReturnAdClosed -= OnAdClosed.Invoke;
            }
        }
        #endregion

        #region Manager Events wrappers
        private void OnInterstitialAdFailedToLoad( AdType adType, string error )
        {
            if (adType == AdType.Interstitial)
                OnAdFailedToLoad.Invoke( error );
        }

        private void OnInterstitialAdLoaded( AdType adType )
        {
            if (adType == AdType.Interstitial)
                OnAdLoaded.Invoke();
        }
        #endregion
    }
}
