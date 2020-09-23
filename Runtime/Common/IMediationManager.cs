
using System;

namespace CAS
{
    public delegate void CASTypedEvent( AdType adType );
    public delegate void CASTypedEventWithError( AdType adType, string error );
    public delegate void CASEventWithError( string error );

    public interface IMediationManager
    {
        #region Events
        /// <summary>
        /// Executed when <see cref="AdType"/> load ad response
        /// </summary>
        event CASTypedEvent OnLoadedAd;
        /// <summary>
        /// Executed when <see cref="AdType"/> failed to load ad response with error message
        /// </summary>
        event CASTypedEventWithError OnFailedToLoadAd;

        /// <summary>
        /// Executed when the ad is displayed.
        /// </summary>
        event Action OnBannerAdShown;
        /// <summary>
        /// Executed when the ad is failed to display.
        /// The Banner may automatically appear when the Ad is ready again.
        /// This will trigger the <see cref="OnBannerAdShown"/> callback again.
        /// </summary>
        event CASEventWithError OnBannerAdFailedToShow;
        /// <summary>
        /// Executed when the user clicks on an Ad.
        /// </summary>
        event Action OnBannerAdClicked;
        /// <summary>
        /// Executed when the ad is hidden from screen.
        /// </summary>
        event Action OnBannerAdHidden;

        /// <summary>
        /// Executed when the ad is displayed.
        /// </summary>
        event Action OnInterstitialAdShown;
        /// <summary>
        /// Executed when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnInterstitialAdFailedToShow;
        /// <summary>
        /// Executed when the user clicks on an Ad.
        /// </summary>
        event Action OnInterstitialAdClicked;
        /// <summary>
        /// Executed when the ad is closed.
        /// </summary>
        event Action OnInterstitialAdClosed;

        /// <summary>
        /// Executed when the ad is displayed.
        /// </summary>
        event Action OnRewardedAdShown;
        /// <summary>
        /// Executed when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnRewardedAdFailedToShow;
        /// <summary>
        /// Executed when the user clicks on an Ad.
        /// </summary>
        event Action OnRewardedAdClicked;
        /// <summary>
        /// Executed when the ad is completed.
        /// </summary>
        event Action OnRewardedAdCompleted;
        /// <summary>
        /// Executed when the ad is closed.
        /// </summary>
        event Action OnRewardedAdClosed;
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
        /// Get last active mediation ad name of selected [type].
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

        /// <summary>
        /// Hide banner from screen. Call <see cref="ShowAd(AdType)"/> for <see cref="AdType.Banner"/> to show again.
        /// </summary>
        void HideBanner();
    }
}
