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
        [SerializeField] private bool useDefaultSize = true;
        [SerializeField] private Vector2Int templateSize = new Vector2Int(300, 250);
        [SerializeField] private string placement;

        public UnityEvent OnAdLoaded;
        public CASUEventWithError OnAdFailedToLoad;
        public CASUEventWithError OnAdFailedToShow;
        public UnityEvent OnAdShown;
        public UnityEvent OnAdClicked;
        public UnityEvent OnAdHidden;
        public CASUEventWithMeta OnAdImpression;

        private NativeOverlayAd _ad;
        private bool _loadWhenReady;

        public bool isAdReady => _ad != null && _ad.isReady;
        public Rect rectInPixels => _ad != null ? _ad.rectInPixels : Rect.zero;

        public void LoadAd()
        {
            if (_ad == null) _loadWhenReady = true;
            else _ad.Load();
        }

        public void ShowAd()
        {
            if (_ad == null) return;
            _ad.Show();
            if (_ad.isReady) OnAdShown.Invoke();
        }

        public void HideAd()
        {
            if (_ad == null) return;
            _ad.Hide();
            OnAdHidden.Invoke();
        }

        public void SetAdPosition(AdPosition position)
        {
            adPosition = position;
            adOffset = Vector2Int.zero;
            _ad?.SetPosition(position);
        }

        public void SetAdPosition(int x, int y, AdPosition position = AdPosition.TopLeft)
        {
            adPosition = position;
            adOffset = new Vector2Int(x, y);
            _ad?.SetPosition(x, y, position);
        }

        public void SetTemplateSize(int widthDp, int heightDp)
        {
            useDefaultSize = false;
            templateSize = new Vector2Int(widthDp, heightDp);
            _ad?.Render(widthDp, heightDp);
        }

        public void SetDefaultTemplateSize()
        {
            useDefaultSize = true;
            _ad?.RenderDefault();
        }

        public void SetPlacement(string value)
        {
            placement = value;
            if (_ad != null) _ad.placement = value;
        }

        private void Start()
        {
            CASFactory.TryGetManagerByIndexAsync(managerId.index, OnManagerReady);
        }

        private void OnEnable()
        {
            if (_ad == null) return;
            ApplyPosition();
            _ad.Show();
            if (_ad.isReady) OnAdShown.Invoke();
        }

        private void OnDisable()
        {
            if (_ad == null) return;
            _ad.Hide();
            OnAdHidden.Invoke();
        }

        private void OnDestroy()
        {
            CASFactory.OnManagerStateChanged -= OnManagerReady;
            Detach();
        }

        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index)
                return;

            CASFactory.OnManagerStateChanged -= OnManagerReady;

            _ad = new NativeOverlayAd(manager);
            _ad.OnAdLoaded += AdLoaded;
            _ad.OnAdFailedToLoad += AdFailedToLoad;
            _ad.OnAdFailedToShow += AdFailedToShow;
            _ad.OnAdClicked += OnAdClicked.Invoke;
            _ad.OnAdImpression += OnAdImpression.Invoke;

            _ad.placement = placement;

            if (useDefaultSize)
                _ad.RenderDefault();
            else
                _ad.Render(templateSize.x, templateSize.y);

            ApplyPosition();

            if (isActiveAndEnabled)
                _ad.Show();

            _loadWhenReady = false;
            _ad.Load();
        }

        private void ApplyPosition()
        {
            _ad?.SetPosition(adOffset.x, adOffset.y, adPosition);
        }

        private void AdLoaded()
        {
            OnAdLoaded.Invoke();
            if (!isActiveAndEnabled) return;
            _ad.Show();
            OnAdShown.Invoke();
        }

        private void AdFailedToLoad(AdError error)
        {
            OnAdFailedToLoad.Invoke(error.GetMessage());
        }

        private void AdFailedToShow(AdError error)
        {
            OnAdFailedToShow.Invoke(error.GetMessage());
        }

        private void Detach()
        {
            if (_ad == null) return;
            _ad.OnAdLoaded -= AdLoaded;
            _ad.OnAdFailedToLoad -= AdFailedToLoad;
            _ad.OnAdFailedToShow -= AdFailedToShow;
            _ad.OnAdClicked -= OnAdClicked.Invoke;
            _ad.OnAdImpression -= OnAdImpression.Invoke;
            _ad.Dispose();
            _ad = null;
        }
    }
}