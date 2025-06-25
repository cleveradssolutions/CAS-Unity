//  Copyright © 2025 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    public delegate void CASTypedEvent(AdType adType);
    public delegate void CASTypedEventWithError(AdType adType, string error);
    public delegate void CASEventWithError(string error);
    public delegate void CASEventWithAdError(AdError error);
    public delegate void CASEventWithMeta(AdMetaData meta);

    public delegate void CASInitCompleteEvent(InitialConfiguration config);

    public class WikiPageAttribute : Attribute
    {
        public WikiPageAttribute(string url) { }
    }

    /// <summary>
    /// Interface for managing CAS mediation.
    /// Get instance using the CAS.MobileAds.BuildManager() builder.
    /// </summary>
    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-SDK")]
    public interface IMediationManager
    {
        #region Interstitial Ads events
        /// <summary>
        /// Called when ad ready to shown.
        /// </summary>
        event Action OnInterstitialAdLoaded;
        /// <summary>
        /// Called when failed to load ad response with error message
        /// </summary>
        event CASEventWithAdError OnInterstitialAdFailedToLoad;
        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnInterstitialAdShown;
        /// <summary>
        /// The same call as the <see cref="OnInterstitialAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        [Obsolete("Use OnAdImpression event to collect AdMetaData about the ad impression or OnInterstitialAdShown event if AdMetaData is not used.")]
        event CASEventWithMeta OnInterstitialAdOpening;
        /// <summary>
        /// Called when the ad impression detects paid revenue.
        /// </summary>
        event CASEventWithMeta OnInterstitialAdImpression;
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
        /// Called when ad ready to shown.
        /// </summary>
        event Action OnRewardedAdLoaded;
        /// <summary>
        /// Called when failed to load ad response with error message
        /// </summary>
        event CASEventWithAdError OnRewardedAdFailedToLoad;
        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnRewardedAdShown;
        /// <summary>
        /// The same call as the <see cref="OnRewardedAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        [Obsolete("Use OnAdImpression event to collect AdMetaData about the ad impression or OnRewardedAdShown event if AdMetaData is not used.")]
        event CASEventWithMeta OnRewardedAdOpening;
        /// <summary>
        /// Called when the ad impression detects paid revenue.
        /// </summary>
        event CASEventWithMeta OnRewardedAdImpression;
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

        #region App Open Ads events
        /// <summary>
        /// Called when ad ready to shown.
        /// </summary>
        event Action OnAppOpenAdLoaded;
        /// <summary>
        /// Called when failed to load ad response with error message
        /// </summary>
        event CASEventWithAdError OnAppOpenAdFailedToLoad;
        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnAppOpenAdShown;
        /// <summary>
        /// Called when the ad impression detects paid revenue.
        /// </summary>
        event CASEventWithMeta OnAppOpenAdImpression;
        /// <summary>
        /// Called when the ad is failed to display.
        /// </summary>
        event CASEventWithError OnAppOpenAdFailedToShow;
        /// <summary>
        /// Called when the user clicks on the Ad.
        /// </summary>
        event Action OnAppOpenAdClicked;
        /// <summary>
        /// Called when the ad is closed.
        /// </summary>
        event Action OnAppOpenAdClosed;
        #endregion

        /// <summary>
        /// The CAS identifier
        /// </summary>
        string managerID { get; }

        /// <summary>
        /// Indicates whether the mediation manager is using test ads for the current session.
        /// </summary>
        bool isTestAdMode { get; }

        /// <summary>
        /// Loads an ad of the specified <see cref="AdType"/>.
        /// <para>Before calling <see cref="ShowAd"/> when <see cref="LoadingManagerMode.Manual"/> is in use, you must call this method to load the ad.</para>
        /// <para>To receive a callback when the ad is successfully loaded, subscribe to the OnLoadedAd events.</para>
        /// <para>For loading banner ads, use <see cref="IAdView.Load"/> obtained from <see cref="GetAdView"/>.</para>
        /// </summary>
        /// <param name="adType">The type of ad to load.</param>
        void LoadAd(AdType adType);

        /// <summary>
        /// Checks whether the specified <see cref="AdType"/> ad is ready to be shown.
        /// <para>To get detailed information about why an ad may not be shown, 
        /// subscribe to the OnAdFailedToShow event instead of relying solely on this method. 
        /// This event provides insights into errors that may prevent the ad from being displayed.</para>
        /// <para>For checking if a banner ad is ready, use the <see cref="IAdView.isReady"/> property from <see cref="GetAdView"/>.</para>
        /// </summary>
        /// <param name="adType">The type of ad to check.</param>
        /// <returns>True if the ad is ready to be shown; otherwise, false.</returns>
        bool IsReadyAd(AdType adType);

        /// <summary>
        /// Shows an ad of the specified <see cref="AdType"/>.
        /// <para>To show a banner ad, use the <see cref="IAdView.SetActive(bool)"/> method obtained from <see cref="GetAdView"/>.</para>
        /// </summary>
        /// <param name="adType">The type of ad to show.</param>
        void ShowAd(AdType adType);

        /// <summary>
        /// Disposes of the ad of the specified <see cref="AdType"/>.
        /// <para>After calling this method, if you want to resume showing ads, 
        /// you must call the <see cref="LoadAd"/> method to reload the ad. 
        /// Note that in automatic loading mode, ads will not be automatically reloaded after Dispose. 
        /// You need to explicitly call <see cref="LoadAd"/> to prepare the ad for future displays.</para>
        /// <para>For disposing of a banner ad, use the <see cref="IDisposable.Dispose"/> method obtained from <see cref="GetAdView"/>.</para>
        /// </summary>
        /// <param name="adType">The type of ad to dispose.</param>
        void DisposeAd(AdType adType);

        /// <summary>
        /// Retrieves the ad view interface for the specified <paramref name="size"/>.
        /// <para>If an ad view of the specified size already exists, a reference to the existing view will be returned instead of creating a new one.</para>
        /// <para>Note that the returned <see cref="IAdView"/> is initially inactive. To display the ad, you need to activate the view by calling <see cref="IAdView.SetActive(bool)"/>.</para>
        /// <para>When the ad view is no longer needed, call <see cref="IDisposable.Dispose"/> to release the associated resources and free up memory.</para>
        /// <para>After disposing of the ad view, you can use this method again to create a new view if needed.</para>
        /// </summary>
        /// <param name="size">The desired size of the ad view.</param>
        /// <returns>The ad view interface for the specified size.</returns>
        IAdView GetAdView(AdSize size);

        #region Return to App Ads eveents
        /// <summary>
        /// Called when the ad is displayed.
        /// </summary>
        event Action OnAppReturnAdShown;
        /// <summary>
        /// The same call as the <see cref="OnAppReturnAdShown"/> but with <see cref="AdMetaData"/> about the impression. 
        /// </summary>
        [Obsolete("Use OnAdImpression event to collect AdMetaData about the ad impression or OnAppReturnAdShown event if AdMetaData is not used.")]
        event CASEventWithMeta OnAppReturnAdOpening;
        /// <summary>
        /// Called when the ad impression detects paid revenue.
        /// </summary>
        event CASEventWithMeta OnAppReturnAdImpression;
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
        #endregion

        /// <summary>
        /// The Return Ad which is displayed once the user returns to your application after a certain period of time.
        /// <para>To minimize the intrusiveness, short time periods are ignored.</para>
        /// <para>Return ads are disabled by default.</para>
        /// </summary>
        void SetAppReturnAdsEnabled(bool enable);

        /// <summary>
        /// Calling this method will indicate to skip one next ad impression when returning to the app.
        /// <para>You can call this method when you intentionally redirect the user to another application (for example Google Play)
        /// and do not want them to see ads when they return to your application.</para>
        /// </summary>
        void SkipNextAppReturnAds();

        /// <summary>
        /// Set enabled <see cref="AdType"/> to processing.
        /// <para>If processing is inactive then all calls to the selected ad type
        /// will fail with error <see cref="AdError.ManagerIsDisabled"/>.</para>
        /// <para>The state will not be saved between sessions.</para>
        /// </summary>
        void SetEnableAd(AdType adType, bool enabled);

        /// <summary>
        /// Check enabled <see cref="AdType"/> is processing.
        /// Read more about <see cref="SetEnableAd"/>.
        /// </summary>
        bool IsEnabledAd(AdType adType);

        /// <summary>
        /// The latest free ad page for your own promotion.
        /// <br>This ad page will be displayed when there is no paid ad to show or internet availability.</br>
        /// <br>By default, this page will not be displayed while the ad content is NULL.</br>
        /// <para>**Attention!** Impressions and clicks on this ad page don't make money.</para>
        /// </summary>
        LastPageAdContent lastPageAdContent { get; set; }
    }
}
