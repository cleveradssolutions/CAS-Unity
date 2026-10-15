//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Rewarded Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Rewarded-Ad-object")]
    public sealed class RewardedAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        [Tooltip("An optional placement name for the ad instance that helps categorize and track statistics across different ad placements.")]
        [SerializeField]
        private string placement;

        public bool restartInterstitialInterval = false;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        public CASUEventWithMeta OnAdImpression;

        public UnityEvent OnReward;

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
                manager.SetPreloadedBufferSize(AdType.Rewarded, size);
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
                manager.LoadAd(AdType.Rewarded);
        }

        /// <summary>
        /// Check ready ad to present.
        /// <para>It is not required to check this before calling <see cref="Present"/>:
        /// if the ad cannot be shown, the <see cref="OnAdFailedToShow"/> event will be invoked.</para>
        /// </summary>
        public bool IsAdLoaded()
        {
            return manager != null && manager.IsReadyAd(AdType.Rewarded);
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
            manager.ShowAd(AdType.Rewarded, placement);
        }

        /// <summary>
        /// Sets custom data to be included in server-side verification callbacks.
        /// <para>Maximum 8192 characters allowed for the custom data.</para>
        /// <para>The callbacks contain query parameters that describe the rewarded ad interaction,
        /// including the placement and userID from CAS.targetingOptions alongside any custom data provided here.</para>
        /// <para>This feature is currently in closed beta.</para>
        /// </summary>
        public void SetServerSideVerificationData(string data)
        {
            if (manager != null)
            {
                manager.SetServerSideVerificationData(data);
            }
        }

        /// <summary>
        /// Disposes of the ad of the Rewarded Ads.
        /// <para>After calling this method, if you want to resume showing ads, 
        /// you must call the <see cref="LoadAd"/> method to reload the ad. 
        /// Note that in automatic loading mode, ads will not be automatically reloaded after Dispose. 
        /// You need to explicitly call <see cref="LoadAd"/> to prepare the ad for future displays.</para>
        public void DestroyAd()
        {
            if (manager == null) return;
            manager.DisposeAd(AdType.Rewarded);
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
                manager.OnRewardedAdLoaded -= OnAdLoaded.Invoke;
                manager.OnRewardedAdFailedToLoad -= AdFailedToLoad;
                manager.OnRewardedAdFailedToShow -= AdFailedToShow;
                manager.OnRewardedAdClicked -= OnAdClicked.Invoke;
                manager.OnRewardedAdCompleted -= OnReward.Invoke;
                manager.OnRewardedAdClosed -= AdClosed;
                manager.OnRewardedAdImpression -= OnAdImpression.Invoke;
            }
        }
        #endregion

        #region Manager Events wrappers
        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            CASFactory.OnManagerStateChanged -= OnManagerReady;
            this.manager = manager;
            manager.OnRewardedAdLoaded += OnAdLoaded.Invoke;
            manager.OnRewardedAdFailedToLoad += AdFailedToLoad;
            manager.OnRewardedAdFailedToShow += AdFailedToShow;
            manager.OnRewardedAdClicked += OnAdClicked.Invoke;
            manager.OnRewardedAdCompleted += OnReward.Invoke;
            manager.OnRewardedAdClosed += AdClosed;
            manager.OnRewardedAdImpression += OnAdImpression.Invoke;

            try
            {
                if (manager.IsReadyAd(AdType.Rewarded))
                {
                    OnAdLoaded.Invoke();
                }
                else if (loadAdOnAwake == 1)
                {
                    loadAdOnAwake = 0;
                    manager.LoadAd(AdType.Rewarded);
                }
                if (loadAdOnAwake > 1)
                {
                    loadAdOnAwake = 0;
                    manager.SetPreloadedBufferSize(AdType.Rewarded, loadAdOnAwake);
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

        private void AdClosed()
        {
            if (restartInterstitialInterval)
                CAS.MobileAds.settings.RestartInterstitialInterval();
            OnAdClosed.Invoke();
        }
        #endregion
    }
}