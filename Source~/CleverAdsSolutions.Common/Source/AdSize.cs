//  Copyright © 2025 CAS.AI. All rights reserved.

namespace CAS
{
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Banner-Ads#ad-size" )]
    public enum AdSize
    {
        // 0 Index reserved for internal logic

        /// <summary>
        /// Standard banner size 320dp width and 50dp height
        /// </summary>
        Banner = 1,
        /// <summary>
        /// Pick Adaptive AdSize for screen width but not more than 728dp.
        /// <para>Pick the best ad size, adaptive banners use fixed aspect ratios instead of fixed heights.</para>
        /// <para>The height of adaptive banners cannot be less than 50 dp and more than 250 dp.</para>
        /// <para>This results in banner ads that occupy a more consistent portion
        /// of the screen across devices and provide opportunities for improved performance.</para>
        /// <para>For full width banners, you can use the <see cref="AdaptiveFullWidth"/>.</para>
        /// </summary>
        AdaptiveBanner = 2,
        /// <summary>
        /// Typically, Smart Banners on phones have a <see cref="Banner"/> size.
        /// Or on tablets a <see cref="Leaderboard"/> size.
        /// </summary>
        SmartBanner = 3,
        /// <summary>
        /// Leaderboard banner size 728dp width and 90dp height
        /// </summary>
        Leaderboard = 4,
        /// <summary>
        /// Medium Rectangle size 300dp width and 250dp height
        /// </summary>
        MediumRectangle = 5,
        /// <summary>
        /// Pick Adaptive AdSize for full screen width.
        /// <para>Pick the best ad size, adaptive banners use fixed aspect ratios instead of fixed heights.</para>
        /// <para>The height of adaptive banners cannot be less than 50 dp and more than 250 dp.</para>
        /// <para>This results in banner ads that occupy a more consistent portion
        /// of the screen across devices and provide opportunities for improved performance.</para>
        /// </summary>
        AdaptiveFullWidth = 6,
        /// <summary>
        /// Thin banners have a smaller height, taller banners compared to anchored adaptive banners.
        /// The thin banner uses the full available screen width.
        /// <para>Pick the best ad size in full width of screen and height 32-50 for Landscape and 50-90 for Portrait orientations.</para>
        /// <para>The height of thin banners can be 32 dp for landscape orientation.</para>
        /// </summary>
        ThinBanner = 7
    }
}
