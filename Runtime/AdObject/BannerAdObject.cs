//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu( "CleverAdsSolutions/Banner Ad Object" )]
    [DisallowMultipleComponent]
    [HelpURL( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Banner-Ad-object" )]
    public sealed class BannerAdObject : MonoBehaviour
    {
        /// <summary>
        /// Last active Banner Ad object. May be is null!
        /// </summary>
        public static BannerAdObject Instance { get; private set; }

        public ManagerIndex managerId;

        [SerializeField]
        private AdPosition adPosition = AdPosition.BottomCenter;
        [Tooltip( "For greater control over where a AdView is placed on screen, make sure to select AdPosition.TopLeft.\n" +
            "Use Density-independent Pixels (DP)." )]
        [SerializeField]
        private Vector2Int adOffset = Vector2Int.zero;
        [SerializeField]
        private AdSize adSize = AdSize.Banner;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdHidden;

        private IMediationManager manager;
        private IAdView adView;
        private bool waitOfLoad = false;

        /// <summary>
        /// <see cref="IAdView.position"/>
        /// </summary>
        public void SetAdPosition( AdPosition position )
        {
            adOffset = Vector2Int.zero;
            adPosition = position;
            if (adView != null)
                adView.position = position;
        }

        /// <summary>
        /// <see cref="IAdView.SetPosition(int, int)"/>
        /// </summary>
        /// <param name="x">X-coordinate on screen in DP.</param>
        /// <param name="y">Y-coordinate on screen in DP.</param>
        public void SetAdPosition( int x, int y )
        {
            adOffset = new Vector2Int( x, y );
            adPosition = AdPosition.TopLeft;
            if (adView != null)
                adView.SetPosition( x, y );
        }

        public void SetAdSize( AdSize size )
        {
            adSize = size;
            RefreshLinktWithAdView();
        }

        /// <summary>
        /// Manual load the Ad or reload current loaded Ad to skip impression.
        /// <para>You can get a callback for the successful loading of an ad by subscribe to <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void LoadAd()
        {
            if (adView == null)
                waitOfLoad = true;
            else
                adView.Load();
        }

        /// <summary>
        /// <see cref="IAdView.rectInPixels"/>
        /// </summary>
        public Rect rectInPixels
        {
            get
            {
                if (adView == null)
                    return Rect.zero;
                return adView.rectInPixels;
            }
        }

        public void SetAdPositionEnumIndex( int enumIndex )
        {
            SetAdPosition( ( AdPosition )enumIndex );
        }

        public void SetAdSizeEnumIndex( int enumIndex )
        {
            SetAdSize( ( AdSize )enumIndex );
        }

        #region MonoBehaviour
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            MobileAds.settings.isExecuteEventsOnUnityThread = true;
            if (!CASFactory.TryGetManagerByIndexAsync( OnManagerReady, managerId.index ))
                OnAdFailedToLoad.Invoke( "Manager not initialized yet" );
        }

        private void OnEnable()
        {
            if (adView != null)
            {
                if (adPosition == AdPosition.TopLeft)
                    adView.SetPosition( adOffset.x, adOffset.y );
                else
                    adView.position = adPosition;
                adView.SetActive( true );
            }
        }

        private void OnDisable()
        {
            if (adView != null)
                adView.SetActive( false );
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            DetachAdView();
            CASFactory.UnsubscribeReadyManagerAsync( OnManagerReady, managerId.index );
        }

        #endregion

        #region Manager Events wrappers
        private void OnManagerReady( IMediationManager manager )
        {
            if (!this) // When object are destroyed
                return;

            this.manager = manager;
            RefreshLinktWithAdView();
        }

        private void DetachAdView()
        {
            if (adView != null)
            {
                adView.OnLoaded -= OnBannerLoaded;
                adView.OnFailed -= OnBannerLoadFailed;
                adView.OnClicked -= OnBannerClicked;
                adView.OnPresented -= OnBannerPresenting;
                adView.OnHidden -= OnBannerHidden;
                adView.SetActive( false );
                adView = null;
            }
        }

        private void RefreshLinktWithAdView()
        {
            if (manager == null)
                return;
            var newView = manager.GetAdView( adSize );
            if (newView == adView)
                return;
            DetachAdView();

            newView.OnLoaded += OnBannerLoaded;
            newView.OnFailed += OnBannerLoadFailed;
            newView.OnClicked += OnBannerClicked;
            newView.OnPresented += OnBannerPresenting;
            newView.OnHidden += OnBannerHidden;

            adView = newView;

            try
            {
                if (adView.isReady)
                    OnAdLoaded.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }

            if (isActiveAndEnabled)
            {
                if (adPosition == AdPosition.TopLeft)
                    adView.SetPosition( adOffset.x, adOffset.y );
                else
                    adView.position = adPosition;
                adView.SetActive( true );
            }

            if (waitOfLoad)
            {
                waitOfLoad = false;
                adView.Load();
            }
        }

        private void OnBannerLoaded( IAdView ad )
        {
            if (ad == adView)
                OnAdLoaded.Invoke();
        }

        private void OnBannerLoadFailed( IAdView ad, AdError error )
        {
            if (ad != adView)
                return;
            OnAdFailedToLoad.Invoke( error.GetMessage() );
        }

        private void OnBannerPresenting( IAdView view, AdMetaData data )
        {
            if (view == adView)
                OnAdShown.Invoke();
        }

        private void OnBannerClicked( IAdView view )
        {
            if (view == adView)
                OnAdClicked.Invoke();
        }

        private void OnBannerHidden( IAdView view )
        {
            if (view == adView)
                OnAdHidden.Invoke();
        }
        #endregion


        [Obsolete( "Migrated to new ad size api: rectInPixels.height" )]
        public float GetHeightInPixels()
        {
            return rectInPixels.height;
        }

        [Obsolete( "Migrated to new ad size api: rectInPixels.width" )]
        public float GetWidthInPixels()
        {
            return rectInPixels.width;
        }
    }
}