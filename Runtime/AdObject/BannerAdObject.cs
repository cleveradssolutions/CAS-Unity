//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Banner Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Banner-Ad-object")]
    public sealed class BannerAdObject : MonoBehaviour
    {
        /// <summary>
        /// Last active Banner Ad object. May be is null!
        /// </summary>
        public static BannerAdObject Instance { get; private set; }

        public ManagerIndex managerId;

        [SerializeField]
        private AdPosition adPosition = AdPosition.BottomCenter;
        [Tooltip("For greater control over where a AdView is placed on screen. Use Density-independent Pixels (DP).")]
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
        /// The AdView will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// </summary>
        /// <param name="x">X-coordinate on screen in DP.</param>
        /// <param name="y">Y-coordinate on screen in DP.</param>
        /// <param name="position">The corner of the screen.</param>
        public void SetAdPosition(int x, int y, AdPosition position = AdPosition.TopLeft)
        {
            adOffset = new Vector2Int(x, y);
            adPosition = position;
            if (adView != null)
                adView.SetPosition(x, y, position);
        }

        public void SetAdSize(AdSize size)
        {
            adSize = size;
            RefreshLinkWithAdView();
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
            if (!CASFactory.TryGetManagerByIndexAsync(managerId.index, OnManagerReady))
                OnAdFailedToLoad.Invoke(AdError.ManagerIsDisabled.GetMessage());
        }

        private void OnEnable()
        {
            if (adView != null)
            {
                adView.SetPosition(adOffset.x, adOffset.y, adPosition);
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
            if (manager == null)
                CASFactory.OnManagerStateChanged -= OnManagerReady;
            DetachAdView();
        }

        #endregion

        #region Manager Events wrappers
        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            CASFactory.OnManagerStateChanged -= OnManagerReady;

            this.manager = manager;
            RefreshLinkWithAdView();
        }

        private void DetachAdView()
        {
            if (adView == null)
                return;

            adView.OnLoaded -= AdLoaded;
            adView.OnFailed -= AdLoadFailed;
            adView.OnClicked -= AdClicked;
            adView.OnImpression -= AdImpression;
            adView.SetActive(false);
            adView = null;
        }

        private void RefreshLinkWithAdView()
        {
            if (manager == null)
                return;
            var newView = manager.GetAdView(adSize);
            if (newView == adView)
                return;
            DetachAdView();

            newView.OnLoaded += AdLoaded;
            newView.OnFailed += AdLoadFailed;
            newView.OnClicked += AdClicked;
            newView.OnImpression += AdImpression;

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
                adView.SetPosition(adOffset.x, adOffset.y, adPosition);
                adView.SetActive(true);
            }

            if (loadAdOnAwake)
            {
                loadAdOnAwake = false;
                adView.Load();
            }
        }

        private void AdLoaded(IAdView ad)
        {
            if (ad == adView)
            {
                OnAdLoaded.Invoke();
                if (isActiveAndEnabled)
                    OnAdShown.Invoke();
            }
        }

        private void AdLoadFailed(IAdView ad, AdError error)
        {
            if (ad != adView)
                return;
            OnAdFailedToLoad.Invoke(error.GetMessage());
            if (isActiveAndEnabled)
                OnAdHidden.Invoke();
        }

        private void AdImpression(IAdView ad, AdMetaData impression)
        {
            OnAdImpression.Invoke(impression);
        }

        private void AdClicked(IAdView view)
        {
            if (view == adView)
                OnAdClicked.Invoke();
        }
        #endregion
    }
}