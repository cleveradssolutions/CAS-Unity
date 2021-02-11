using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu( "CleverAdsSolutions/Banner Ad Object" )]
    [DisallowMultipleComponent]
    public sealed class BannerAdObject : MonoBehaviour
    {
        public static BannerAdObject Instance { get; private set; }

        public ManagerIndex managerId;
        [SerializeField]
        private AdPosition adPosition = AdPosition.BottomCenter;
        [SerializeField]
        private AdSize adSize = AdSize.Banner;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdHidden;

        private bool active = false;
        private IMediationManager manager;

        public void SetAdPosition( AdPosition position )
        {
            adPosition = position;
            if (active && manager != null)
                manager.bannerPosition = position;
        }

        public void SetAdSize( AdSize size )
        {
            adSize = size;
            if (active && manager != null)
                manager.bannerSize = size;
        }

        public float GetHeightInPixels()
        {
            if (manager == null)
                return 0.0f;
            return manager.GetBannerHeightInPixels();
        }

        public float GetWidthInPixels()
        {
            if (manager == null)
                return 0.0f;
            return manager.GetBannerWidthInPixels();
        }

        #region MonoBehaviour
        private void Awake()
        {
            if (Instance)
            {
                Instance.active = false;
                Destroy( Instance );
            }
            active = true;
            Instance = this;
        }

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
            if (active)
            {
                this.manager = manager;
                manager.bannerSize = adSize;
                manager.bannerPosition = adPosition;
                manager.OnLoadedAd += OnBannerLoaded;
                manager.OnFailedToLoadAd += OnBannerLoadFailed;
                manager.OnBannerAdClicked += OnAdClicked.Invoke;
                manager.OnBannerAdShown += OnAdShown.Invoke;
                manager.OnBannerAdHidden += OnAdHidden.Invoke;

                try
                {
                    if (manager.IsReadyAd( AdType.Banner ))
                        OnAdLoaded.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
                if (isActiveAndEnabled)
                    manager.ShowAd( AdType.Banner );
            }
        }

        private void OnEnable()
        {
            if (active && manager != null)
                manager.ShowAd( AdType.Banner );
        }

        private void OnDisable()
        {
            if (active && manager != null)
                manager.HideBanner();
        }

        private void OnDestroy()
        {
            active = false;
            if (Instance == this)
                Instance = null;
            if (manager != null)
            {
                manager.OnLoadedAd -= OnBannerLoaded;
                manager.OnFailedToLoadAd -= OnBannerLoadFailed;
                manager.OnBannerAdClicked -= OnAdClicked.Invoke;
                manager.OnBannerAdShown -= OnAdShown.Invoke;
                manager.OnBannerAdHidden -= OnAdHidden.Invoke;
            }
            CASFactory.UnsubscribeReadyManagerAsync( OnManagerReady, managerId.index );
        }

        #endregion

        #region Manager Events wrappers
        private void OnBannerLoaded( AdType type )
        {
            if (type == AdType.Banner)
                OnAdLoaded.Invoke();
        }

        private void OnBannerLoadFailed( AdType type, string error )
        {
            if (type == AdType.Banner)
                OnAdFailedToLoad.Invoke( error );
        }
        #endregion
    }
}