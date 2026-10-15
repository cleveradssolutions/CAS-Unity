//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEditor.Android;
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

        [Tooltip("An optional placement name for the ad instance that helps categorize and track statistics across different ad placements.")]
        [SerializeField]
        private string placement;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private int loadAdOnAwake = 0;

        /// <summary>
        /// Sets the number of <see cref="AdType"/> ads that the SDK keeps loaded at all times.
        /// <para>Loading starts immediately. After an ad is shown, a new ad is loaded automatically to keep the buffer full.</para>
        /// <para>The <see cref="OnAdLoaded"/> is invoked once for each ad loaded into the buffer.</para>
        /// <para>The <paramref name="size"/> is clamped to the range from 1 to 4.</para>
        /// <para>While the buffer is active, <see cref="LoadingManagerMode.Manual"/> is ignored and ads are reloaded automatically.</para>
        /// <para>To stop buffering, call <see cref="LoadAd"/> to load only a single ad,
        /// or call <see cref="DestroyAd"/> to stop loading altogether.</para>
        /// </summary>
        /// <param name="size">The number of ads to keep loaded, from 1 to 4.</param>
        public void SetPreloadedBufferSize(int size)
        {
            if (manager == null)
                loadAdOnAwake = size;
            else
                manager.SetPreloadedBufferSize(AdType.Interstitial, size);
        }

        /// <summary>
        /// Check ready ad to present.
        /// <para>It is not required to check this before calling <see cref="Present"/>:
        /// if the ad cannot be shown, the <see cref="OnAdFailedToShow"/> event will be invoked.</para>
        /// </summary>
        public bool IsAdLoaded()
        {
            return manager != null && manager.IsReadyAd(AdType.Interstitial);
        }

        /// <summary>
        /// Manual load Ad.
        /// <para>Please call load before each show ad when active load mode is <see cref="LoadingManagerMode.Manual"/>.</para>
        /// <para>You can get a callback for the successful loading of an ad by subscribe <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
                loadAdOnAwake = 1;
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
            manager.ShowAd(AdType.Interstitial, placement);
        }

        /// <summary>
        /// Disposes of the ad of the Interstitial Ads.
        /// <para>After calling this method, if you want to resume showing ads, 
        /// you must call the <see cref="LoadAd"/> method to reload the ad. 
        /// Note that in automatic loading mode, ads will not be automatically reloaded after Dispose. 
        /// You need to explicitly call <see cref="LoadAd"/> to prepare the ad for future displays.</para>
        public void DestroyAd()
        {
            if (manager == null) return;
            manager.DisposeAd(AdType.Interstitial);
        }
        
        /// <summary>
        /// Renamed to <see cref="IsAdLoaded"/>
        /// </summary>
        public bool isAdReady
        {
            get { return IsAdLoaded(); }
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
                else if (loadAdOnAwake == 1)
                {
                    loadAdOnAwake = 0;
                    manager.LoadAd(AdType.Interstitial);
                }
                if (loadAdOnAwake > 1)
                {
                    loadAdOnAwake = 0;
                    manager.SetPreloadedBufferSize(AdType.Interstitial, loadAdOnAwake);
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