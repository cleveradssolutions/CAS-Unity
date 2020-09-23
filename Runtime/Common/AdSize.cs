namespace CAS
{
    public enum AdSize
    {
        // 0 Index reserved for internal logic

        /// <summary>
        /// Standard banner size 320dp width and 50dp height
        /// </summary>
        Banner = 1,
        /// <summary>
        /// Create Adaptive AdSize placed in container for current screen orientation.
        /// Container height cannot be less than 50dp.
        ///
        /// Pick the best ad size, adaptive banners use fixed aspect ratios instead of fixed heights.
        /// This results in banner ads that occupy a more consistent portion of the screen across devices and provide opportunities for improved performance.
        ///
        /// SeeAlso: [Google Adaptive Banner](https://developers.google.com/admob/ios/banner/adaptive)
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
        MediumRectangle = 5
    }
}
