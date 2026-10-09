//  Copyright � 2026 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal sealed class CASNativeOverlayClient :INativeOverlayAd, CASCallback.Handler, CASCallback.RectHandler
    {
        private readonly CASManagerBase _manager;
        private readonly CASCallback _callback;

        private AndroidJavaObject _bridge;

        private string _placement;
        private Rect _rectInPixels;

        internal CASNativeOverlayClient(CASManagerBase manager)
        {
            _manager = manager;
            _callback = new CASCallback(this);

            _bridge = new AndroidJavaObject(
                CASJavaBridge.NativeOverlayClass,
                _callback,
                manager.managerID
            );
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
                return _bridge != null &&
                       _bridge.Call<bool>("isReady");
            }
        }

        public string placement
        {
            get => _placement;

            set
            {
                _placement = value;

                if (_bridge != null)
                    _bridge.Call("setPlacement", value);
            }
        }

        public Rect rectInPixels => _rectInPixels;

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
                _bridge.Call("setPosition", (int)position, x, y);
        }

        public void SetPositionPx(int x, int y, AdPosition position)
        {
            if (_bridge != null)
                _bridge.Call("setPositionPx", (int)position, x, y);
        }

        public void RenderTemplate(NativeTemplateStyle style,int widthDp,int heightDp)
        {
            if (_bridge == null)
                return;

            string json = style == null? null: JsonUtility.ToJson(ToBridgeStyle(style));

            _bridge.Call("renderTemplate", json, widthDp, heightDp);
        }

        public void HandleCallback(int action,int type,int error,string errorMessage,object impression)
        {
            if (type != AdTypeCode.NATIVE)
                return;

            switch (action)
            {
                case AdActionCode.LOADED:
                    OnAdLoaded?.Invoke();
                    break;

                case AdActionCode.FAILED:
                    OnAdFailedToLoad?.Invoke(new AdError(error, errorMessage));
                    break;

                case AdActionCode.SHOW_FAILED:
                    OnAdFailedToShow?.Invoke(new AdError(error, errorMessage));
                    break;

                case AdActionCode.CLICKED:
                    OnAdClicked?.Invoke();
                    break;

                case AdActionCode.IMPRESSION:
                    OnAdImpression?.Invoke(_manager.WrapImpression(AdType.Native, impression));
                    break;
            }
        }

        public void HandleRect(int x,int y,int width,int height)
        {
            _rectInPixels = new Rect(x, y, width, height);

            OnAdRectChanged?.Invoke(_rectInPixels);
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

        private static BridgeTemplateStyle ToBridgeStyle(
            NativeTemplateStyle style)
        {
            return new BridgeTemplateStyle
            {
                mainBackgroundColor =ToAndroidColor(style.MainBackgroundColor),

                headline = ToBridge(style.Headline),
                body = ToBridge(style.Body),
                advertiser = ToBridge(style.Advertiser),
                callToAction = ToBridge(style.CallToAction),
                store = ToBridge(style.Store),
                price = ToBridge(style.Price),
                reviewCount = ToBridge(style.ReviewCount),
                adLabel = ToBridge(style.AdLabel)
            };
        }

        private static BridgeTextStyle ToBridge(
            NativeTemplateTextStyle style)
        {
            if (style == null)
                return null;

            return new BridgeTextStyle
            {
                backgroundColor = ToAndroidColor(style.BackgroundColor),
                textColor = ToAndroidColor(style.FontColor),
                fontSize = style.FontSize,
                fontStyle = (int)style.Style
            };
        }

        private static int ToAndroidColor(Color color)
        {
            Color32 value = color;
            uint argb =((uint)value.a << 24) | ((uint)value.r << 16) | ((uint)value.g << 8) | value.b;

            return unchecked((int)argb);
        }

        [Serializable]
        private sealed class BridgeTemplateStyle
        {
            public int mainBackgroundColor;

            public BridgeTextStyle headline;
            public BridgeTextStyle body;
            public BridgeTextStyle advertiser;
            public BridgeTextStyle callToAction;
            public BridgeTextStyle store;
            public BridgeTextStyle price;
            public BridgeTextStyle reviewCount;
            public BridgeTextStyle adLabel;
        }

        [Serializable]
        private sealed class BridgeTextStyle
        {
            public int backgroundColor;
            public int textColor;

            public float fontSize;
            public int fontStyle;
        }
    }
}
#endif