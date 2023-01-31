//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Return To Play Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Return-To-Play-Ad-Object")]
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

        public CASUEventWithMeta OnAdImpression;

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
                        manager.SetAppReturnAdsEnabled(_allowReturnToPlayAd);
                    else
                        CASFactory.TryGetManagerByIndexAsync(OnManagerReady, managerId.index);
                }
            }
        }

        #region MonoBehaviour
        private void Start()
        {
            MobileAds.settings.isExecuteEventsOnUnityThread = true;
            if (!CASFactory.TryGetManagerByIndexAsync(OnManagerReady, managerId.index))
                OnAdFailedToLoad.Invoke(AdError.ManagerIsDisabled.GetMessage());
        }

        private void OnManagerReady(IMediationManager manager)
        {
            manager.SetAppReturnAdsEnabled(_allowReturnToPlayAd);

            if (!this) // When object are destroyed
                return;

            this.manager = manager;
            manager.OnInterstitialAdLoaded += OnAdLoaded.Invoke;
            manager.OnInterstitialAdFailedToLoad += OnInterstitialAdFailedToLoad;
            manager.OnAppReturnAdFailedToShow += OnInterstitialAdFailedToShow;
            manager.OnAppReturnAdShown += OnAdShown.Invoke;
            manager.OnAppReturnAdClicked += OnAdClicked.Invoke;
            manager.OnAppReturnAdClosed += OnAdClosed.Invoke;
            manager.OnRewardedAdImpression -= OnAdImpression.Invoke;

            try
            {
                if (manager.IsReadyAd(AdType.Interstitial))
                    OnAdLoaded.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnInterstitialAdLoaded -= OnAdLoaded.Invoke;
                manager.OnInterstitialAdFailedToLoad -= OnInterstitialAdFailedToLoad;
                manager.OnAppReturnAdFailedToShow -= OnInterstitialAdFailedToShow;
                manager.OnAppReturnAdShown -= OnAdShown.Invoke;
                manager.OnAppReturnAdClicked -= OnAdClicked.Invoke;
                manager.OnAppReturnAdClosed -= OnAdClosed.Invoke;
                manager.OnRewardedAdImpression -= OnAdImpression.Invoke;
            }
        }
        #endregion

        #region Manager Events wrappers
        private void OnInterstitialAdFailedToLoad(AdError error)
        {
            OnAdFailedToLoad.Invoke(error.GetMessage());
        }
        private void OnInterstitialAdFailedToShow(string error)
        {
            OnAdFailedToShow.Invoke(error);
            OnAdClosed.Invoke();
        }
        #endregion
    }
}
