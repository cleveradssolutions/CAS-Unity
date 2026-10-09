using System;
using UnityEngine;

namespace CAS
{
    public interface INativeOverlayAd : IDisposable
    {
        /// <summary>
        /// Called when the ad impression detects paid revenue. 
        /// </summary>
        event Action<AdMetaData> OnImpression;
        /// <summary>
        /// Сalled when an error occurred with the ad.
        /// </summary>
        event Action<AdError> OnFailedToShow;
        /// <summary>
        /// Called when the user clicks on the Ad.
        /// </summary>
        event Action OnClicked;

        event Action<Rect> OnRectChanged;

        /// <summary>
        /// Get the manager of the AdView.
        /// </summary>
        IMediationManager manager { get; }

        /// <summary>
        /// Indicates whether the ad has expired.
        ///
        /// This property returns `true` if the ad is no longer valid or has surpassed its
        /// intended display duration. An expired ad may not be shown to users, and interactions
        /// with it may be restricted or disabled.
        /// </summary>
        bool isExpired { get; }

        /// <summary>
        /// Get the real AdView rect with position and size in pixels on screen.
        /// <para>The position on the screen is calculated with the addition of indents for the cutouts.</para>
        /// </summary>
        Rect rectInPixels { get; }

        /// <summary>
        /// Renders the native overlay ad with provided style and the given size in DP.
        /// </summary>
        /// <param name="style">Native template style.</param>
        /// <param name="widthDp">Overlay width in DP.</param>
        /// <param name="heightDp">Overlay height in DP.</param>
        void RenderTemplate(NativeTemplateStyle style = null, int widthDp = 0, int heightDp = 0);

        /// <summary>
        /// The position where the AdView ad should be placed.
        /// The <see cref="AdPosition"/> enum lists the valid ad position values.
        /// <para>Default: <see cref="AdPosition.BottomCenter"/></para>
        /// </summary>
        void SetPosition(AdPosition position);

        /// <summary>
        /// The AdView will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// </summary>
        /// <param name="xDp">X-coordinate on screen in DP.</param>
        /// <param name="yDp">Y-coordinate on screen in DP.</param>
        /// <param name="position">The corner of the screen.</param>
        void SetPosition(int xDp, int yDp, AdPosition position = AdPosition.TopLeft);

        /// <summary>
        /// The AdView will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined in pixels.</para>
        /// </summary>
        /// <param name="xPx">X-coordinate on screen in pixels.</param>
        /// <param name="yPx">Y-coordinate on screen in pixels.</param>
        /// <param name="position">The corner of the screen.</param>
        void SetPositionPx(int xPx, int yPx, AdPosition position = AdPosition.TopLeft);

        /// <summary>
        /// Activate or deactivate the AdView
        /// <para>If the AdView is active, it is displayed on the screen and user can interact with it.</para>
        /// <para>When you need to hide AdView from the user, just deactivate it.</para>
        /// </summary>
        void SetActive(bool active);
    }
}