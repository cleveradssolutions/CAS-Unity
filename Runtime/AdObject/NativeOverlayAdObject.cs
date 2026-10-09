using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Native Overlay Ad Object")]
    [DisallowMultipleComponent]
    public sealed class NativeOverlayAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;

        [SerializeField] private AdPosition adPosition = AdPosition.BottomCenter;
        [SerializeField] private Vector2Int adOffset = Vector2Int.zero;
        [SerializeField] private Vector2Int templateSize = new Vector2Int(300, 250);

        [SerializeField] private NativeTemplateStyle style = new NativeTemplateStyle();

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

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdHidden;
        public CASUEventWithMeta OnAdImpression;

        private IMediationManager manager;
        private INativeOverlayAd ad;
        private bool loadWhenReady;
        private bool loading;

        /// <summary>
        /// Get the real Overlay rect with position and size in pixels on screen.
        /// <para>Return <see cref="Rect.zero"/> when ad view is not active.</para>
        /// <para>The position on the screen is calculated with the addition of indents for the cutouts.</para>
        /// </summary>
        public Rect rectInPixels => ad != null ? ad.rectInPixels : Rect.zero;

        /// <summary>
        /// Check ready ad to present.
        /// </summary>
        public bool IsAdLoaded()
        {
            return ad != null && !ad.isExpired;
        }

        /// <summary>
        /// Manual load the Ad or reload current loaded Ad to skip impression.
        /// <para>You can get a callback for the successful loading of an ad by subscribe to <see cref="OnAdLoaded"/>.</para>
        /// </summary>
        public void LoadAd()
        {
            if (manager == null)
            {
                loadWhenReady = true;
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
            manager.LoadNativeOverlayAd(options, AdLoaded);
        }

        /// <summary>
        /// The position where the Overlay ad should be placed.
        /// The <see cref="AdPosition"/> enum lists the valid ad position values.
        /// </summary>
        public void SetAdPosition(AdPosition position)
        {
            adPosition = position;
            adOffset = Vector2Int.zero;
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
            adPosition = position;
            adOffset = new Vector2Int(x, y);
            if (ad == null) return;
            ad.SetPosition(x, y, position);
        }

        public void SetTemplateSize(int widthDp, int heightDp)
        {
            templateSize = new Vector2Int(widthDp, heightDp);
            if (ad == null) return;
            ad.RenderTemplate(style, widthDp, heightDp);
        }

        public void SetTemplateStyle(NativeTemplateStyle style, int widthDp, int heightDp)
        {
            this.style = style;
            templateSize = new Vector2Int(widthDp, heightDp);
            if (ad == null) return;
            ad.RenderTemplate(style, widthDp, heightDp);
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
            Detach();
        }

        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index)
                return;

            CASFactory.OnManagerStateChanged -= OnManagerReady;

            this.manager = manager;

            if (loadWhenReady)
            {
                loadWhenReady = false;
                LoadAd();
            }
        }

        private void AdLoaded(INativeOverlayAd ad, AdError error)
        {
            if (!this)
            {
                // Instance destroyed already
                ad.Dispose();
                return;
            }
            loading = false;
            if (ad == null)
            {
                // Ad failed to load
                OnAdFailedToLoad.Invoke(error.GetMessage());
                return;
            }

            Detach();

            this.ad = ad;
            ad.OnFailedToShow += AdFailedToShow;
            ad.OnClicked += OnAdClicked.Invoke;
            ad.OnImpression += OnAdImpression.Invoke;
            ad.SetPosition(adOffset.x, adOffset.y, adPosition);
            ad.RenderTemplate(null, templateSize.x, templateSize.y);

            OnAdLoaded.Invoke();
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void AdFailedToShow(AdError error)
        {
            OnAdFailedToShow.Invoke(error.GetMessage());
        }

        private void Detach()
        {
            if (ad == null) return;
            ad.Dispose();
            ad = null;
        }
        #endregion
    }
}