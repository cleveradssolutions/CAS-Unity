// Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal sealed class CASNativeOverlayClient : CASNativeOverlayBase, CASCallback.Handler, CASCallback.RectHandler
    {
        private readonly CASCallback _callback;
        private AndroidJavaObject _bridge;

        internal CASNativeOverlayClient(CASManagerBase manager, CASNativeOverlayAdLoadCallback callback) : base(manager, callback)
        {
            _callback = new CASCallback(this);
            _bridge = new AndroidJavaObject(CASJavaBridge.NativeOverlayClass, _callback);
        }

        public override bool isExpired { get { return _bridge == null || _bridge.Call<bool>("isExpired"); } }

        internal void Load(NativeAdOptions options)
        {
            if (options == null) options = new NativeAdOptions();
            _bridge.Call("load", _manager.managerID, (int)options.adChoicesPlacement, options.isStartVideoMuted, options.placement);
        }

        public override void SetActive(bool active)
        {
            if (_bridge != null) _bridge.Call(active ? "show" : "hide");
        }

        protected override void SetPositionNative(int position, int x, int y)
        {
            if (_bridge != null) _bridge.Call("setPosition", (int)position, x, y);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            if (_bridge != null) _bridge.Call("setPositionPx", (int)position, x, y);
        }

        public override void RenderTemplate(int widthDp, int heightDp, NativeTemplateStyle style)
        {
            if (_bridge != null) _bridge.Call("renderTemplate", ToBridgeStyle(style), widthDp, heightDp);
        }

        public override void Dispose()
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