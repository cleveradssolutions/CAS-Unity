//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Interstitial Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Interstitial-Ad-object")]
    public sealed class InterstitialAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private bool loadAdOnAwake = false;

        /// <summary>
        /// Check ready ad to present.
        /// </summary>
        public bool isAdReady
        {
            get { return manager != null && manager.IsReadyAd(AdType.Interstitial); }
        }

        /// <summary>
        /// Manual load Ad.
        /// <para>Please call load before each show ad when active load mode is <see cref="LoadingManagerMode.Manual"/>.</para>
        /// <para>You can get a callback for the successful loading of an ad by subscribe <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
                loadAdOnAwake = true;
            else
                manager.LoadAd(AdType.Interstitial);
        }

        /// <summary>
        /// Present ad to user
        /// </summary>
        public void Present()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke(new AdError(AdError.NotInitialized, null).ToString());
                return;
            }
            OnAdShown.Invoke();
            manager.ShowAd(AdType.Interstitial);
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
                manager.OnInterstitialAdFailedToShow -= AdFailedToShow;
                manager.OnInterstitialAdClicked -= OnAdClicked.Invoke;
                manager.OnInterstitialAdClosed -= OnAdClosed.Invoke;
                manager.OnInterstitialAdImpression -= OnAdImpression.Invoke;
            }

        }
        #endregion

        #region Manager Events wrappers
        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            CASFactory.OnManagerStateChanged -= OnManagerReady;

            this.manager = manager;
            manager.OnInterstitialAdLoaded += OnAdLoaded.Invoke;
            manager.OnInterstitialAdFailedToLoad += AdFailedToLoad;
            manager.OnInterstitialAdFailedToShow += AdFailedToShow;
            manager.OnInterstitialAdClicked += OnAdClicked.Invoke;
            manager.OnInterstitialAdClosed += OnAdClosed.Invoke;
            manager.OnInterstitialAdImpression += OnAdImpression.Invoke;

            try
            {
                if (manager.IsReadyAd(AdType.Interstitial))
                {
                    OnAdLoaded.Invoke();
                }
                else if (loadAdOnAwake)
                {
                    loadAdOnAwake = false;
                    manager.LoadAd(AdType.Interstitial);
                }
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