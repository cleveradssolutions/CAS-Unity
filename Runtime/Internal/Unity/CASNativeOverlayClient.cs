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

        private NativeTemplateStyle _style;

        public bool isReady => _ready;
        public string placement { get => _placement; set => _placement = value; }
        public Rect rectInPixels => _rect;
        public int widthInPixels => (int)_rect.width;
        public int heightInPixels => (int)_rect.height;


        public void RenderTemplate(NativeTemplateStyle style, int widthDp, int heightDp)
        {
            _style = style;

            _widthDp = widthDp > 0
                ? widthDp
                : 300;

            _heightDp = heightDp > 0
                ? heightDp
                : 250;

            CalculateRect();
        }
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
                    x = safe.xMin;
                    break;

                case AdPosition.TopRight:
                case AdPosition.MiddleRight:
                case AdPosition.BottomRight:
                    x = safe.xMax - width;
                    break;

                default:
                    x = safe.center.x - width * 0.5f;
                    break;
            }

            switch (_position)
            {
                case AdPosition.TopLeft:
                case AdPosition.TopCenter:
                case AdPosition.TopRight:
                    y = safe.yMin;
                    break;

                case AdPosition.BottomLeft:
                case AdPosition.BottomCenter:
                case AdPosition.BottomRight:
                    y = safe.yMax - height;
                    break;

                default:
                    y = safe.center.y - height * 0.5f;
                    break;
            }

            x = Mathf.Clamp(
                x + _offsetX * scale,
                safe.xMin,
                safe.xMax - width
            );

            y = Mathf.Clamp(
                y + _offsetY * scale,
                safe.yMin,
                safe.yMax - height
            );

            var newRect = new Rect(x, y, width, height);

            if (_rect == newRect)
                return;

            _rect = newRect;
            OnAdRectChanged?.Invoke(_rect);
        }

        public void Dispose()
        {
            _ready = false;
            _active = false;
            _rect = Rect.zero;
        }
    }
}
#endif