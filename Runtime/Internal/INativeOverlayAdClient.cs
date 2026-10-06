using System;
using UnityEngine;

namespace CAS
{
    internal interface INativeOverlayAdClient : IDisposable
    {
        event Action OnAdLoaded;
        event Action<AdError> OnAdFailedToLoad;
        event Action<AdError> OnAdFailedToShow;
        event Action OnAdClicked;
        event Action<AdMetaData> OnAdImpression;
        event Action<Rect> OnAdRectChanged;

        bool isReady { get; }

        string placement { get; set; }

        Rect rectInPixels { get; }

        int widthInPixels { get; }

        int heightInPixels { get; }

        void Load();

        void Show();

        void Hide();

        void SetPosition(AdPosition position);

        void SetPosition(int x, int y, AdPosition position);

        void SetPositionPx(int x, int y, AdPosition position);

        void RenderTemplate(NativeTemplateStyle style, int widthDp, int heightDp);
    }
}