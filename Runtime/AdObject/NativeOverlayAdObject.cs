using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that loads a Native ad and shows it as an overlay template on top of the game,
    /// without code, using <see cref="UnityEvent"/> callbacks configured in the Inspector.
    /// <para>The overlay is visible while this component is enabled and active,
    /// and hidden while it is disabled.</para>
    /// <para>Unlike other AdObjects, the loaded ad belongs to this component:
    /// it is not shared with other AdObjects and is destroyed together with this MonoBehaviour.</para>
    /// <para>Ads are not loaded automatically. Call <see cref="LoadAd"/> to load an ad.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Native Overlay Ad Object")]
    [DisallowMultipleComponent]
    public sealed class NativeOverlayAdObject : MonoBehaviour
    {
        /// <summary>
        /// The index of the <see cref="IMediationManager"/> that provides ads for this component.
        /// </summary>
        public ManagerIndex managerId;

        [Tooltip("The position of the overlay on the screen.")]
        [SerializeField] private AdPosition overlayPosition = AdPosition.BottomCenter;
        [Tooltip("The offset from the selected position, in Density-independent Pixels (DP).")]
        [SerializeField] private Vector2Int positionOffset = Vector2Int.zero;
        [Tooltip("The size of the native ad template, in Density-independent Pixels (DP).")]
        [SerializeField] private Vector2Int templateSize = new Vector2Int(300, 250);

        [Tooltip("The colors and fonts of the native ad template.")]
        [SerializeField] private NativeTemplateStyle templateStyle = new NativeTemplateStyle();

        /// <summary>
        /// This determines where the AdChoices icon is displayed within the ad content.
        /// Use this property to configure the placement according to your app's design and ad requirements.
        /// The icon placement is applied only before ad load.
        /// </summary>
        public AdChoicesPlacement adChoicesPlacement = AdChoicesPlacement.TopRight;

        /// <summary>
        /// Sets the initial mute state of the video.
        /// By default the video will start with the audio muted.
        /// The mute state is applied only before ad load.
        /// </summary>
        public bool isStartVideoMuted = true;

        /// <summary>
        /// An optional placement name for the ad instance that helps categorize
        /// and track statistics across different ad placements.
        /// The placement name is applied only before ad load.
        /// Maximum 100 characters allowed for the placement name.
        /// </summary>
        [Tooltip("An optional placement name for the ad instance that helps categorize and track statistics across different ad placements.")]
        public string placement = null;

        [Tooltip("Load the ad automatically on comnponent start")]
        [SerializeField]
        private bool loadOnAwake = false;

        /// <summary>
        /// Invoked when an ad is loaded and the template is rendered.
        /// </summary>
        public UnityEvent OnAdLoaded;
        /// <summary>
        /// Invoked with the error message when an ad fails to load,
        /// or when the loaded ad fails to show.
        /// </summary>
        public CASUEventWithError OnAdFailedToLoad;
        /// <summary>
        /// Invoked when the overlay becomes visible:
        /// when an ad is loaded while the component is enabled,
        /// or when the component is enabled with a loaded ad.
        /// </summary>
        public UnityEvent OnAdShown;
        /// <summary>
        /// Invoked when the user clicks the ad.
        /// </summary>
        public UnityEvent OnAdClicked;
        /// <summary>
        /// Invoked when the overlay is hidden because the component is disabled.
        /// </summary>
        public UnityEvent OnAdHidden;
        /// <summary>
        /// Invoked when the ad impression is recorded, with the ad metadata for revenue tracking.
        /// </summary>
        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private INativeOverlayAd ad;
        private bool loading;

        /// <summary>
        /// The actual position and size of the overlay on the screen, in pixels.
        /// <para>Returns <see cref="Rect.zero"/> when no ad is loaded.</para>
        /// <para>The position includes the insets for display cutouts.</para>
        /// </summary>
        public Rect rectInPixels {
            get{ return ad != null ? ad.rectInPixels : Rect.zero; }
        }

        /// <summary>
        /// Checks whether an ad is loaded and not expired.
        /// </summary>
        /// <returns><c>true</c> if a loaded ad can be shown; otherwise <c>false</c>.</returns>
        public bool IsAdLoaded()
        {
            return ad != null && !ad.isExpired;
        }

        /// <summary>
        /// Loads a new Native ad. When it is loaded, it replaces the current ad, which is destroyed.
        /// <para>The call is ignored while a previous load is still in progress.
        /// If the manager is not initialized yet, loading starts once it is ready.</para>
        /// <para><see cref="adChoicesPlacement"/>, <see cref="isStartVideoMuted"/> and <see cref="placement"/>
        /// are applied when this method is called.</para>
        /// <para>Subscribe to <see cref="OnAdLoaded"/> and <see cref="OnAdFailedToLoad"/> to get the result.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
            {
                loadOnAwake = true;
                return;
            }
            if (loading) return;
            loading = true;
            var options = new NativeAdOptions
            {
                adChoicesPlacement = adChoicesPlacement,
                isStartVideoMuted = isStartVideoMuted,
                placement = placement
            };
            manager.LoadNativeOverlayAd(options, AdLoadingResult);
        }

        /// <summary>
        /// Sets the position of the overlay on the screen and resets the offset set by
        /// <see cref="SetAdPosition(int, int, AdPosition)"/>.
        /// </summary>
        /// <param name="position">The position on the screen.</param>
        public void SetAdPosition(AdPosition position)
        {
            overlayPosition = position;
            positionOffset = Vector2Int.zero;
            if (ad == null) return;
            ad.SetPosition(position);
        }

        /// <summary>
        /// The Overlay will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// </summary>
        /// <param name="x">X-coordinate on screen in DP.</param>
        /// <param name="y">Y-coordinate on screen in DP.</param>
        /// <param name="position">The corner of the screen.</param>
        public void SetAdPosition(int x, int y, AdPosition position = AdPosition.TopLeft)
        {
            overlayPosition = position;
            positionOffset = new Vector2Int(x, y);
            if (ad == null) return;
            ad.SetPosition(position, x, y);
        }

        /// <summary>
        /// Change the size of the native ad template.
        /// <para>The template is re-rendered with the new size; the loaded ad content is not destroyed.</para>
        /// </summary>
        /// <param name="widthDp">Template width in DP.</param>
        /// <param name="heightDp">Template height in DP.</param>
        public void SetTemplateSize(int widthDp, int heightDp)
        {
            templateSize = new Vector2Int(widthDp, heightDp);
            if (ad == null) return;
            ad.RenderTemplate(widthDp, heightDp, templateStyle);
        }

        /// <summary>
        /// Change the style and size of the native ad template.
        /// <para>The template is re-rendered with the new style and size; the loaded ad content is not destroyed.</para>
        /// </summary>
        /// <param name="style">The new template style.</param>
        /// <param name="widthDp">Template width in DP.</param>
        /// <param name="heightDp">Template height in DP.</param>
        public void SetTemplateStyle(NativeTemplateStyle style, int widthDp, int heightDp)
        {
            this.templateStyle = style;
            templateSize = new Vector2Int(widthDp, heightDp);
            if (ad == null) return;
            ad.RenderTemplate(widthDp, heightDp, style);
        }

        /// <summary>
        /// Destroys the loaded Native ad and removes the overlay from the screen.
        /// <para>The ad is also destroyed automatically together with this MonoBehaviour,
        /// so call this method only to release the ad earlier.</para>
        /// <para>To show ads again, call <see cref="LoadAd"/>.</para>
        /// </summary>
        public void DestroyAd()
        {
            if (ad == null) return;

            ad.OnFailedToShow -= AdFailedToShow;
            ad.OnClicked -= OnAdClicked.Invoke;
            ad.OnImpression -= OnAdImpression.Invoke;

            ad.Dispose();
            ad = null;
        }

        #region MonoBehaviour

        private void Start()
        {
            if (!CASFactory.TryGetManagerByIndexAsync(managerId.index, OnManagerReady))
                OnAdFailedToLoad.Invoke(new AdError(AdError.NotInitialized, null).ToString());
        }

        private void OnEnable()
        {
            if (ad == null) return;
            ad.SetActive(true);
            OnAdShown.Invoke();
        }

        private void OnDisable()
        {
            if (ad == null) return;
            ad.SetActive(false);
            OnAdHidden.Invoke();
        }

        private void OnDestroy()
        {
            if (manager == null)
                CASFactory.OnManagerStateChanged -= OnManagerReady;
            DestroyAd();
        }

        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index)
                return;

            CASFactory.OnManagerStateChanged -= OnManagerReady;

            this.manager = manager;

            if (loadOnAwake)
            {
                loadOnAwake = false;
                LoadAd();
            }
        }

        private void AdLoadingResult(INativeOverlayAd ad, AdError error)
        {
            if (!this)
            {
                // Instance destroyed already
                ad?.Dispose();
                return;
            }
            loading = false;
            if (ad == null)
            {
                // Ad failed to load
                OnAdFailedToLoad.Invoke(error.GetMessage());
                return;
            }

            DestroyAd();

            this.ad = ad;
            ad.OnFailedToShow += AdFailedToShow;
            ad.OnClicked += OnAdClicked.Invoke;
            ad.OnImpression += OnAdImpression.Invoke;
            ad.SetPosition(overlayPosition, positionOffset.x, positionOffset.y);
            ad.RenderTemplate(templateSize.x, templateSize.y, templateStyle);

            OnAdLoaded.Invoke();
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void AdFailedToShow(string error)
        {
            DestroyAd();
            OnAdHidden.Invoke();
            OnAdFailedToLoad.Invoke(error);
        }
        #endregion
    }
}