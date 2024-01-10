//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Interstitial Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Interstitial-Ad-object")]
    public sealed class InterstitialAdObject : MonoBehaviour, IInternalAdObject
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
        /// Manual load Interstitial Ad.
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
        /// Presetn ad to user
        /// </summary>
        public void Present()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke(AdError.ManagerIsDisabled.GetMessage());
                return;
            }
            manager.ShowAd(AdType.Interstitial);
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
            manager.OnInterstitialAdLoaded += OnAdLoaded.Invoke;
            manager.OnInterstitialAdFailedToLoad += OnInterstitialAdFailedToLoad;
            manager.OnInterstitialAdFailedToShow += OnInterstitialAdFailedToShow;
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

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnInterstitialAdLoaded -= OnAdLoaded.Invoke;
                manager.OnInterstitialAdFailedToLoad -= OnInterstitialAdFailedToLoad;
                manager.OnInterstitialAdFailedToShow -= OnInterstitialAdFailedToShow;
                manager.OnInterstitialAdClicked -= OnAdClicked.Invoke;
                manager.OnInterstitialAdClosed -= OnAdClosed.Invoke;
                manager.OnInterstitialAdImpression -= OnAdImpression.Invoke;
            }
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);
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