//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace CAS.Unity
{
    internal sealed class CASNativeOverlayClient : INativeOverlayAdClient
    {
        private bool _ready;
        private bool _active;
        private string _placement;
        private Rect _rect;
        private AdPosition _position = AdPosition.BottomCenter;
        private int _offsetX;
        private int _offsetY;
        private int _widthDp = 300;
        private int _heightDp = 250;

        public event Action OnAdLoaded;
        public event Action<AdError> OnAdFailedToLoad;
        public event Action<AdError> OnAdFailedToShow;
        public event Action OnAdClicked;
        public event Action<AdMetaData> OnAdImpression;
        public event Action<Rect> OnAdRectChanged;

        public bool isReady => _ready;
        public string placement { get => _placement; set => _placement = value; }
        public Rect rectInPixels => _rect;
        public int widthInPixels => (int)_rect.width;
        public int heightInPixels => (int)_rect.height;

        public void Load()
        {
            _ready = true;
            CalculateRect();
            OnAdLoaded?.Invoke();
        }

        public void Show()
        {
            _active = true;
        }

        public void Hide()
        {
            _active = false;
        }

        public void SetPosition(AdPosition position)
        {
            _position = position;
            _offsetX = 0;
            _offsetY = 0;
            CalculateRect();
        }

        public void SetPosition(int x, int y, AdPosition position)
        {
            _position = position;
            _offsetX = x;
            _offsetY = y;
            CalculateRect();
        }

        public void SetPositionPx(int x, int y, AdPosition position)
        {
            float scale = MobileAds.GetDeviceScreenScale();
            SetPosition((int)(x / scale), (int)(y / scale), position);
        }

        public void Render(int widthDp, int heightDp)
        {
            _widthDp = widthDp;
            _heightDp = heightDp;
            CalculateRect();
        }

        public void RenderDefault()
        {
            Render(300, 250);
        }

        public void OnGUIAd(GUIStyle style)
        {
            if (!_active || !_ready)
                return;

            CalculateRect();

            if (GUI.Button(_rect, "CAS.AI Native Overlay Ad", style))
                OnAdClicked?.Invoke();
        }

        private void CalculateRect()
        {
            float scale = MobileAds.GetDeviceScreenScale();
            float width = _widthDp * scale;
            float height = _heightDp * scale;
            Rect safe = Screen.safeArea;

            float x;
            float y;

            switch (_position)
            {
                case AdPosition.TopLeft:
                case AdPosition.MiddleLeft:
                case AdPosition.BottomLeft:
                    x = safe.xMin + _offsetX * scale;
                    break;
                case AdPosition.TopRight:
                case AdPosition.MiddleRight:
                case AdPosition.BottomRight:
                    x = safe.xMax - width - _offsetX * scale;
                    break;
                default:
                    x = safe.center.x - width * 0.5f + _offsetX * scale;
                    break;
            }

            switch (_position)
            {
                case AdPosition.TopLeft:
                case AdPosition.TopCenter:
                case AdPosition.TopRight:
                    y = safe.yMin + _offsetY * scale;
                    break;
                case AdPosition.BottomLeft:
                case AdPosition.BottomCenter:
                case AdPosition.BottomRight:
                    y = safe.yMax - height - _offsetY * scale;
                    break;
                default:
                    y = safe.center.y - height * 0.5f + _offsetY * scale;
                    break;
            }

            x = Mathf.Clamp(x, safe.xMin, safe.xMax - width);
            y = Mathf.Clamp(y, safe.yMin, safe.yMax - height);

            _rect = new Rect(x, y, width, height);
            OnAdRectChanged?.Invoke(_rect);
        }

        public void SetBackgroundColor(Color color) { }
        public void SetHeadlineColor(Color color) { }
        public void SetBodyColor(Color color) { }
        public void SetAdvertiserColor(Color color) { }
        public void SetCallToActionTextColor(Color color) { }
        public void SetCallToActionBackgroundColor(Color color) { }

        public void Dispose()
        {
            _ready = false;
            _active = false;
            _rect = Rect.zero;
        }
    }
}
#endif