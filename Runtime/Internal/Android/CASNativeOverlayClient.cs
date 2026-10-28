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

        
        public override AdMetaData adContentInfo
        {
            get
            {
                if (_bridge == null || isExpired)
                    return null;

                using (var nativeInfo = _bridge.Call<AndroidJavaObject>("getAdContentInfo"))
                {
                    if (nativeInfo == null)
                        return null;

                    var info = new CASImpressionClient(AdType.Native, nativeInfo);

                    return new CASAdMetaDataSnapshot(info);
                }
            }
        }

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
            if (_bridge != null) _bridge.Call("setPosition", position, x, y);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            if (_bridge != null) _bridge.Call("setPositionPx", position, x, y);
        }

        public override void RenderTemplate(int widthDp, int heightDp, NativeTemplateStyle style)
        {
            if (_bridge != null) _bridge.Call("renderTemplate", ToNativeStyleArray(style), widthDp, heightDp);
        }

        public override void Dispose()
        {
             _manager.UnregisterNativeOverlay(this);
            
            _loadCallback = null;
            if (_bridge == null) return;

            try { _bridge.Call("destroy"); }
            finally
            {
                _bridge.Dispose();
                _bridge = null;
            }
        }

        
    }
}
#endif