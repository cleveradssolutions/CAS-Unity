//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/AppOpen Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Interstitial-Ad-object")]
    public sealed class AppOpenAdObject : MonoBehaviour, IInternalAdObject
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
            get { return manager != null && manager.IsReadyAd(AdType.AppOpen); }
        }

        /// <summary>
        /// Manual load Ad.
        /// <para>Please call load before each show ad.</para>
        /// <para>You can get a callback for the successful loading of an ad by subscribe <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
                loadAdOnAwake = true;
            else
                manager.LoadAd(AdType.AppOpen);
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
            manager.ShowAd(AdType.AppOpen);
        }

        #region MonoBehaviour
        private void Start()
        {
            if (!CASFactory.TryGetManagerByIndexAsync(this, managerId.index, true))
                OnAdFailedToLoad.Invoke(AdError.ManagerIsDisabled.GetMessage());
        }

        void IInternalAdObject.OnManagerReady(InitialConfiguration config)
        {
            this.manager = config.manager;
            manager.OnAppOpenAdLoaded += OnAdLoaded.Invoke;
            manager.OnAppOpenAdFailedToLoad += AdFailedToLoad;
            manager.OnAppOpenAdFailedToShow += AdFailedToShow;
            manager.OnAppOpenAdClicked += OnAdClicked.Invoke;
            manager.OnAppOpenAdClosed += OnAdClosed.Invoke;
            manager.OnAppOpenAdImpression += OnAdImpression.Invoke;

            try
            {
                if (manager.IsReadyAd(AdType.AppOpen))
                {
                    OnAdLoaded.Invoke();
                }
                else if (loadAdOnAwake)
                {
                    loadAdOnAwake = false;
                    manager.LoadAd(AdType.AppOpen);
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
                manager.OnAppOpenAdLoaded -= OnAdLoaded.Invoke;
                manager.OnAppOpenAdFailedToLoad -= AdFailedToLoad;
                manager.OnAppOpenAdFailedToShow -= AdFailedToShow;
                manager.OnAppOpenAdClicked -= OnAdClicked.Invoke;
                manager.OnAppOpenAdClosed -= OnAdClosed.Invoke;
                manager.OnAppOpenAdImpression -= OnAdImpression.Invoke;
            }
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);
        }
        #endregion

        #region Manager Events wrappers
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