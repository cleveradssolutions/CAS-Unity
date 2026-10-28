// Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal sealed class CASNativeOverlayClient : CASNativeOverlayBase
    {
        private IntPtr _overlayRef;
        private IntPtr _overlayClient;

        internal CASNativeOverlayClient(CASManagerBase manager, CASNativeOverlayAdLoadCallback callback) : base(manager, callback)
        {
        }

        internal void Load(IntPtr overlayRef, IntPtr client, NativeAdOptions options)
        {
            if (options == null) options = new NativeAdOptions();
            _overlayRef = overlayRef;
            _overlayClient = client;
            CASExterns.CASUSetNativeOverlayDelegateAndLoadAd(
                overlayRef,
                NativeOverlayAdActionCallback,
                NativeOverlayAdImpressionCallback,
                NativeOverlayAdRectCallback,
                (int)options.adChoicesPlacement,
                options.isStartVideoMuted,
                options.placement);
        }

        ~CASNativeOverlayClient()
        {
            Dispose();
        }

        public override void Dispose()
        {
            _loadCallback = null;
            try
            {
                if (_overlayRef != IntPtr.Zero)
                {
                    _overlayRef = IntPtr.Zero;
                    ((GCHandle)_overlayClient).Free();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override bool isExpired
        {
            get
            {
                return CASExterns.CASUIsNativeOverlayAdExpired(_overlayRef);
            }
        }


        public override AdMetaData adContentInfo { get { return null; } }

        public override void SetActive(bool active)
        {
            if (active)
                CASExterns.CASUPresentNativeOverlayAd(_overlayRef);
            else
                CASExterns.CASUHideNativeOverlayAd(_overlayRef);
        }

        protected override void SetPositionNative(int position, int x, int y)
        {
            CASExterns.CASUSetNativeOverlayAdPosition(_overlayRef, position, x, y);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            CASExterns.CASUSetNativeOverlayAdPositionPx(_overlayRef, position, x, y);
        }

        public override void RenderTemplate(int widthDp, int heightDp, NativeTemplateStyle style)
        {
            CASExterns.CASURenderNativeOverlayAd(_overlayRef, widthDp, heightDp, ToNativeStyleArray(style));
        }

        private static CASNativeOverlayClient GetClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASNativeOverlayClient;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUNativeOverlayAdActionCallback))]
        private static void NativeOverlayAdActionCallback(IntPtr view, int action, int error, string errorMessage)
        {
            try
            {
                GetClient(view).HandleCallback(action, 0, error, errorMessage, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUNativeOverlayAdImpressionCallback))]
        private static void NativeOverlayAdImpressionCallback(IntPtr view, IntPtr impression)
        {
            try
            {
                GetClient(view).HandleCallback(AdActionCode.IMPRESSION, 0, 0, null, impression);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUNativeOverlayAdRectCallback))]
        private static void NativeOverlayAdRectCallback(IntPtr view, float x, float y, float width, float heigt)
        {
            try
            {
                GetClient(view).rectInPixels = new Rect(x, y, width, heigt);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif