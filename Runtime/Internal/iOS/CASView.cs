//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal class CASView : IAdView
    {
        private readonly CASMediationManager _manager;
        public IntPtr _viewRef;
        private IntPtr _viewClient;

        private int _refreshInterval = -1;
        private AdPosition _position = AdPosition.BottomCenter;
        private int _positionX = 0;
        private int _positionY = 0;
        private bool _waitOfHideCallback;

        public event CASViewEvent OnLoaded;
        public event CASViewEventWithError OnFailed;
        public event CASViewEventWithMeta OnPresented;
        public event CASViewEvent OnClicked;
        public event CASViewEvent OnHidden;

        public AdSize size { get; }
        public IMediationManager manager { get { return _manager; } }


        public Rect rectInPixels
        {
            get
            {
                return new Rect(
                    CASExterns.CASUGetAdViewXOffsetInPixels( _viewRef ),
                    CASExterns.CASUGetAdViewYOffsetInPixels( _viewRef ),
                    CASExterns.CASUGetAdViewWidthInPixels( _viewRef ),
                    CASExterns.CASUGetAdViewHeightInPixels( _viewRef )
                    );
            }
        }

        public bool isReady
        {
            get { return CASExterns.CASUIsAdViewReady( _viewRef ); }
        }

        public int refreshInterval
        {
            get { return _refreshInterval; }
            set
            {
                if (_refreshInterval != value)
                {
                    _refreshInterval = value;
                    CASExterns.CASUSetAdViewRefreshInterval( _viewRef, value );
                }
            }
        }

        public AdPosition position
        {
            get { return _position; }
            set { SetPosition( value, 0, 0 ); }
        }

        internal CASView( CASMediationManager manager, AdSize size )
        {
            _manager = manager;
            this.size = size;
        }

        public void Attach( IntPtr viewRef, IntPtr client )
        {
            _viewRef = viewRef;
            _viewClient = client;
            CASExterns.CASUAttachAdViewDelegate( viewRef,
                AdViewLoadedCallback,
                AdViewFailedCallback,
                AdViewPresentedCallback,
                AdViewClickedCallback );
        }

        ~CASView()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (_viewRef != IntPtr.Zero)
                {
                    _manager.CallbackOnDestroy( this );
                    _viewRef = IntPtr.Zero;
                    ( ( GCHandle )_viewClient ).Free();
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        public void DisableRefresh()
        {
            refreshInterval = 0;
        }

        public void Load()
        {
            CASExterns.CASULoadAdView( _viewRef );
        }

        public void SetActive( bool active )
        {
            if (active)
            {
                CASExterns.CASUPresentAdView( _viewRef );
                return;
            }

            CASExterns.CASUHideAdView( _viewRef );
            if (_waitOfHideCallback)
            {
                _waitOfHideCallback = false;
                if (OnHidden != null)
                    OnHidden( this );
            }
        }

        public void SetPosition( int x, int y )
        {
            SetPosition( AdPosition.TopLeft, x, y );
        }

        private void SetPosition( AdPosition position, int x, int y )
        {
            if (position == AdPosition.Undefined)
                return;
            if (position != _position || x != _positionX || y != _positionY)
            {
                _position = position;
                _positionX = x;
                _positionY = y;
                CASExterns.CASUSetAdViewPosition( _viewRef, ( int )position, x, y );
            }
        }

        private static CASView IntPtrToAdViewClient( IntPtr managerClient )
        {
            GCHandle handle = ( GCHandle )managerClient;
            return handle.Target as CASView;
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidLoadedAdCallback ) )]
        private static void AdViewLoadedCallback( IntPtr view )
        {
            try
            {
                var instance = IntPtrToAdViewClient( view );
                if (instance != null && instance.OnLoaded != null)
                    instance.OnLoaded( instance );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidFailedAdCallback ) )]
        private static void AdViewFailedCallback( IntPtr view, int error )
        {
            try
            {
                var instance = IntPtrToAdViewClient( view );
                if (instance != null)
                {
                    if (instance.OnFailed != null)
                        instance.OnFailed( instance, ( AdError )error );

                    if (instance._waitOfHideCallback)
                    {
                        instance._waitOfHideCallback = false;
                        if (instance.OnHidden != null)
                            instance.OnHidden( instance );
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillOpeningWithMetaCallback ) )]
        private static void AdViewPresentedCallback( IntPtr view, int net, double cpm, int accuracy )
        {
            try
            {
                var instance = IntPtrToAdViewClient( view );
                if (instance != null)
                {
                    instance._waitOfHideCallback = true;
                    if (instance.OnPresented != null)
                        instance.OnPresented( instance,
                            new AdMetaData( AdType.Banner, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void AdViewClickedCallback( IntPtr view )
        {
            try
            {
                var instance = IntPtrToAdViewClient( view );
                if (instance != null && instance.OnClicked != null)
                    instance.OnClicked( instance );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
    }
}
#endif