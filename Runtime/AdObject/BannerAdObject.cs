//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Banner Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Banner-Ad-object")]
    public sealed class BannerAdObject : MonoBehaviour, IInternalAdObject
    {
        /// <summary>
        /// Last active Banner Ad object. May be is null!
        /// </summary>
        public static BannerAdObject Instance { get; private set; }

        public ManagerIndex managerId;

        [SerializeField]
        private AdPosition adPosition = AdPosition.BottomCenter;
        [Tooltip("For greater control over where a AdView is placed on screen, make sure to select AdPosition.TopLeft.\n" +
            "Use Density-independent Pixels (DP).")]
        [SerializeField]
        private Vector2Int adOffset = Vector2Int.zero;
        [SerializeField]
        private AdSize adSize = AdSize.Banner;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdHidden;

        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private IAdView adView;
        private bool loadAdOnAwake = false;

        /// <summary>
        /// The position where the AdView ad should be placed.
        /// The <see cref="AdPosition"/> enum lists the valid ad position values.
        /// </summary>
        public void SetAdPosition(AdPosition position)
        {
            adOffset = Vector2Int.zero;
            adPosition = position;
            if (adView != null)
                adView.position = position;
        }

        /// <summary>
        /// For greater control over where a AdView is placed on screen than what's offered by <see cref="AdPosition"/> values.
        /// <para>The top-left corner of the AdView will be positioned
        /// at the <paramref name="x"/> and <paramref name="y"/> values passed to the method,
        /// where the origin is the top-left of the screen.</para>
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// <para>Screen positioning coordinates are only available for the <see cref="AdPosition.TopLeft"/>.</para>
        /// </summary>
        /// <param name="x">X-coordinate on screen in DP.</param>
        /// <param name="y">Y-coordinate on screen in DP.</param>
        public void SetAdPosition(int x, int y)
        {
            adOffset = new Vector2Int(x, y);
            adPosition = AdPosition.TopLeft;
            if (adView != null)
                adView.SetPosition(x, y);
        }

        public void SetAdSize(AdSize size)
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
                loadAdOnAwake = true;
            else
                adView.Load();
        }

        /// <summary>
        /// Get the real AdView rect with position and size in pixels on screen.
        /// <para>Return <see cref="Rect.zero"/> when ad view is not active.</para>
        /// <para>The position on the screen is calculated with the addition of indents for the cutouts.</para>
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

        public void SetAdPositionEnumIndex(int enumIndex)
        {
            SetAdPosition((AdPosition)enumIndex);
        }

        public void SetAdSizeEnumIndex(int enumIndex)
        {
            SetAdSize((AdSize)enumIndex);
        }

        #region MonoBehaviour
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            MobileAds.settings.isExecuteEventsOnUnityThread = true;
            if (!CASFactory.TryGetManagerByIndexAsync(this, managerId.index))
                OnAdFailedToLoad.Invoke(AdError.ManagerIsDisabled.GetMessage());
        }

        private void OnEnable()
        {
            if (adView != null)
            {
                if (adPosition == AdPosition.TopLeft)
                    adView.SetPosition(adOffset.x, adOffset.y);
                else
                    adView.position = adPosition;
                adView.SetActive(true);
                if (adView.isReady)
                    OnAdShown.Invoke();
            }
        }

        private void OnDisable()
        {
            if (adView != null)
            {
                adView.SetActive(false);
                OnAdHidden.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            DetachAdView();
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);
        }

        #endregion

        #region Manager Events wrappers
        void IInternalAdObject.OnManagerReady(InitialConfiguration config)
        {
            this.manager = config.manager;
            RefreshLinktWithAdView();
        }

        private void DetachAdView()
        {
            if (adView == null)
                return;

            adView.OnLoaded -= OnBannerLoaded;
            adView.OnFailed -= OnBannerLoadFailed;
            adView.OnClicked -= OnBannerClicked;
            adView.OnImpression -= OnBannerImpression;
            adView.SetActive(false);
            adView = null;
        }

        private void RefreshLinktWithAdView()
        {
            if (manager == null)
                return;
            var newView = manager.GetAdView(adSize);
            if (newView == adView)
                return;
            DetachAdView();

            newView.OnLoaded += OnBannerLoaded;
            newView.OnFailed += OnBannerLoadFailed;
            newView.OnClicked += OnBannerClicked;
            newView.OnImpression += OnBannerImpression;

            adView = newView;

            try
            {
                if (adView.isReady)
                    OnAdLoaded.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (isActiveAndEnabled)
            {
                if (adPosition == AdPosition.TopLeft)
                    adView.SetPosition(adOffset.x, adOffset.y);
                else
                    adView.position = adPosition;
                adView.SetActive(true);
            }

            if (loadAdOnAwake)
            {
                loadAdOnAwake = false;
                adView.Load();
            }
        }

        private void OnBannerLoaded(IAdView ad)
        {
            if (ad == adView)
            {
                OnAdLoaded.Invoke();
                if (isActiveAndEnabled)
                    OnAdShown.Invoke();
            }
        }

        private void OnBannerLoadFailed(IAdView ad, AdError error)
        {
            if (ad != adView)
                return;
            OnAdFailedToLoad.Invoke(error.GetMessage());
            if (isActiveAndEnabled)
                OnAdHidden.Invoke();
        }

        private void OnBannerImpression(IAdView ad, AdMetaData impression)
        {
            OnAdImpression.Invoke(impression);
        }

        private void OnBannerClicked(IAdView view)
        {
            if (view == adView)
                OnAdClicked.Invoke();
        }
        #endregion
    }
}