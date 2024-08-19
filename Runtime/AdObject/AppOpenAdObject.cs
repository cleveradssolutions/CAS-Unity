//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/AppOpen Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Open-Ad-object")]
    public sealed class AppOpenAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        [SerializeField]
        private bool autoshow = false;

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
        /// Manual load Ad.
        /// <para>Please call load method before each show ad.</para>
        /// <para>You can get a callback for the successful loading of an ad by subscribe <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void Load()
        {
            if (manager == null)
                loadAdOnAwake = true;
            else
                manager.LoadAd(AdType.AppOpen);
        }

        /// <summary>
        /// Check ready ad to present.
        /// </summary>
        public bool IsLoaded()
        {
            return manager != null && manager.IsReadyAd(AdType.AppOpen);
        }

        /// <summary>
        /// Present ad to user
        /// </summary>
        public void Show()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke(new AdError(AdError.NotInitialized, null).ToString());
                return;
            }
            OnAdShown.Invoke();
            manager.ShowAd(AdType.AppOpen);
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
                manager.OnAppOpenAdLoaded -= OnAdLoaded.Invoke;
                manager.OnAppOpenAdFailedToLoad -= AdFailedToLoad;
                manager.OnAppOpenAdFailedToShow -= AdFailedToShow;
                manager.OnAppOpenAdClicked -= OnAdClicked.Invoke;
                manager.OnAppOpenAdClosed -= OnAdClosed.Invoke;
                manager.OnAppOpenAdImpression -= OnAdImpression.Invoke;
                if (autoshow)
                {
                    MobileAds.OnApplicationForeground -= AppForeground;
                }
            }

        }
        #endregion

        #region Manager Events wrappers
        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            CASFactory.OnManagerStateChanged -= OnManagerReady;

            this.manager = manager;
            manager.OnAppOpenAdLoaded += OnAdLoaded.Invoke;
            manager.OnAppOpenAdFailedToLoad += AdFailedToLoad;
            manager.OnAppOpenAdFailedToShow += AdFailedToShow;
            manager.OnAppOpenAdClicked += OnAdClicked.Invoke;
            manager.OnAppOpenAdClosed += OnAdClosed.Invoke;
            manager.OnAppOpenAdImpression += OnAdImpression.Invoke;

            if (autoshow)
            {
                MobileAds.OnApplicationForeground += AppForeground;
            }

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
                else
                {
                    OnAdFailedToLoad.Invoke(new AdError(AdError.NotReady, null).ToString());
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

        private void AppForeground()
        {
            if (IsLoaded())
            {
                CASFactory.UnityLog("AppOpen ad are shown because the app has come to the foreground.");
                Show();
            }
        }
        #endregion
    }
}