//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal class CASViewClient : IAdView
    {
        private readonly CASManagerClient _manager;
        private IntPtr _viewRef;
        private IntPtr _viewClient;

        private AdPosition _position = AdPosition.BottomCenter;
        private int _positionX = 0;
        private int _positionY = 0;

        public event CASViewEvent OnLoaded;
        public event CASViewEventWithError OnFailed;
        public event CASViewEventWithMeta OnImpression;
        public event CASViewEvent OnClicked;

        public AdSize size { get; private set; }
        public Rect rectInPixels { get; private set; }
        public IMediationManager manager { get { return _manager; } }

        public bool isReady
        {
            get { return CASExterns.CASUIsAdViewReady(_viewRef); }
        }

        public int refreshInterval
        {
            get { return CASExterns.CASUGetAdViewRefreshInterval(_viewRef); }
            set { CASExterns.CASUSetAdViewRefreshInterval(_viewRef, value); }
        }

        public AdPosition position
        {
            get { return _position; }
            set { SetPosition(value, 0, 0); }
        }

        internal CASViewClient(CASManagerClient manager, AdSize size)
        {
            _manager = manager;
            this.size = size;
        }

        public void Attach(IntPtr viewRef, IntPtr client)
        {
            _viewRef = viewRef;
            _viewClient = client;
            CASExterns.CASUAttachAdViewDelegate(viewRef,
                AdViewLoadedCallback,
                AdViewFailedCallback,
                AdViewPresentedCallback,
                AdViewClickedCallback,
                AdViewRectCallback);
        }

        ~CASViewClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (_viewRef != IntPtr.Zero)
                {
                    _manager.RemoveAdViewFromFactory(this);
                    CASExterns.CASUDestroyAdView(_viewRef, _manager.managerID + "_" + (int)size);
                    _viewRef = IntPtr.Zero;
                    ((GCHandle)_viewClient).Free();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DisableRefresh()
        {
            refreshInterval = 0;
        }

        public void Load()
        {
            CASExterns.CASULoadAdView(_viewRef);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                CASExterns.CASUPresentAdView(_viewRef);
                return;
            }

            rectInPixels = Rect.zero;
            CASExterns.CASUHideAdView(_viewRef);
        }

        public void SetPosition(int x, int y)
        {
            SetPosition(AdPosition.TopLeft, x, y);
        }

        private void SetPosition(AdPosition position, int x, int y)
        {
            if (position == AdPosition.Undefined)
                return;
            if (position != _position || x != _positionX || y != _positionY)
            {
                _position = position;
                _positionX = x;
                _positionY = y;
                CASExterns.CASUSetAdViewPosition(_viewRef, (int)position, x, y);
            }
        }

        private static CASViewClient IntPtrToAdViewClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASViewClient;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewDidLoadCallback))]
        private static void AdViewLoadedCallback(IntPtr view)
        {
            try
            {
                var instance = IntPtrToAdViewClient(view);
                if (instance != null && instance.OnLoaded != null)
                    instance.OnLoaded(instance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewDidFailedCallback))]
        private static void AdViewFailedCallback(IntPtr view, int error)
        {
            try
            {
                var instance = IntPtrToAdViewClient(view);
                if (instance != null && instance.OnFailed != null)
                    instance.OnFailed(instance, (AdError)error);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewWillPresentCallback))]
        private static void AdViewPresentedCallback(IntPtr view, IntPtr impression)
        {
            try
            {
                var instance = IntPtrToAdViewClient(view);
                if (instance == null)
                    return;
                var metadata = new CASImpressionClient(AdType.Banner, impression);
                if (instance.OnImpression != null)
                    instance.OnImpression(instance, metadata);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewDidClickedCallback))]
        private static void AdViewClickedCallback(IntPtr view)
        {
            try
            {
                var instance = IntPtrToAdViewClient(view);
                if (instance != null && instance.OnClicked != null)
                    instance.OnClicked(instance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUViewDidRectCallback))]
        private static void AdViewRectCallback(IntPtr view, float x, float y, float width, float heigt)
        {
            try
            {
                var instance = IntPtrToAdViewClient(view);
                if (instance != null)
                    instance.rectInPixels = new Rect(x, y, width, heigt);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif