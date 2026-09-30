//  Copyright © 2026 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace CAS.Android
{
    internal sealed class CASNativeOverlayClient :AndroidJavaProxy, INativeOverlayAdClient
    {
        private readonly CASManagerBase _manager;

        private AndroidJavaObject _bridge;

        private string _placement;
        private Rect _rectInPixels;

        internal CASNativeOverlayClient(CASManagerBase manager) : base(CASJavaBridge.NativeOverlayCallbackClass)
        {
            _manager = manager;

            _bridge = new AndroidJavaObject(CASJavaBridge.NativeOverlayClass,this,manager.managerID);
        }

        public event Action OnAdLoaded;
        public event Action<AdError> OnAdFailedToLoad;
        public event Action<AdError> OnAdFailedToShow;
        public event Action OnAdClicked;
        public event Action<AdMetaData> OnAdImpression;
        public event Action<Rect> OnAdRectChanged;


        public bool isReady
        {
            get
            {
                return _bridge != null && _bridge.Call<bool>("isReady");
            }
        }


        public string placement
        {
            get
            {
                return _placement;
            }
            set
            {
                _placement = value;

                if (_bridge != null)
                    _bridge.Call("setPlacement", value);
            }
        }


        public Rect rectInPixels
        {
            get
            {
                return _rectInPixels;
            }
        }


        public int widthInPixels
        {
            get
            {
                if (_bridge == null)
                    return 0;

                return _bridge.Call<int>("getWidthInPixels");
            }
        }


        public int heightInPixels
        {
            get
            {
                if (_bridge == null)
                    return 0;

                return _bridge.Call<int>("getHeightInPixels");
            }
        }


        public void Load()
        {
            if (_bridge != null)
                _bridge.Call("load");
        }


        public void Show()
        {
            if (_bridge != null)
                _bridge.Call("show");
        }


        public void Hide()
        {
            if (_bridge != null)
                _bridge.Call("hide");
        }


        public void SetPosition(AdPosition position)
        {
            if (_bridge != null)
                _bridge.Call("setPosition", (int)position);
        }


        public void SetPosition(int x, int y, AdPosition position)
        {
            if (_bridge != null)
            {
                _bridge.Call("setPosition", (int)position, x, y);
            }
        }


        public void SetPositionPx(int x, int y, AdPosition position)
        {
            if (_bridge != null)
            {
                _bridge.Call("setPositionPx", (int)position, x, y);
            }
        }


        public void Render(int widthDp, int heightDp)
        {
            if (_bridge != null)
            {
                _bridge.Call("render", widthDp, heightDp);
            }
        }


        public void RenderDefault()
        {
            if (_bridge != null)
                _bridge.Call("renderDefault");
        }


        public void SetBackgroundColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setBackgroundColor", ToAndroidColor(color));
            }
        }


        public void SetHeadlineColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setHeadlineColor", ToAndroidColor(color));
            }
        }


        public void SetBodyColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setBodyColor", ToAndroidColor(color));
            }
        }


        public void SetAdvertiserColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setAdvertiserColor", ToAndroidColor(color));
            }
        }


        public void SetCallToActionTextColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setCallToActionTextColor", ToAndroidColor(color));
            }
        }


        public void SetCallToActionBackgroundColor(Color color)
        {
            if (_bridge != null)
            {
                _bridge.Call("setCallToActionBackgroundColor", ToAndroidColor(color));
            }
        }


        public void Dispose()
        {
            if (_bridge == null)
                return;

            try
            {
                _bridge.Call("destroy");
            }
            finally
            {
                _bridge.Dispose();
                _bridge = null;
            }
        }


        private static int ToAndroidColor(Color color)
        {
            Color32 value = color;

            uint argb =((uint)value.a << 24) |((uint)value.r << 16) |((uint)value.g << 8) |value.b;

            return unchecked((int)argb);
        }

        public void onNativeAdLoaded()
        {
            CASJavaBridge.ExecuteEvent(() => OnAdLoaded?.Invoke());
        }

        public void onNativeAdFailedToLoad(int code, string message)
        {
            CASJavaBridge.ExecuteEvent(() => OnAdFailedToLoad?.Invoke(new AdError(code, message)));
        }

        public void onNativeAdFailedToShow(int code, string message)
        {
            CASJavaBridge.ExecuteEvent(() => OnAdFailedToShow?.Invoke(new AdError(code, message)));
        }

        public void onNativeAdClicked()
        {
            CASJavaBridge.ExecuteEvent(() => OnAdClicked?.Invoke());
        }


        public void onNativeAdImpression(AndroidJavaObject impression)
        {
            CASJavaBridge.ExecuteEvent(() => {
                OnAdImpression?.Invoke(_manager.WrapImpression(AdType.Native, impression));
            });
        }

        [Preserve]
        public void onNativeAdRect(int x, int y, int width, int height)
        {
            CASJavaBridge.ExecuteEvent(() => {
                _rectInPixels = new Rect(x, y, width, height);
                OnAdRectChanged?.Invoke(_rectInPixels);
            });
        }

    }
}

#endif