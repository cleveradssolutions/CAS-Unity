//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using UnityEngine;

namespace CAS.Android
{
    internal class CASViewClient : AndroidJavaProxy, IAdView
    {
        private readonly CASManagerClient _manager;
        private readonly AndroidJavaObject _bridge;
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
            set { SetPosition(value, 0, 0); }
        }

        public bool isReady
        {
            get { return _bridge.Call<bool>("isReady"); }
        }

        public int refreshInterval
        {
            get { return _bridge.Call<int>("getRefreshInterval"); }
            set { _bridge.Call("setRefreshInterval", value); }
        }

        internal CASViewClient(CASManagerClient manager, AdSize size, AndroidJavaObject bridge)
            : base(CASJavaBridge.adViewCallbackClass)
        {
            this.size = size;
            _manager = manager;
            _bridge = new AndroidJavaObject(CASJavaBridge.adViewClass, (int)size, this, bridge);
        }

        public void Dispose()
        {
            _manager.RemoveAdViewFromFactory(this);
            _bridge.Call("destroy");
        }

        public void DisableRefresh()
        {
            refreshInterval = 0;
        }

        public void Load()
        {
            _bridge.Call("load");
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                _bridge.Call("show");
                return;
            }
            rectInPixels = Rect.zero;
            _bridge.Call("hide");
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
                _bridge.Call("setPosition", (int)position, x, y);
            }
        }

        #region Android Native callbacks
        public void onAdViewLoaded()
        {
            CASFactory.UnityLog("Callback Loaded " + size.ToString());
            if (OnLoaded == null) return;
            CASJavaBridge.ExecuteEvent(HandleAdLoaded);
        }

        public void onAdViewFailed(int error)
        {
            CASFactory.UnityLog("Callback Failed " + size.ToString());
            if (OnFailed == null) return;
            CASJavaBridge.ExecuteEvent(HandleAdFailed, error);
        }

        public void onAdViewImpression(AndroidJavaObject impression)
        {
            CASFactory.UnityLog("Callback Impression " + size.ToString());
            if (OnImpression == null) return;
            CASJavaBridge.ExecuteEvent(HandleAdImpression, AdType.Banner, impression);
        }

        public void onAdViewClicked()
        {
            CASFactory.UnityLog("Callback Clicked " + size.ToString());
            if (OnClicked == null) return;
            CASJavaBridge.ExecuteEvent(HandleAdClicked);
        }

        public void onAdViewRect(int x, int y, int width, int height)
        {
            rectInPixels = new Rect(x, y, width, height);
        }
        #endregion

        #region Unity thread callbacks
        private void HandleAdLoaded()
        {
            if (OnLoaded != null)
                OnLoaded(this);
        }

        private void HandleAdFailed(AdError error)
        {
            if (OnFailed != null)
                OnFailed(this, error);
        }
        private void HandleAdImpression(AdMetaData impression)
        {
            if (OnImpression != null)
                OnImpression(this, impression);
        }
        private void HandleAdClicked()
        {
            if (OnClicked != null)
                OnClicked(this);
        }
        #endregion
    }
}
#endif