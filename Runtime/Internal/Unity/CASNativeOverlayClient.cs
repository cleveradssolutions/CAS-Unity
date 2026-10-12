// Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace CAS.Unity
{
    internal sealed class CASNativeOverlayClient : INativeOverlayAd
    {
        private readonly CASManagerClient _manager;
        private readonly CASManagerBehaviour _behaviour;

        private bool _loaded;
        private bool _active;
        private bool _disposed;

        private Rect _rect;
        private AdPosition _position = AdPosition.BottomCenter;
        private int _offsetX;
        private int _offsetY;
        private int _widthDp = 300;
        private int _heightDp = 250;

        internal CASNativeOverlayClient(CASManagerClient manager, CASManagerBehaviour behaviour)
        {
            _manager = manager;
            _behaviour = behaviour;
        }

        public event Action<AdMetaData> OnImpression;
        public event Action<AdError> OnFailedToShow;
        public event Action OnClicked;
        public event Action<Rect> OnRectChanged;

        public IMediationManager manager => _manager;
        public bool isExpired => _disposed;
        public Rect rectInPixels => _rect;

        internal void Load(NativeAdOptions options, CASNativeOverlayAdLoadCallback callback)
        {
            _behaviour.Post(() =>
            {
                if (_disposed) return;
                _loaded = true;
                callback?.Invoke(this, default(AdError));
            }, 0.5f);
        }

        public void SetActive(bool active)
        {
            if (!_disposed) _active = active;
        }

        public void SetPosition(AdPosition position)
        {
            _position = position;
            _offsetX = 0;
            _offsetY = 0;
            CalculateRect();
        }

        public void SetPosition(int xDp, int yDp, AdPosition position)
        {
            _position = position;
            _offsetX = xDp;
            _offsetY = yDp;
            CalculateRect();
        }

        public void SetPositionPx(int xPx, int yPx, AdPosition position)
        {
            float scale = MobileAds.GetDeviceScreenScale();
            SetPosition((int)(xPx / scale), (int)(yPx / scale), position);
        }

        public void RenderTemplate(NativeTemplateStyle style, int widthDp, int heightDp)
        {
            _widthDp = widthDp > 0 ? widthDp : 300;
            _heightDp = heightDp > 0 ? heightDp : 250;
            CalculateRect();
        }

        internal void OnGUIAd(GUIStyle style)
        {
            if (!_active || !_loaded || _disposed) return;

            CalculateRect();
            if (GUI.Button(_rect, "CAS.AI Native Overlay Ad", style)) OnClicked?.Invoke();
        }

        private void CalculateRect()
        {
            float scale = MobileAds.GetDeviceScreenScale();
            float width = _widthDp * scale;
            float height = _heightDp * scale;
            Rect safe = Screen.safeArea;

            float x;
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

            float y;
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

            x = Mathf.Clamp(x + _offsetX * scale, safe.xMin, safe.xMax - width);
            y = Mathf.Clamp(y + _offsetY * scale, safe.yMin, safe.yMax - height);

            var rect = new Rect(x, y, width, height);
            if (_rect == rect) return;

            _rect = rect;
            OnRectChanged?.Invoke(_rect);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _loaded = false;
            _active = false;
            _rect = Rect.zero;
            _behaviour.RemoveNativeOverlay(this);
        }
    }
}
#endif