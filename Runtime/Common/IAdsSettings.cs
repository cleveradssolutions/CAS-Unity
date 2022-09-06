//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace CAS
{
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Privacy-Laws" )]
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

    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Privacy-Laws" )]
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

    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Privacy-Laws" )]
    public enum Audience
    {
        /// <summary>
        /// The user's age has not been determined.
        /// 
        /// <para>If your app's target age groups include both children and older audiences,
        /// any ads that may be shown to children must comply with Google Play's Families Ads Program.
        /// A neutral age screen must be implemented so that any ads not suitable for children are only shown to older audiences.
        /// A neutral age screen is a mechanism to verify a user's age in a way that doesn't encourage them to falsify their age
        /// and gain access to areas of your app that aren't designed for children, for example, an age gate.</para>
        /// <para>You could change the audience at runtime after determining the user's age.</para>
        /// </summary>
        Mixed,
        /// <summary>
        /// Audiences under the age of 13 who subject of COPPA.
        ///
        /// <para>When using this feature, a Tag For Users under the Age of Consent in Europe (TFUA) parameter
        /// will be included in the ad request.
        /// Also the state of GDPR and CCPA will be overridden automatically
        /// to <see cref="ConsentStatus.Denied"/> and <see cref="CCPAStatus.OptOutSale"/></para>
        ///
        /// <para>It also allows application to comply with the Google Play Families Policy requirements:</para>
        /// <para>- Ads displayed to those users do not involve interest-based advertising or remarketing;</para>
        /// <para>- Ads displayed to those users present content that is appropriate for children;</para>
        /// <para>- Ads displayed to those users follow the Families ad format requirements;</para>
        /// <para>- Compliance with all applicable legal regulations and industry standards relating to advertising to children.</para>
        /// </summary>
        Children,
        /// <summary>
        /// Audiences over the age of 13 NOT subject to the restrictions of child protection laws.
        /// </summary>
        NotChildren,
    }

    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Configuring-SDK" )]
    public interface IAdsSettings
    {
        /// <summary>
        /// GDPR user Consent SDK Implementation for ads on session.
        /// <para>Default: <see cref="ConsentStatus.Undefined"/></para>
        /// </summary>
        ConsentStatus userConsent { get; set; }

        /// <summary>
        /// Whether or not user has opted out of the sale of their personal information.
        /// <para>Default: <see cref="CCPAStatus.Undefined"/></para>
        /// </summary>
        CCPAStatus userCCPAStatus { get; set; }

        /// <summary>
        /// Ad filters by Audience
        /// <para>By default selected in `Assets/CleverAdsSolutions/Settings` menu</para>
        /// </summary>
        Audience taggedAudience { get; set; }

        /// <summary>
        /// An ad unit’s automatic refresh rate (in seconds) determines how often a new ad request is generated for that ad unit.  
        /// <para>Ad requests should not be made when the device screen is turned off.</para>
        /// <para>We recomended using refresh rate 30 seconds.</para>
        /// <para>However, you can choose any value you want longer than 10 seconds.</para>
        /// <para>30 seconds by default.</para>
        /// </summary>
        int bannerRefreshInterval { get; set; }

        /// <summary>
        /// You can limit the posting of an interstitial ad to a period of time in seconds after the ad is closed,
        /// during which display attempts will fail.
        /// <para>Default: 0 seconds.</para>
        /// <para>Note that the interval starts only after the Interstitial Ad closes <see cref="IMediationManager.OnInterstitialAdClosed"/>.</para>
        /// <para>If you need to wait for a period of time after the start of the game or after showing a Rewarded Ad
        /// until next Interstitial Ad impression then please call the following method: <see cref="RestartInterstitialInterval"/></para>
        /// </summary>
        int interstitialInterval { get; set; }

        /// <summary>
        /// Restart interval until next Interstitial ad display.
        /// <para>By default, the interval before first Interstitial Ad impression is ignored.</para>
        /// <para>You can use this method to delay displaying ad.</para>
        /// </summary>
        void RestartInterstitialInterval();

        /// <summary>
        /// Sounds in ads mute state
        /// <para>Disabled by default.</para>
        /// </summary>
        bool isMutedAdSounds { get; set; }

        /// <summary>
        /// This option will compare ad cost and serve regular interstitial ads
        /// when rewarded video ads are expected to generate less revenue.
        /// <para>Interstitial Ads does not require to watch the video to the end,
        /// but the <see cref="IMediationManager.OnRewardedAdCompleted"/> callback will be triggered in any case.</para>
        /// <para>By default selected in `Assets/CleverAdsSolutions/Settings` menu</para>
        /// </summary>
        bool allowInterstitialAdsWhenVideoCostAreLower { get; set; }

        /// <summary>
        /// The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdc with tag `CAS`.  
        /// <para>Disabling Debug Mode may improve application performance.</para>
        /// Disabled by default.
        /// </summary>
        bool isDebugMode { get; set; }

        /// <summary>
        /// Identifiers corresponding to test devices which will always request test ads.
        /// List of test devices should be defined before first MediationManager initialized.
        /// <para>1. Run an app configured with the CAS SDK.</para>
        /// <para>2. Check the console or logcat output for a message that looks like this:
        /// "To get test ads on this device, set ... "</para>
        /// <para>3. Copy your alphanumeric test device ID to your clipboard.</para>
        /// <para>4. Add the test device ID to the list.</para>
        /// </summary>
        void SetTestDeviceIds( List<string> testDeviceIds );

        /// <summary>
        /// Identifiers corresponding to test devices which will always request test ads.
        /// </summary>
        List<string> GetTestDeviceIds();

        /// <summary>
        /// CAS mediation processing mode of ad requests.
        /// <para>By default selected in `Assets/CleverAdsSolutions/Settings` menu</para>
        /// </summary>
        LoadingManagerMode loadingMode { get; set; }

        /// <summary>
        /// Callbacks from CleverAdsSolutions are not guaranteed to be called on Unity thread.
        /// <para>You can use <see cref="EventExecutor.Add(Action)"/> to schedule each calls on the next Update() loop.
        /// OR enable this property to automatically execute all calls on the next Update() loop.</para>
        /// <para>Enabled by default.</para>
        /// </summary>
        bool isExecuteEventsOnUnityThread { get; set; }

        /// <summary>
        /// Indicates if the Unity app should be paused when a full screen ad (interstitial
        /// or rewarded video ad) is displayed.
        /// <para>Enabled by default.</para>
        /// </summary>
        bool iOSAppPauseOnBackground { get; set; }

        /// <summary>
        /// The SDK automatically collects location data if the user allowed the app to track the location.
        /// <para>iOS supported only right now.</para>
        /// <para>By default selected in `Assets/CleverAdsSolutions/Settings` menu</para>
        /// </summary>
        bool trackLocationEnabled { get; set; }

        /// <summary>
        /// If your application uses Google Analytics (Firebase)
        /// then Clever Ads Solutions collects ad impressions and states analytics.  
        /// <para>This flag has no effect on ad revenue.</para>
        /// <para>Disabling analytics collection may save internet traffic and improve application performance.</para>
        /// Disabled by default.
        /// </summary>
        bool analyticsCollectionEnabled { get; set; }
    }
}
