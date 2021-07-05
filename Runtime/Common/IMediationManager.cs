
using System;

namespace CAS
{
    public delegate void CASTypedEvent( AdType adType );
    public delegate void CASTypedEventWithError( AdType adType, string error );
    public delegate void CASEventWithError( string error );
    public delegate void CASEventWithMeta( AdMetaData meta );

    public interface IMediationManager
    {
        #region Events
        /// <summary>
        /// Called when <see cref="AdType"/> load ad response
        /// </summary>
        event CASTypedEvent OnLoadedAd;
        /// <summary>
        /// Called when <see cref="AdType"/> failed to load ad response with error message
        /// </summary>
        event CASTypedEventWithError OnFailedToLoadAd;

        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnBannerAdShown;
        /// <summary>
        /// The same call as the <see cref="OnBannerAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        event CASEventWithMeta OnBannerAdOpening;
        /// <summary>
        /// Called when the ad is failed to display.
        /// The Banner may automatically appear when the Ad is ready again.
        /// This will trigger the <see cref="OnBannerAdShown"/> callback again.
        /// </summary>
        event CASEventWithError OnBannerAdFailedToShow;
        /// <summary>
        /// Called when the user clicks on an Ad.
        /// </summary>
        event Action OnBannerAdClicked;
        /// <summary>
        /// Called when the ad is hidden from screen.
        /// </summary>
        event Action OnBannerAdHidden;

        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnInterstitialAdShown;
        /// <summary>
        /// The same call as the <see cref="OnInterstitialAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        event CASEventWithMeta OnInterstitialAdOpening;
        /// <summary>
        /// Called when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnInterstitialAdFailedToShow;
        /// <summary>
        /// Called when the user clicks on an Ad.
        /// </summary>
        event Action OnInterstitialAdClicked;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnInterstitialAdClosed;

        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnRewardedAdShown;
        /// <summary>
        /// The same call as the <see cref="OnRewardedAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        event CASEventWithMeta OnRewardedAdOpening;
        /// <summary>
        /// Called when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnRewardedAdFailedToShow;
        /// <summary>
        /// Called when the user clicks on an Ad.
        /// </summary>
        event Action OnRewardedAdClicked;
        /// <summary>
        /// Called when the ad is completed.
        /// </summary>
        event Action OnRewardedAdCompleted;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnRewardedAdClosed;

        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnAppReturnAdShown;
        /// <summary>
        /// The same call as the <see cref="OnAppReturnAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        event CASEventWithMeta OnAppReturnAdOpening;
        /// <summary>
        /// Called when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnAppReturnAdFailedToShow;
        /// <summary>
        /// Called when the user clicks on an Ad.
        /// </summary>
        event Action OnAppReturnAdClicked;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnAppReturnAdClosed;
        #endregion

        #region Mediation manager state
        /// <summary>
        /// CAS manager (Placement) identifier
        /// </summary>
        string managerID { get; }

        /// <summary>
        /// Is Mediation manager use test ads for current session.
        /// </summary>
        bool isTestAdMode { get; }

        /// <summary>
        /// The latest free ad page for your own promotion.
        /// This ad page will be displayed when there is no paid ad to show or internet availability.
        ///
        /// **Attention!** Impressions and clicks on this ad page don't make money.
        ///
        /// By default, this page will not be displayed while the ad content is NULL.
        /// </summary>
        LastPageAdContent lastPageAdContent { get; set; }

        /// <summary>
        /// Get last active mediation ad name of selected <see cref="AdType"/>.
        /// Can return Empty Sting.
        /// </summary>
        string GetLastActiveMediation( AdType adType );

        /// <summary>
        /// Check selected <see cref="AdType"/> is processing.
        /// </summary>
        bool IsEnabledAd( AdType adType );

        /// <summary>
        /// Set enabled <see cref="AdType"/> to processing.
        /// The state will not be saved between sessions.
        /// </summary>
        void SetEnableAd( AdType adType, bool enabled );
        #endregion

        /// <summary>
        /// Manual load <see cref="AdType"/> Ad.
        /// For <see cref="AdType.Banner"/> can be reloaded for any active <see cref="LoadingManagerMode"/>.
        /// For other Ad types allowed only with <see cref="LoadingManagerMode.Manual"/>.
        /// Please call load before each show ad.
        /// You can get a callback for the successful loading of an ad by subscribe <see cref="OnLoadedAd"/>.
        /// </summary>
        void LoadAd( AdType adType );

        /// <summary>
        /// Check ready selected <see cref="AdType"/> to show.
        /// </summary>
        bool IsReadyAd( AdType adType );

        /// <summary>
        /// Force show ad by selected <see cref="AdType"/>.
        /// </summary>
        void ShowAd( AdType adType );

        #region Banner Ad
        /// <summary>
        /// The size of the Banner Ad.
        /// We recommended set once immediately after <see cref="MobileAds.Initialize(string, AdFlags, bool, InitCompleteAction)"/>.
        /// If active <see cref="LoadingManagerMode.Manual"/>
        /// then please call <see cref="LoadAd(AdType)"/> each banner size changed.
        /// </summary>
        AdSize bannerSize { get; set; }

        /// <summary>
        /// The position of the Banner Ad using <see cref="AdPosition"/>.
        /// </summary>
        AdPosition bannerPosition { get; set; }

        /// <summary>
        /// Hide banner from screen. Call <see cref="ShowAd(AdType)"/> for <see cref="AdType.Banner"/> to show again.
        /// </summary>
        void HideBanner();

        /// <summary>
        /// The Banner Ad Height in pixels of current <see cref="AdSize"/>
        /// </summary>
        float GetBannerHeightInPixels();

        /// <summary>
        /// The Banner Ad Width in pixels of current <see cref="AdSize"/>
        /// </summary>
        float GetBannerWidthInPixels();
        #endregion

        /// <summary>
        /// The Return Ad which is displayed once the user returns to your application after a certain period of time.
        /// To minimize the intrusiveness, short time periods are ignored.
        /// Return ads are disabled by default.
        /// </summary>
        void SetAppReturnAdsEnabled( bool enable );

        /// <summary>
        /// Calling this method will indicate to skip one next ad impression when returning to the app.
        ///
        /// You can call this method when you intentionally redirect the user to another application (for example Google Play)
        /// and do not want them to see ads when they return to your application.
        /// </summary>
        void SkipNextAppReturnAds();
    }
}
