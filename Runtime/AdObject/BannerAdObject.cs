//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that loads and shows a Banner ad without code,
    /// using <see cref="UnityEvent"/> callbacks configured in the Inspector.
    /// <para>The banner is visible while this component is enabled and active,
    /// and hidden while it is disabled.</para>
    /// <para>The component gets the <see cref="IAdView"/> of the selected size from the
    /// <see cref="IMediationManager"/> with <see cref="managerId"/> once the manager is initialized.
    /// Calls made before that are deferred until it is ready.</para>
    /// <para>The AdView belongs to the manager, not to this component.
    /// Destroying the component only hides the banner and does not destroy the loaded ad,
    /// so different BannerAdObjects with the same <see cref="AdSize"/> share the same AdView.
    /// Call <see cref="DestroyAd"/> to release it.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Banner Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Banner-Ad-object")]
    public sealed class BannerAdObject : MonoBehaviour
    {
        /// <summary>
        /// The most recently awakened Banner Ad object.
        /// <para>It is <c>null</c> when there is no such object, or after it is destroyed.</para>
        /// </summary>
        public static BannerAdObject Instance { get; private set; }

        /// <summary>
        /// The index of the <see cref="IMediationManager"/> that provides ads for this component.
        /// </summary>
        public ManagerIndex managerId;

        [Tooltip("The position of the banner on the screen.")]
        [SerializeField]
        private AdPosition adPosition = AdPosition.BottomCenter;
        [Tooltip("For greater control over where an AdView is placed on screen. Use Density-independent Pixels (DP).")]
        [SerializeField]
        private Vector2Int adOffset = Vector2Int.zero;
        [Tooltip("The size of the banner. Changing the size at runtime loads a new ad.")]
        [SerializeField]
        private AdSize adSize = AdSize.Banner;

        /// <summary>
        /// An optional placement name for the ad instance that helps categorize
        /// and track statistics across different ad placements.
        /// The placement name is applied only before ad load.
        /// Maximum 100 characters allowed for the placement name.
        /// </summary>
        [Tooltip("An optional placement name for the ad instance that helps categorize and track statistics across different ad placements.")]
        public string placement = null;

        /// <summary>
        /// Invoked when an ad is loaded.
        /// <para>Also invoked right after initialization or a size change if the AdView already has a loaded ad.</para>
        /// </summary>
        public UnityEvent OnAdLoaded;
        /// <summary>
        /// Invoked with the error message when an ad fails to load,
        /// or when the manager could not be found on <c>Start</c>.
        /// </summary>
        public CASUEventWithError OnAdFailedToLoad;
        /// <summary>
        /// Invoked when the banner becomes visible:
        /// when an ad is loaded while the component is enabled,
        /// or when the component is enabled with a loaded ad.
        /// </summary>
        public UnityEvent OnAdShown;
        /// <summary>
        /// Invoked when the user clicks the ad.
        /// </summary>
        public UnityEvent OnAdClicked;
        /// <summary>
        /// Invoked when the banner is hidden:
        /// when the component is disabled, or when an ad fails to load while the component is enabled.
        /// </summary>
        public UnityEvent OnAdHidden;

        /// <summary>
        /// Invoked when the ad impression is recorded, with the ad metadata for revenue tracking.
        /// </summary>
        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private IAdView adView;
        private bool loadAdOnAwake = false;

        /// <summary>
        /// Sets the position of the banner on the screen and resets the offset set by
        /// <see cref="SetAdPosition(int, int, AdPosition)"/>.
        /// </summary>
        /// <param name="position">The position on the screen.</param>
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

        /// <summary>
        /// Changes the size of the banner ad.
        /// <para>The component switches to the AdView of the new size from the manager.
        /// The previous AdView is hidden but not destroyed.</para>
        /// </summary>
        /// <param name="size">The new banner size.</param>
        public void SetAdSize(AdSize size)
        {
            adSize = size;
            RefreshLinkWithAdView();
        }

        /// <summary>
        /// Loads an ad, or replaces the currently loaded ad with a new one.
        /// <para>If the manager is not initialized yet, loading starts once it is ready.</para>
        /// <para>Subscribe to <see cref="OnAdLoaded"/> and <see cref="OnAdFailedToLoad"/> to get the result.</para>
        /// </summary>
        public void LoadAd()
        {
            if (adView == null)
                loadAdOnAwake = true;
            else
                adView.Load();
        }

        /// <summary>
        /// The actual position and size of the AdView on the screen, in pixels.
        /// <para>Returns <see cref="Rect.zero"/> when the AdView is not active.</para>
        /// <para>The position includes the insets for display cutouts.</para>
        /// </summary>
        public Rect rectInPixels {
            get{ return adView != null ? adView.rectInPixels : Rect.zero; }
        }

        /// <summary>
        /// Alternative to <see cref="SetAdPosition(AdPosition)"/> for calling from the Inspector (e.g. UnityEvent),
        /// since enum parameters are not supported there.
        /// </summary>
        /// <param name="enumIndex">Index of the <see cref="AdPosition"/> enum value.</param>
        public void SetAdPositionEnumIndex(int enumIndex)
        {
            SetAdPosition((AdPosition)enumIndex);
        }

        /// <summary>
        /// Alternative to <see cref="SetAdSize(AdSize)"/> for calling from the Inspector (e.g. UnityEvent),
        /// since enum parameters are not supported there.
        /// </summary>
        /// <param name="enumIndex">Index of the <see cref="AdSize"/> enum value.</param>
        public void SetAdSizeEnumIndex(int enumIndex)
        {
            SetAdSize((AdSize)enumIndex);
        }

        /// <summary>
        /// Destroys the AdView with the loaded Banner ad.
        /// <para>The loaded ad is not destroyed together with this MonoBehaviour.
        /// Different BannerAdObjects with the same <see cref="AdSize"/> use the same AdView.
        /// Call this method if you want to release the resources of the loaded ad.</para>
        /// <para>To show ads again, call <see cref="LoadAd"/>.
        /// Ads are not reloaded automatically after this call, even in automatic loading mode.</para>
        /// </summary>
        public void DestroyAd()
        {
            if (adView == null) return;
            adView.Dispose();
        }

        #region MonoBehaviour
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (!CASFactory.TryGetManagerByIndexAsync(managerId.index, OnManagerReady))
                OnAdFailedToLoad.Invoke(new AdError(AdError.NotInitialized, null).ToString());
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
            adView.placement = placement;

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