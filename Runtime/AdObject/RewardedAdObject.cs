//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that loads and shows Rewarded ads without code,
    /// using <see cref="UnityEvent"/> callbacks configured in the Inspector.
    /// <para>The component subscribes to the <see cref="IMediationManager"/> with <see cref="managerId"/>
    /// once the manager is initialized. Calls made before that are deferred until it is ready.</para>
    /// <para>The loaded ad belongs to the manager, not to this component.
    /// Destroying the component does not destroy the loaded ad,
    /// so different AdObjects can share the same loaded ad.
    /// Call <see cref="DestroyAd"/> to release it.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Rewarded Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Rewarded-Ad-object")]
    public sealed class RewardedAdObject : MonoBehaviour
    {
        /// <summary>
        /// The index of the <see cref="IMediationManager"/> that provides ads for this component.
        /// </summary>
        public ManagerIndex managerId;

        [Tooltip("An optional placement name for the ad instance that helps categorize and track statistics across different ad placements.")]
        [SerializeField]
        private string placement;

        /// <summary>
        /// Restart the Interstitial ads interval after the Rewarded ad is closed,
        /// so an Interstitial ad is not shown right after a Rewarded ad.
        /// </summary>
        public bool restartInterstitialInterval = false;

        /// <summary>
        /// Invoked when an ad is loaded and ready to show.
        /// <para>Also invoked right after initialization if the manager already has a loaded ad.</para>
        /// </summary>
        public UnityEvent OnAdLoaded;
        /// <summary>
        /// Invoked with the error message when an ad fails to load,
        /// or when the manager could not be found on <c>Start</c>.
        /// </summary>
        public CASUEventWithError OnAdFailedToLoad;
        /// <summary>
        /// Invoked with the error message when an ad fails to show.
        /// <see cref="OnAdClosed"/> is invoked right after it.
        /// </summary>
        public CASUEventWithError OnAdFailedToShow;
        /// <summary>
        /// Invoked when <see cref="Present"/> is called on an initialized manager, before the ad is presented.
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

        /// <summary>
        /// Invoked when the user has earned the reward by watching the ad.
        /// <para>Grant the reward here, not in <see cref="OnAdClosed"/>:
        /// the ad can be closed without earning the reward.</para>
        /// </summary>
        public UnityEvent OnReward;

        private IMediationManager manager;
        private int loadAdOnAwake = 0;

        /// <summary>
        /// Sets the number of Rewarded ads that the SDK keeps loaded at all times.
        /// <para>Loading starts immediately. After an ad is shown, a new ad is loaded automatically to keep the buffer full.</para>
        /// <para>The <see cref="OnAdLoaded"/> is invoked once for each ad loaded into the buffer.</para>
        /// <para>The <paramref name="size"/> is clamped to the range from 1 to 4.</para>
        /// <para>While the buffer is active, <see cref="LoadingManagerMode.Manual"/> is ignored and ads are reloaded automatically.</para>
        /// <para>To stop buffering, call <see cref="LoadAd"/> to load only a single ad,
        /// or call <see cref="DestroyAd"/> to stop loading altogether.</para>
        /// </summary>
        /// <param name="size">The number of ads to keep loaded, from 1 to 4.</param>
        public void SetPreloadedAdBufferSize(int size)
        {
            if (manager == null)
                loadAdOnAwake = size;
            else
                manager.SetPreloadedAdBufferSize(AdType.Rewarded, size);
        }

        /// <summary>
        /// Loads a single Rewarded ad.
        /// <para>Call this before each <see cref="Present"/> when the loading mode is <see cref="LoadingManagerMode.Manual"/>.</para>
        /// <para>If the manager is not initialized yet, loading starts once it is ready.</para>
        /// <para>Subscribe to <see cref="OnAdLoaded"/> and <see cref="OnAdFailedToLoad"/> to get the result.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
                loadAdOnAwake = 1;
            else
                manager.LoadAd(AdType.Rewarded);
        }

        /// <summary>
        /// Checks whether an ad is loaded and ready to show.
        /// <para>It is not required to check this before calling <see cref="Present"/>:
        /// if the ad cannot be shown, <see cref="OnAdFailedToShow"/> is invoked.</para>
        /// </summary>
        /// <returns><c>true</c> if an ad is ready; <c>false</c> if not, or if the manager is not initialized yet.</returns>
        public bool IsAdLoaded()
        {
            return manager != null && manager.IsReadyAd(AdType.Rewarded);
        }

        /// <summary>
        /// Shows the loaded Rewarded ad to the user.
        /// <para>If the manager is not initialized yet, <see cref="OnAdFailedToShow"/> is invoked.
        /// Otherwise <see cref="OnAdShown"/> is invoked and the ad is presented
        /// with the placement name set in the Inspector.</para>
        /// <para>The reward is reported by <see cref="OnReward"/>.</para>
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
        /// <para>The call is ignored if the manager is not initialized yet.</para>
        /// </summary>
        /// <param name="data">The custom data, up to 8192 characters.</param>
        public void SetServerSideVerificationData(string data)
        {
            if (manager != null)
            {
                manager.SetServerSideVerificationData(data);
            }
        }

        /// <summary>
        /// Destroys the loaded Rewarded ad and stops loading new ones.
        /// <para>The loaded ad is not destroyed together with this MonoBehaviour.
        /// Different AdObjects can use the same loaded ad.
        /// Call this method if you want to release the resources of the loaded ad.</para>
        /// <para>To show ads again, call <see cref="LoadAd"/>.
        /// Ads are not reloaded automatically after this call, even in automatic loading mode.</para>
        /// </summary>
        public void DestroyAd()
        {
            if (manager == null) return;
            manager.DisposeAd(AdType.Rewarded);
        }

        /// <summary>
        /// Same as <see cref="IsAdLoaded"/>. Kept for backward compatibility.
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
                    manager.SetPreloadedAdBufferSize(AdType.Rewarded, loadAdOnAwake);
                    loadAdOnAwake = 0;
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