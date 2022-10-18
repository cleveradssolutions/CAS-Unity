//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;

namespace CAS
{
    public delegate void CASTypedEvent( AdType adType );
    public delegate void CASTypedEventWithError( AdType adType, string error );
    public delegate void CASEventWithError( string error );
    public delegate void CASEventWithAdError( AdError error );
    public delegate void CASEventWithMeta( AdMetaData meta );

    internal class WikiPageAttribute : Attribute
    {
        internal WikiPageAttribute( string url ) { }
    }

    /// <summary>
    /// Interface for managing CAS mediation.
    /// Get instance using the <see cref="MobileAds.BuildManager"/> builder.
    /// </summary>
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-SDK" )]
    public interface IMediationManager : ISingleBannerManager
    {
        /// <summary>
        /// Called when <see cref="AdType"/> load ad response
        /// </summary>
        [Obsolete( "Use OnInterstitialAdLoaded, OnRewardedAdLoaded, IAdView.OnLoaded instead." )]
        event CASTypedEvent OnLoadedAd;
        /// <summary>
        /// Called when <see cref="AdType"/> failed to load ad response with error message
        /// </summary>
        [Obsolete( "Use OnInterstitialAdFailedToLoad, OnRewardedAdFailedToLoad, IAdView.OnFailed instead." )]
        event CASTypedEventWithError OnFailedToLoadAd;

        #region Interstitial Ads events
        /// <summary>
        /// Called when Interstitial ad ready to shown.
        /// </summary>
        event Action OnInterstitialAdLoaded;
        /// <summary>
        /// Called when Interstitial failed to load ad response with error message
        /// </summary>
        event CASEventWithAdError OnInterstitialAdFailedToLoad;
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
        /// Called when the user clicks on the Ad.
        /// </summary>
        event Action OnInterstitialAdClicked;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnInterstitialAdClosed;
        #endregion

        #region Rewarded Ads events
        /// <summary>
        /// Called when Rewarded video ad ready to shown.
        /// </summary>
        event Action OnRewardedAdLoaded;
        /// <summary>
        /// Called when Rewarded video failed to load ad response with error message
        /// </summary>
        event CASEventWithAdError OnRewardedAdFailedToLoad;
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
        /// Called when the user clicks on the Ad.
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
        #endregion

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
        /// <br>This ad page will be displayed when there is no paid ad to show or internet availability.</br>
        /// <para>**Attention!** Impressions and clicks on this ad page don't make money.</para>
        /// <para>By default, this page will not be displayed while the ad content is NULL.</para>
        /// </summary>
        LastPageAdContent lastPageAdContent { get; set; }

        /// <summary>
        /// Check selected <see cref="AdType"/> is processing.
        /// </summary>
        bool IsEnabledAd( AdType adType );

        /// <summary>
        /// Set enabled <see cref="AdType"/> to processing.
        /// <para>If processing is inactive then all calls to the selected ad type
        /// will fail with error <see cref="AdError.ManagerIsDisabled"/>.</para>
        /// <para>The state will not be saved between sessions.</para>
        /// </summary>
        void SetEnableAd( AdType adType, bool enabled );

        /// <summary>
        /// Manual load <see cref="AdType"/> Ad.
        /// <para>Please call load before each show ad whe active load mode is <see cref="LoadingManagerMode.Manual"/>.</para>
        /// <para>You can get a callback for the successful loading of an ad by subscribe <see cref="OnLoadedAd"/>.</para>
        /// <para>Please for <see cref="AdType.Banner"/> use new ad size api <see cref="GetAdView(AdSize)"/>.Load() instead.</para>
        /// </summary>
        void LoadAd( AdType adType );

        /// <summary>
        /// Check ready selected <see cref="AdType"/> to show.
        /// <para>Please for <see cref="AdType.Banner"/> use new ad size api <see cref="GetAdView(AdSize)"/>.isReady instead.</para>
        /// </summary>
        bool IsReadyAd( AdType adType );

        /// <summary>
        /// Force show ad by selected <see cref="AdType"/>.
        /// <para>Please for <see cref="AdType.Banner"/> use new ad size api <see cref="GetAdView(AdSize)"/>.SetActive(true) instead.</para>
        /// </summary>
        void ShowAd( AdType adType );

        /// <summary>
        /// Get the ad view interface for specific <paramref name="size"/>.
        /// <para>If a view for specific size has already been created then a reference to it
        /// will be returned without creating a new one.</para>
        /// <para>The newly created AdView has an inactive state. When you are ready to show the ad on the screen,
        /// simply call a <see cref="IAdView.SetActive(bool)"/> method.</para>
        /// <para>If you no longer need the AdView with this size, please call <see cref="IDisposable.Dispose()"/> to free memory.</para>
        /// <para>After calling Dispose(), you can use GetAdView() method to create a new view.</para>
        /// </summary>
        /// <param name="size">The ad size you want using.</param>
        IAdView GetAdView( AdSize size );

        #region Return to App Ads
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
        /// Called when the user clicks on the Ad.
        /// </summary>
        event Action OnAppReturnAdClicked;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnAppReturnAdClosed;

        /// <summary>
        /// The Return Ad which is displayed once the user returns to your application after a certain period of time.
        /// <para>To minimize the intrusiveness, short time periods are ignored.</para>
        /// <para>Return ads are disabled by default.</para>
        /// </summary>
        void SetAppReturnAdsEnabled( bool enable );

        /// <summary>
        /// Calling this method will indicate to skip one next ad impression when returning to the app.
        /// <para>You can call this method when you intentionally redirect the user to another application (for example Google Play)
        /// and do not want them to see ads when they return to your application.</para>
        /// </summary>
        void SkipNextAppReturnAds();

        #endregion

        [Obsolete( "No longer supported" )]
        string GetLastActiveMediation( AdType adType );
    }
}
