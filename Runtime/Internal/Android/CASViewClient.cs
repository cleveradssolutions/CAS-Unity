//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using UnityEngine;

namespace CAS.Android
{
    internal class CASViewClient : IAdView
    {
        private readonly CASManagerClient _manager;
        private readonly AndroidJavaObject _bridge;
#pragma warning disable CS0414
        private AdEventsProxy _callbackProxy;
#pragma warning restore CS0414
        private AdPosition _position = AdPosition.BottomCenter;
        private int _positionX = 0;
        private int _positionY = 0;

        public event CASViewEvent OnLoaded;
        public event CASViewEventWithError OnFailed;
        public event CASViewEventWithMeta OnImpression;
        public event CASViewEvent OnClicked;

        public IMediationManager manager { get { return _manager; } }
        public AdSize size { get; private set; }
        public Rect rectInPixels { get; private set; }

        public AdPosition position
        {
            get { return _position; }
            set { SetPosition( value, 0, 0 ); }
        }

        public bool isReady
        {
            get { return _bridge.Call<bool>( "isReady" ); }
        }

        public int refreshInterval
        {
            get { return _bridge.Call<int>( "getRefreshInterval" ); }
            set { _bridge.Call( "setRefreshInterval", value ); }
        }

        internal CASViewClient( CASManagerClient manager, AdSize size, AndroidJavaObject bridge, AdEventsProxy callback )
        {
            _manager = manager;
            this.size = size;
            _bridge = bridge;
            _callbackProxy = callback;

            callback.OnAdLoaded += CallbackOnLoaded;
            callback.OnAdFailed += CallbackOnFailed;
            callback.OnAdOpening += CallbackOnOpen;
            callback.OnAdClicked += CallbackOnClick;
            callback.OnAdRect += CallbackOnRect;
        }

        public void Dispose()
        {
            _manager.RemoveAdViewFromFactory( this );
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
            rectInPixels = Rect.zero;
            _bridge.Call( "hide" );
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
                _bridge.Call( "setPosition", (int)position, x, y );
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
            if (OnImpression != null)
                OnImpression( this, meta );
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
        }

        private void CallbackOnRect( Rect rect )
        {
            rectInPixels = rect;
        }
        #endregion
    }
}
#endif