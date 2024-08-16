﻿//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal class CASViewClient : CASViewBase
    {
        private IntPtr _viewRef;
        private IntPtr _viewClient;

        public override bool isReady
        {
            get { return CASExterns.CASUIsAdViewReady(_viewRef); }
        }

        public override int refreshInterval
        {
            get { return CASExterns.CASUGetAdViewRefreshInterval(_viewRef); }
            set { CASExterns.CASUSetAdViewRefreshInterval(_viewRef, value); }
        }

        internal CASViewClient(CASManagerBase manager, AdSize size) : base(manager, size) { }

        public void Attach(IntPtr viewRef, IntPtr client)
        {
            _viewRef = viewRef;
            _viewClient = client;
            CASExterns.CASUAttachAdViewDelegate(viewRef,
                AdViewActionCallback,
                AdViewImpressionCallback,
                AdViewRectCallback);
        }

        ~CASViewClient()
        {
            Dispose();
        }

        public override void Dispose()
        {
            try
            {
                if (_viewRef != IntPtr.Zero)
                {
                    // Ignoer base implementation before CAS iOS 4.0
                    //base.Dispose();
                    _manager.RemoveAdViewFromFactory(this);
                    CASExterns.CASUDestroyAdView(_viewRef);
                    _viewRef = IntPtr.Zero;
                    ((GCHandle)_viewClient).Free();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal override void Enable()
        {
            // Temptimplementation before CAS iOS 4.0
            var iosManager = _manager as CASManagerClient;
            iosManager.EnableAd(AdType.Banner);
        }

        internal override void DestroyNative()
        {
            // Temptimplementation before CAS iOS 4.0
            var iosManager = _manager as CASManagerClient;
            iosManager.DisposeAd(AdType.Banner);
        }

        public override void LoadNative()
        {
            CASExterns.CASULoadAdView(_viewRef);
        }

        public override void SetActive(bool active)
        {
            if (active)
            {
                CASExterns.CASUPresentAdView(_viewRef);
                return;
            }

            CASExterns.CASUHideAdView(_viewRef);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            CASExterns.CASUSetAdViewPositionPx(_viewRef, position, x, y);
        }

        protected override void SetPositionNative(int position, int x, int y)
        {
            CASExterns.CASUSetAdViewPosition(_viewRef, position, x, y);
        }

        private static CASViewClient GetClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASViewClient;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewActionCallback))]
        private static void AdViewActionCallback(IntPtr view, int action, int error)
        {
            try
            {
                GetClient(view).HandleCallback(action, 0, error, null, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewImpressionCallback))]
        private static void AdViewImpressionCallback(IntPtr view, IntPtr impression)
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

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewRectCallback))]
        private static void AdViewRectCallback(IntPtr view, float x, float y, float width, float heigt)
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