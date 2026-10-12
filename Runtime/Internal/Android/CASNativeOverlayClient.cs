// Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal sealed class CASNativeOverlayClient : INativeOverlayAd, CASCallback.Handler, CASCallback.RectHandler
    {
        private readonly CASManagerBase _manager;
        private readonly CASCallback _callback;
        private AndroidJavaObject _bridge;
        private CASNativeOverlayAdLoadCallback _loadCallback;
        private Rect _rectInPixels;

        internal CASNativeOverlayClient(CASManagerBase manager)
        {
            _manager = manager;
            _callback = new CASCallback(this);
            _bridge = new AndroidJavaObject(CASJavaBridge.NativeOverlayClass, _callback, manager.managerID);
        }

        public event Action<AdMetaData> OnImpression;
        public event Action<AdError> OnFailedToShow;
        public event Action OnClicked;
        public event Action<Rect> OnRectChanged;

        public IMediationManager manager => _manager;
        public bool isExpired => _bridge == null || _bridge.Call<bool>("isExpired");
        public Rect rectInPixels => _rectInPixels;

        internal void Load(NativeAdOptions options, CASNativeOverlayAdLoadCallback callback)
        {
            if (options == null) options = new NativeAdOptions();
            _loadCallback = callback;
            _bridge.Call("load", (int)options.adChoicesPlacement, options.isStartVideoMuted, options.placement);
        }

        public void SetActive(bool active)
        {
            if (_bridge != null) _bridge.Call(active ? "show" : "hide");
        }

        public void SetPosition(AdPosition position)
        {
            if (_bridge != null) _bridge.Call("setPosition", (int)position);
        }

        public void SetPosition(int xDp, int yDp, AdPosition position)
        {
            if (_bridge != null) _bridge.Call("setPosition", (int)position, xDp, yDp);
        }

        public void SetPositionPx(int xPx, int yPx, AdPosition position)
        {
            if (_bridge != null) _bridge.Call("setPositionPx", (int)position, xPx, yPx);
        }

        public void RenderTemplate(NativeTemplateStyle style, int widthDp, int heightDp)
        {
            if (_bridge != null) _bridge.Call("renderTemplate", ToBridgeStyle(style), widthDp, heightDp);
        }

        public void HandleCallback(int action, int type, int error, string errorMessage, object impression)
        {
            if (type != AdTypeCode.NATIVE) return;

            switch (action)
            {
                case AdActionCode.LOADED:
                    var loaded = _loadCallback;
                    _loadCallback = null;
                    loaded?.Invoke(this, default(AdError));
                    break;

                case AdActionCode.FAILED:
                    var failed = _loadCallback;
                    _loadCallback = null;
                    var loadError = new AdError(error, errorMessage);
                    Dispose();
                    failed?.Invoke(null, loadError);
                    break;

                case AdActionCode.SHOW_FAILED:
                    OnFailedToShow?.Invoke(new AdError(error, errorMessage));
                    break;

                case AdActionCode.CLICKED:
                    OnClicked?.Invoke();
                    break;

                case AdActionCode.IMPRESSION:
                    OnImpression?.Invoke(_manager.WrapImpression(AdType.Native, impression));
                    break;
            }
        }

        public void HandleRect(int x, int y, int width, int height)
        {
            _rectInPixels = new Rect(x, y, width, height);
            OnRectChanged?.Invoke(_rectInPixels);
        }

        public void Dispose()
        {
            _loadCallback = null;
            if (_bridge == null) return;

            try { _bridge.Call("destroy"); }
            finally
            {
                _bridge.Dispose();
                _bridge = null;
            }
        }

        private static int[] ToBridgeStyle(NativeTemplateStyle style)
        {
            if (style == null) return null;

            var data = new int[18];
            data[1] = ToAndroidColor(style.MainBackgroundColor);
            WriteTextStyle(data, 2, 1, style.Headline);
            WriteTextStyle(data, 6, 2, style.Body);
            WriteTextStyle(data, 10, 4, style.CallToAction);
            WriteTextStyle(data, 14, 8, style.Other);
            return data;
        }

        private static void WriteTextStyle(int[] data, int offset, int flag, NativeTemplateTextStyle style)
        {
            if (style == null) return;

            data[0] |= flag;
            data[offset] = ToAndroidColor(style.BackgroundColor);
            data[offset + 1] = ToAndroidColor(style.FontColor);
            data[offset + 2] = style.FontSize;
            data[offset + 3] = (int)style.Style;
        }

        private static int ToAndroidColor(Color color)
        {
            Color32 value = color;
            uint argb = ((uint)value.a << 24) | ((uint)value.r << 16) | ((uint)value.g << 8) | value.b;
            return unchecked((int)argb);
        }
    }
}
#endif