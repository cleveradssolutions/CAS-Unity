//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that enables Return To Play ads without code:
    /// the SDK shows an Interstitial ad automatically when the user returns to the app.
    /// <para>Callbacks are configured in the Inspector as <see cref="UnityEvent"/>s.</para>
    /// <para>Return To Play ads use the Interstitial ads of the manager,
    /// so the load events are shared with <see cref="InterstitialAdObject"/>.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Return To Play Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Return-To-Play-Ad-Object")]
    public class ReturnToPlayAdObject : MonoBehaviour
    {
        /// <summary>
        /// The index of the <see cref="IMediationManager"/> that provides ads for this component.
        /// </summary>
        public ManagerIndex managerId;
        [Tooltip("Show an ad automatically when the user returns to the app.")]
        [SerializeField]
        private bool _allowReturnToPlayAd = true;

        /// <summary>
        /// Invoked when an Interstitial ad is loaded and ready to show.
        /// <para>Also invoked right after initialization if the manager already has a loaded ad.</para>
        /// </summary>
        public UnityEvent OnAdLoaded;
        /// <summary>
        /// Invoked with the error message when an Interstitial ad fails to load,
        /// or when the manager could not be found on <c>Start</c>.
        /// </summary>
        public CASUEventWithError OnAdFailedToLoad;
        /// <summary>
        /// Invoked with the error message when a Return To Play ad fails to show.
        /// <see cref="OnAdClosed"/> is invoked right after it.
        /// </summary>
        public CASUEventWithError OnAdFailedToShow;
        /// <summary>
        /// Invoked when a Return To Play ad is shown.
        /// </summary>
        public UnityEvent OnAdShown;
        /// <summary>
        /// Invoked when the user clicks the ad.
        /// </summary>
        public UnityEvent OnAdClicked;
        /// <summary>
        /// Invoked when the ad is closed, including after <see cref="OnAdFailedToShow"/>.
        /// </summary>
        public UnityEvent OnAdClosed;

        /// <summary>
        /// Invoked when the ad impression is recorded, with the ad metadata for revenue tracking.
        /// </summary>
        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;

        /// <summary>
        /// Enables or disables automatic Return To Play ads.
        /// <para>If the manager is not initialized yet, the value is applied once it is ready.</para>
        /// </summary>
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
