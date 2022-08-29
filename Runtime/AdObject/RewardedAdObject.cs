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
    [AddComponentMenu( "CleverAdsSolutions/Rewarded Ad Object" )]
    [DisallowMultipleComponent]
    [HelpURL( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Rewarded-Ad-object" )]
    public sealed class RewardedAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        public bool restartInterstitialInterval = false;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        public UnityEvent OnReward;

        private IMediationManager manager;

        public void Present()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke( "Manager not initialized yet" );
                return;
            }
            manager.ShowAd( AdType.Rewarded );
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
            if (!this) // When object are destroyed
                return;

            this.manager = manager;
            manager.OnLoadedAd += OnRewardedAdLoaded;
            manager.OnFailedToLoadAd += OnRewardedAdFailedToLoad;
            manager.OnRewardedAdFailedToShow += OnAdFailedToShow.Invoke;
            manager.OnRewardedAdShown += OnAdShown.Invoke;
            manager.OnRewardedAdClicked += OnAdClicked.Invoke;
            manager.OnRewardedAdCompleted += OnReward.Invoke;
            manager.OnRewardedAdClosed += OnRewardedAdClosed;

            try
            {
                if (manager.IsReadyAd( AdType.Rewarded ))
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
                manager.OnLoadedAd -= OnRewardedAdLoaded;
                manager.OnFailedToLoadAd -= OnRewardedAdFailedToLoad;
                manager.OnRewardedAdFailedToShow -= OnAdFailedToShow.Invoke;
                manager.OnRewardedAdShown -= OnAdShown.Invoke;
                manager.OnRewardedAdClicked -= OnAdClicked.Invoke;
                manager.OnRewardedAdCompleted -= OnReward.Invoke;
                manager.OnRewardedAdClosed -= OnRewardedAdClosed;
            }
            CASFactory.UnsubscribeReadyManagerAsync( OnManagerReady, managerId.index );
        }
        #endregion

        #region Manager Events wrappers
        private void OnRewardedAdFailedToLoad( AdType adType, string error )
        {
            if (adType == AdType.Rewarded)
                OnAdFailedToLoad.Invoke( error );
        }

        private void OnRewardedAdLoaded( AdType adType )
        {
            if (adType == AdType.Rewarded)
                OnAdLoaded.Invoke();
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