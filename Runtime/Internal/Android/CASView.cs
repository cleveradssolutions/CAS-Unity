//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal class CASView : IAdView
    {
        private readonly CASMediationManager _manager;
        private readonly AndroidJavaObject _bridge;
#pragma warning disable CS0414
        private AdEventsProxy _callbackProxy;
#pragma warning restore CS0414
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

        public IMediationManager manager { get { return _manager; } }
        public AdSize size { get; private set; }

        public AdPosition position
        {
            get { return _position; }
            set {  SetPosition( value, 0, 0 ); }
        }

        public Rect rectInPixels
        {
            get
            {
                var nativeRect = _bridge.Call<int[]>( "getRectInPixels" );
                return new Rect( nativeRect[0], nativeRect[1], nativeRect[2], nativeRect[3] );
            }
        }

        public bool isReady
        {
            get
            {
                return _bridge.Call<bool>( "isReady" );
            }
        }

        public int refreshInterval
        {
            get { return _refreshInterval; }
            set
            {
                if (_refreshInterval != value)
                {
                    _refreshInterval = value;
                    _bridge.Call( "setRefreshInterval", value );
                }
            }
        }

        internal CASView( CASMediationManager manager, AdSize size, AndroidJavaObject bridge, AdEventsProxy callback )
        {
            _manager = manager;
            this.size = size;
            _bridge = bridge;
            _callbackProxy = callback;

            callback.OnAdLoaded += CallbackOnLoaded;
            callback.OnAdFailed += CallbackOnFailed;
            callback.OnAdOpening += CallbackOnOpen;
            callback.OnAdClicked += CallbackOnClick;
        }

        public void Dispose()
        {
            _manager.CallbackOnDestroy( this );
            _bridge.Call( "destroy" );
        }

        public void DisableRefresh()
        {
            refreshInterval = 0;
        }

        public void Load()
        {
            _bridge.Call( "load" );
        }

        public void SetActive( bool active )
        {
            if (active)
            {
                _bridge.Call( "show" );
                return;
            }
            _bridge.Call( "hide" );
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
                _bridge.Call( "setPosition", ( int )position, x, y );
            }
        }

        #region Callbacks
        private void CallbackOnClick()
        {
            if (OnClicked != null)
                OnClicked( this );
        }

        private void CallbackOnOpen( AdMetaData meta )
        {
            _waitOfHideCallback = true;
            if (OnPresented != null)
                OnPresented( this, meta );
        }

        private void CallbackOnLoaded()
        {
            if (OnLoaded != null)
                OnLoaded( this );
        }

        private void CallbackOnFailed( AdError error )
        {
            if (OnFailed != null)
                OnFailed( this, error );

            if (_waitOfHideCallback)
            {
                _waitOfHideCallback = false;
                if (OnHidden != null)
                    OnHidden( this );
            }
        }
        #endregion
    }
}
#endif