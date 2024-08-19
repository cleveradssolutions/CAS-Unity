//  Copyright © 2024 CAS.AI. All rights reserved.

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
                _allowReturnToPlayAd = value;
                if (manager != null)
                {
                    manager.SetAppReturnAdsEnabled(value);
                }
            }
        }

        #region MonoBehaviour
        private void Start()
        {
            if (!CASFactory.TryGetManagerByIndexAsync(managerId.index, OnManagerReady))
                OnAdFailedToLoad.Invoke(new AdError(AdError.NotInitialized, null).ToString());
        }

        private void OnDestroy()
        {
            if (manager == null)
            {
                CASFactory.OnManagerStateChanged -= OnManagerReady;
            }
            else
            {
                manager.OnInterstitialAdLoaded -= OnAdLoaded.Invoke;
                manager.OnInterstitialAdFailedToLoad -= AdFailedToLoad;
                manager.OnAppReturnAdFailedToShow -= AdFailedToShow;
                manager.OnAppReturnAdShown -= OnAdShown.Invoke;
                manager.OnAppReturnAdClicked -= OnAdClicked.Invoke;
                manager.OnAppReturnAdClosed -= OnAdClosed.Invoke;
                manager.OnAppReturnAdImpression -= OnAdImpression.Invoke;
            }
        }
        #endregion

        #region Manager Events wrappers
        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            CASFactory.OnManagerStateChanged -= OnManagerReady;
            this.manager = manager;

            allowReturnToPlayAd = _allowReturnToPlayAd;

            manager.OnInterstitialAdLoaded += OnAdLoaded.Invoke;
            manager.OnInterstitialAdFailedToLoad += AdFailedToLoad;
            manager.OnAppReturnAdFailedToShow += AdFailedToShow;
            manager.OnAppReturnAdShown += OnAdShown.Invoke;
            manager.OnAppReturnAdClicked += OnAdClicked.Invoke;
            manager.OnAppReturnAdClosed += OnAdClosed.Invoke;
            manager.OnAppReturnAdImpression += OnAdImpression.Invoke;

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

        private void AdFailedToLoad(AdError error)
        {
            OnAdFailedToLoad.Invoke(error.GetMessage());
        }

        private void AdFailedToShow(string error)
        {
            OnAdFailedToShow.Invoke(error);
            OnAdClosed.Invoke();
        }
        #endregion
    }
}
