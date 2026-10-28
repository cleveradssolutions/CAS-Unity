// Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace CAS.Unity
{
    internal sealed class CASNativeOverlayClient : CASNativeOverlayBase
    {
        private readonly CASManagerBehaviour _behaviour;
        private bool _waitImpressionEvent = false;

        public bool loaded;
        public bool active;
        private bool _disposed;
        public int lastError = AdError.Internal;

        public int widthDp = 300;
        public int heightDp = 250;

        internal CASNativeOverlayClient(CASManagerClient manager, CASManagerBehaviour behaviour, CASNativeOverlayAdLoadCallback callback) : base(manager, callback)
        {
            _behaviour = behaviour;
        }

        public override bool isExpired { get { return _disposed; } }

        public override AdMetaData adContentInfo
        {
            get { return null; }
        }

        internal void Load(NativeAdOptions options)
        {
            CallAdAction(AdActionCode.LOADED);
        }

        public override void SetActive(bool active)
        {
            if (!_disposed) this.active = active;
        }

        protected override void SetPositionNative(int position, int x, int y)
        {
            CASFactory.UnityLog("Overlay position changed to " + ((AdPosition)position).ToString() + " with offset: x=" + x + ", y=" + y);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            if (x == 0 && y == 0)
            {
                SetPositionNative(position, x, y);
            }
            else
            {
                float scale = MobileAds.GetDeviceScreenScale();
                _positionX = (int)(x / scale);
                _positionY = (int)(y / scale);
                SetPositionNative(position, _positionX, _positionY);
            }
        }

        public override void RenderTemplate(int widthDp, int heightDp, NativeTemplateStyle style)
        {
            this.widthDp = widthDp > 0 ? widthDp : 300;
            this.heightDp = heightDp > 0 ? heightDp : 250;
            CalculateAdRectOnScreen();
        }

        internal void OnGUIAd(GUIStyle style)
        {
            if (!active || !loaded) return;

            if (_waitImpressionEvent)
            {
                _waitImpressionEvent = false;
                CallAdAction(AdActionCode.IMPRESSION);
            }

            CalculateAdRectOnScreen();
            if (GUI.Button(rectInPixels, "CAS.AI Native Overlay Ad", style))
            {
                CallAdAction(AdActionCode.CLICKED);
            }
        }

        private void CallAdAction(int action, float delay = 0.0f)
        {
            _behaviour.Post(() =>
            {
                if (action == AdActionCode.LOADED)
                {
                    CalculateAdRectOnScreen();
                    loaded = true;
                    _waitImpressionEvent = true;
                }
                HandleCallback(action, 0, lastError, null, null);
            }, delay);
        }

        private void CalculateAdRectOnScreen()
        {
            float scale = MobileAds.GetDeviceScreenScale();
            float width = widthDp * scale;
            float height = heightDp * scale;
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

            x = Mathf.Clamp(x + _positionX * scale, safe.xMin, safe.xMax - width);
            y = Mathf.Clamp(y + _positionY * scale, safe.yMin, safe.yMax - height);

            var rect = new Rect(x, y, width, height);
            rectInPixels = rect;
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            loaded = false;
            active = false;
            rectInPixels = Rect.zero;
            _behaviour._nativeAdOverlays.Remove(this);
        }
    }
}
#endif