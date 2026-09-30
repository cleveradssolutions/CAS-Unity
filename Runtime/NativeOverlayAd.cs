using System;
using UnityEngine;

namespace CAS
{
    public sealed class NativeOverlayAd : IDisposable
    {
        private INativeOverlayAdClient _client;

        public NativeOverlayAd(IMediationManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            var baseManager = manager as CASManagerBase;
            if (baseManager == null) throw new ArgumentException("Invalid CAS mediation manager.", nameof(manager));
            _client = CASFactory.CreateNativeOverlayClient(baseManager);
        }

        public event Action OnAdLoaded
        {
            add { if (_client != null) _client.OnAdLoaded += value; }
            remove { if (_client != null) _client.OnAdLoaded -= value; }
        }

        public event Action<AdError> OnAdFailedToLoad
        {
            add { if (_client != null) _client.OnAdFailedToLoad += value; }
            remove { if (_client != null) _client.OnAdFailedToLoad -= value; }
        }

        public event Action<AdError> OnAdFailedToShow
        {
            add { if (_client != null) _client.OnAdFailedToShow += value; }
            remove { if (_client != null) _client.OnAdFailedToShow -= value; }
        }

        public event Action OnAdClicked
        {
            add { if (_client != null) _client.OnAdClicked += value; }
            remove { if (_client != null) _client.OnAdClicked -= value; }
        }

        public event Action<AdMetaData> OnAdImpression
        {
            add { if (_client != null) _client.OnAdImpression += value; }
            remove { if (_client != null) _client.OnAdImpression -= value; }
        }

        public event Action<Rect> OnAdRectChanged
        {
            add { if (_client != null) _client.OnAdRectChanged += value; }
            remove { if (_client != null) _client.OnAdRectChanged -= value; }
        }

        public bool isReady => _client != null && _client.isReady;
        public Rect rectInPixels => _client != null ? _client.rectInPixels : Rect.zero;
        public int widthInPixels => _client != null ? _client.widthInPixels : 0;
        public int heightInPixels => _client != null ? _client.heightInPixels : 0;

        public string placement
        {
            get => _client != null ? _client.placement : null;
            set { if (_client != null) _client.placement = value; }
        }

        public void Load() { _client?.Load(); }
        public void Show() { _client?.Show(); }
        public void Hide() { _client?.Hide(); }
        public void SetPosition(AdPosition position) { _client?.SetPosition(position); }
        public void SetPosition(int x, int y, AdPosition position = AdPosition.TopLeft) { _client?.SetPosition(x, y, position); }
        public void SetPositionPx(int x, int y, AdPosition position = AdPosition.TopLeft) { _client?.SetPositionPx(x, y, position); }
        public void Render(int widthDp, int heightDp) { _client?.Render(widthDp, heightDp); }
        public void RenderDefault() { _client?.RenderDefault(); }
        public void SetBackgroundColor(Color color) { _client?.SetBackgroundColor(color); }
        public void SetHeadlineColor(Color color) { _client?.SetHeadlineColor(color); }
        public void SetBodyColor(Color color) { _client?.SetBodyColor(color); }
        public void SetAdvertiserColor(Color color) { _client?.SetAdvertiserColor(color); }
        public void SetCallToActionTextColor(Color color) { _client?.SetCallToActionTextColor(color); }
        public void SetCallToActionBackgroundColor(Color color) { _client?.SetCallToActionBackgroundColor(color); }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}