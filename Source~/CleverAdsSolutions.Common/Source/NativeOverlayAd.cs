//  Copyright © 2026 CAS.AI. All rights reserved.

using System;
using UnityEngine;

namespace CAS
{
    public interface INativeOverlayAd : IDisposable
    {
        /// <summary>
        /// Called when the ad impression detects paid revenue. 
        /// </summary>
        event CASEventWithMeta OnImpression;
        /// <summary>
        /// Сalled when an error occurred with the ad.
        /// </summary>
        event CASEventWithError OnFailedToShow;
        /// <summary>
        /// Called when the user clicks on the Ad.
        /// </summary>
        event Action OnClicked;

        /// <summary>
        /// Get the manager of the AdOverlay.
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

        /// <summary>Returns metadata of the loaded ad or null when unavailable or expired.</summary>
        AdMetaData adContentInfo { get; }

        /// <summary>
        /// Get the real AdOverlay rect with position and size in pixels on screen.
        /// <para>The position on the screen is calculated with the addition of indents for the cutouts.</para>
        /// </summary>
        Rect rectInPixels { get; }

        /// <summary>
        /// Renders the native overlay ad with provided style and the given size in DP.
        /// </summary>
        /// <param name="style">Native template style.</param>
        /// <param name="widthDp">Overlay width in DP.</param>
        /// <param name="heightDp">Overlay height in DP.</param>
        void RenderTemplate(int widthDp, int heightDp, NativeTemplateStyle style = null);

        /// <summary>
        /// The AdOverlay will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// </summary>
        /// <param name="position">The corner of the screen.</param>
        /// <param name="xOffsetDP">X-coordinate on screen in DP.</param>
        /// <param name="yOffsetDP">Y-coordinate on screen in DP.</param>
        void SetPosition(AdPosition position, int xOffsetDP = 0, int yOffsetDP = 0);

        /// <summary>
        /// The AdOverlay will be positioned at the X and Y values passed to the method,
        /// where the origin is the selected <see cref="AdPosition"/> corner of the screen.
        /// <para>The coordinates on the screen are determined in pixels.</para>
        /// </summary>
        /// <param name="position">The corner of the screen.</param>
        /// <param name="xOffsetPX">X-coordinate on screen in pixels.</param>
        /// <param name="yOffsetPX">Y-coordinate on screen in pixels.</param>
        void SetPositionPx(AdPosition position, int xOffsetPX, int yOffsetPX);

        /// <summary>
        /// Activate or deactivate the AdOverlay
        /// <para>If the AdOverlay is active, it is displayed on the screen and user can interact with it.</para>
        /// <para>When you need to hide AdOverlay from the user, just deactivate it.</para>
        /// </summary>
        void SetActive(bool active);
    }
}