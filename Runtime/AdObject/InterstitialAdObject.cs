using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu( "CleverAdsSolutions/Interstitial Ad Object" )]
    [DisallowMultipleComponent]
    [HelpURL( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Interstitial-Ad-object" )]
    public sealed class InterstitialAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdClosed;

        private IMediationManager manager;

        public void Present()
        {
            if (manager == null)
            {
                OnAdFailedToShow.Invoke( "Manager not initialized yet" );
                return;
            }
            manager.ShowAd( AdType.Interstitial );
        }

        #region MonoBehaviour
        private void Start()
        {
            try
            {
                OnAdFailedToLoad.Invoke( "Manager not initialized yet" );
            }
            finally
            {
                MobileAds.settings.isExecuteEventsOnUnityThread = true;
                CASFactory.GetReadyManagerByIndexAsync( OnManagerReady, managerId.index );
            }
        }

        private void OnManagerReady( IMediationManager manager )
        {
            this.manager = manager;
            manager.OnLoadedAd += OnInterstitialAdLoaded;
            manager.OnFailedToLoadAd += OnInterstitialAdFailedToLoad;
            manager.OnInterstitialAdFailedToShow += OnAdFailedToShow.Invoke;
            manager.OnInterstitialAdShown += OnAdShown.Invoke;
            manager.OnInterstitialAdClicked += OnAdClicked.Invoke;
            manager.OnInterstitialAdClosed += OnAdClosed.Invoke;

            try
            {
                if (manager.IsReadyAd( AdType.Interstitial ))
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
                manager.OnLoadedAd -= OnInterstitialAdLoaded;
                manager.OnFailedToLoadAd -= OnInterstitialAdFailedToLoad;
                manager.OnInterstitialAdFailedToShow -= OnAdFailedToShow.Invoke;
                manager.OnInterstitialAdShown -= OnAdShown.Invoke;
                manager.OnInterstitialAdClicked -= OnAdClicked.Invoke;
                manager.OnInterstitialAdClosed -= OnAdClosed.Invoke;
            }
            CASFactory.UnsubscribeReadyManagerAsync( OnManagerReady, managerId.index );
        }
        #endregion

        #region Manager Events wrappers
        private void OnInterstitialAdFailedToLoad( AdType adType, string error )
        {
            if (adType == AdType.Interstitial)
                OnAdFailedToLoad.Invoke( error );
        }

        private void OnInterstitialAdLoaded( AdType adType )
        {
            if (adType == AdType.Interstitial)
                OnAdLoaded.Invoke();
        }
        #endregion
    }
}