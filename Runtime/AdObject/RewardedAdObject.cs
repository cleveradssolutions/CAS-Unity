//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Rewarded Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Rewarded-Ad-object")]
    public sealed class RewardedAdObject : MonoBehaviour, IInternalAdObject
    {
        public ManagerIndex managerId;

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
        private bool loadAdOnAwake = false;

        /// <summary>
        /// Check ready ad to present.
        /// </summary>
        public bool isAdReady
        {
            get { return manager != null && manager.IsReadyAd(AdType.Rewarded); }
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
                manager.LoadAd(AdType.Rewarded);
        }

        /// <summary>
        /// Presetn ad to user
        /// </summary>
        public void Present()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke(AdError.ManagerIsDisabled.GetMessage());
                return;
            }
            OnAdShown.Invoke();
            manager.ShowAd(AdType.Rewarded);
        }

        #region MonoBehaviour
        private void Start()
        {
            MobileAds.settings.isExecuteEventsOnUnityThread = true;
            if (!CASFactory.TryGetManagerByIndexAsync(this, managerId.index))
                OnAdFailedToLoad.Invoke(AdError.ManagerIsDisabled.GetMessage());
        }

        void IInternalAdObject.OnManagerReady(InitialConfiguration config)
        {
            this.manager = config.manager;
            manager.OnRewardedAdLoaded += OnAdLoaded.Invoke;
            manager.OnRewardedAdFailedToLoad += OnRewardedAdFailedToLoad;
            manager.OnRewardedAdFailedToShow += OnRewardedAdFailedToShow;
            manager.OnRewardedAdClicked += OnAdClicked.Invoke;
            manager.OnRewardedAdCompleted += OnReward.Invoke;
            manager.OnRewardedAdClosed += OnRewardedAdClosed;
            manager.OnRewardedAdImpression += OnAdImpression.Invoke;

            try
            {
                if (manager.IsReadyAd(AdType.Rewarded))
                {
                    OnAdLoaded.Invoke();
                }
                else if (loadAdOnAwake)
                {
                    loadAdOnAwake = false;
                    manager.LoadAd(AdType.Rewarded);
                }
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
                manager.OnRewardedAdLoaded -= OnAdLoaded.Invoke;
                manager.OnRewardedAdFailedToLoad -= OnRewardedAdFailedToLoad;
                manager.OnRewardedAdFailedToShow -= OnRewardedAdFailedToShow;
                manager.OnRewardedAdClicked -= OnAdClicked.Invoke;
                manager.OnRewardedAdCompleted -= OnReward.Invoke;
                manager.OnRewardedAdClosed -= OnRewardedAdClosed;
                manager.OnRewardedAdImpression -= OnAdImpression.Invoke;
            }
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);
        }
        #endregion

        #region Manager Events wrappers
        private void OnRewardedAdFailedToLoad(AdError error)
        {
            OnAdFailedToLoad.Invoke(error.GetMessage());
        }

        private void OnRewardedAdFailedToShow(string error)
        {
            OnAdFailedToShow.Invoke(error);
            OnAdClosed.Invoke();
        }

        private void OnRewardedAdClosed()
        {
            if (restartInterstitialInterval)
                CAS.MobileAds.settings.RestartInterstitialInterval();
            OnAdClosed.Invoke();
        }
        #endregion
    }
}