using System;
using System.Collections.Generic;

namespace CAS
{
    public enum ConsentStatus
    {
        /// <summary>
        /// Mediation ads network behavior
        /// </summary>
        Undefined,
        /// <summary>
        /// User consents to behavioral targeting in compliance with GDPR.
        /// </summary>
        Accepted,
        /// <summary>
        /// User does not consent to behavioral targeting in compliance with GDPR.
        /// </summary>
        Denied,
    }

    public enum CCPAStatus
    {
        /// <summary>
        /// Mediation ads network behavior
        /// </summary>
        Undefined,
        /// <summary>
        /// User does not consent to the sale of his or her personal information in compliance with CCPA.
        /// </summary>
        OptOutSale,
        /// <summary>
        /// User consents to the sale of his or her personal information in compliance with CCPA.
        /// /// </summary>
        OptInSale,
    }

    public enum Audience
    {
        Mixed,
        /// <summary>
        /// Forces sdk to filter ads with violence, drugs, etc
        /// </summary>
        Children,
        NotChildren,
    }

    public interface IAdsSettings
    {
        /// <summary>
        /// If your application uses Google Analytics (Firebase)
        /// then Clever Ads Solutions collects ad impressions and states analytics.  
        /// This flag has no effect on ad revenue.
        /// Disabling analytics collection may save internet traffic and improve application performance.
        /// Disabled by default.
        /// </summary>
        bool analyticsCollectionEnabled { get; set; }

        /// <summary>
        /// An ad unit’s automatic refresh rate (in seconds) determines how often a new ad request is generated for that ad unit.  
        /// Ad requests should not be made when the device screen is turned off.
        /// We recomended using refresh rate 30 seconds.However,
        /// you can choose any value you want longer than 10 seconds.
        /// 30 seconds by default.
        /// </summary>
        int bannerRefreshInterval { get; set; }

        /// <summary>
        /// You can limit the posting of an interstitial ad to a period of time in seconds after the ad is closed,
        /// during which display attempts will fail.
        /// Default: 0 seconds.
        ///
        /// Note that the interval starts only after the Interstitial Ad closes <see cref="IMediationManager.OnInterstitialAdClosed"/>.
        /// If you need to wait for a period of time after the start of the game or after showing a Rewarded Ad
        /// until next Interstitial Ad impression then please call the following method: <see cref="RestartInterstitialInterval"/>
        /// </summary>
        int interstitialInterval { get; set; }

        /// <summary>
        /// Restart interval until next Interstitial ad display.
        /// By default, the interval before first Interstitial Ad impression is ignored.
        /// You can use this method to delay displaying ad.
        /// </summary>
        void RestartInterstitialInterval();

        /// <summary>
        /// GDPR user Consent SDK Implementation for ads on session.
        /// Default: <see cref="ConsentStatus.Undefined"/>
        /// </summary>
        ConsentStatus userConsent { get; set; }

        /// <summary>
        /// Whether or not user has opted out of the sale of their personal information.
        /// Default: <see cref="CCPAStatus.Undefined"/>
        /// </summary>
        CCPAStatus userCCPAStatus { get; set; }

        /// <summary>
        /// Ad filters by Audience
        /// Default: <see cref="Audience.Mixed"/>
        /// </summary>
        Audience taggedAudience { get; set; }

        /// <summary>
        /// The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdc with tag `CAS`.  
        /// Disabling Debug Mode may improve application performance.
        /// Disabled by default.
        /// </summary>
        bool isDebugMode { get; set; }

        /// <summary>
        /// Sounds in ads mute state
        /// Disabled by default.
        /// </summary>
        bool isMutedAdSounds { get; set; }

        /// <summary>
        /// CAS mediation processing mode of ad requests.
        /// Default: <see cref="LoadingManagerMode.Optimal"/>
        /// </summary>
        LoadingManagerMode loadingMode { get; set; }

        /// <summary>
        /// Identifiers corresponding to test devices which will always request test ads.
        /// The test device identifier for the current device is logged to the console when the first
        /// ad request is made.
        /// </summary>
        void SetTestDeviceIds( List<string> testDeviceIds );

        /// <summary>
        /// Identifiers corresponding to test devices which will always request test ads.
        /// </summary>
        List<string> GetTestDeviceIds();

        /// <summary>
        /// Callbacks from CleverAdsSolutions are not guaranteed to be called on Unity thread.
        /// You can use <see cref="EventExecutor.Add(Action)"/> to schedule each calls on the next Update() loop.
        /// OR enable this property to automatically schedule all calls on the next Update() loop.
        /// Disabled by default.
        /// </summary>
        bool isExecuteEventsOnUnityThread { get; set; }

        /// <summary>
        /// Indicates if the Unity app should be paused when a full screen ad (interstitial
        /// or rewarded video ad) is displayed.
        /// Enabled by default.
        /// </summary>
        bool iOSAppPauseOnBackground { get; set; }
    }
}
