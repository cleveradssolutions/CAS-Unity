//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using UnityEngine;

namespace CAS
{
    public delegate void CASViewEvent( IAdView view );
    public delegate void CASViewEventWithError( IAdView view, AdError error );
    public delegate void CASViewEventWithMeta( IAdView view, AdMetaData data );

    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Banner-Ads" )]
    public interface IAdView : IDisposable
    {
        /// <summary>
        /// Called when the ad loaded and ready to present.
        /// </summary>
        event CASViewEvent OnLoaded;
        /// <summary>
        /// Сalled when an error occurred with the ad.
        /// </summary>
        event CASViewEventWithError OnFailed;
        /// <summary>
        /// Called when the ad view did present for user with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        event CASViewEventWithMeta OnPresented;
        /// <summary>
        /// Called when the user clicks on the Ad.
        /// </summary>
        event CASViewEvent OnClicked;
        /// <summary>
        /// Called when the ad view did hidden from user.
        /// </summary>
        event CASViewEvent OnHidden;

        /// <summary>
        /// Get the manager of the AdView.
        /// </summary>
        IMediationManager manager { get; }

        /// <summary>
        /// Current ad size.
        /// <para>You cannot resize ad, but instead get the new <see cref="IAdView"/> for the size you want
        /// using the <see cref="IMediationManager.GetAdView(AdSize)"/> method.</para>
        /// </summary>
        AdSize size { get; }

        /// <summary>
        /// Get the real AdView rect with position and size in pixels on screen.
        /// <para>Return <see cref="Rect.zero"/> when ad view is not active.</para>
        /// <para>The position on the screen is calculated with the addition of indents for the cutouts.</para>
        /// </summary>
        Rect rectInPixels { get; }

        /// <summary>
        /// Check ready ads to present
        /// </summary>
        bool isReady { get; }

        /// <summary>
        /// Set the number of seconds an ad is displayed before a new ad is shown.
        /// After the interval has passed, a new advertisement will be automatically loaded.
        /// <para>This value will override global <see cref="IAdsSettings.bannerRefreshInterval"/></para>
        /// </summary>
        int refreshInterval { get; set; }

        /// <summary>
        /// Disable auto refresh ads.
        /// </summary>
        void DisableRefresh();

        /// <summary>
        /// The position where the AdView ad should be placed.
        /// The <see cref="AdPosition"/> enum lists the valid ad position values.
        /// <para>Default: <see cref="AdPosition.BottomCenter"/></para>
        /// </summary>
        AdPosition position { get; set; }

        /// <summary>
        /// For greater control over where a AdView is placed on screen than what's offered by <see cref="AdPosition"/> values.
        /// <para>The top-left corner of the AdView will be positioned
        /// at the <paramref name="x"/> and <paramref name="y"/> values passed to the method,
        /// where the origin is the top-left of the screen.</para>
        /// <para>The coordinates on the screen are determined not in pixels, but in Density-independent Pixels(DP)!</para>
        /// <para>Screen positioning coordinates are only available for the <see cref="AdPosition.TopLeft"/>.</para>
        /// </summary>
        /// <param name="x">X-coordinate on screen in DP.</param>
        /// <param name="y">Y-coordinate on screen in DP.</param>
        void SetPosition( int x, int y );

        /// <summary>
        /// Manual load the Ad or reload current loaded Ad to skip impression.
        /// <para>You can get a callback for the successful loading of an ad by subscribe to <see cref="OnLoaded"/>.</para>
        /// </summary>
        void Load();

        /// <summary>
        /// Activate or deactivate the AdView
        /// <para>If the AdView is active, it is displayed on the screen and user can interact with it.</para>
        /// <para>When you need to hide AdView from the user, just deactivate it.</para>
        /// </summary>
        void SetActive( bool active );
    }
}
