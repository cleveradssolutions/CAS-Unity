# CleverAdsSolutions-Unity SDK Integration
The Clever Ads Solutions Unity plugin enables Unity developers to easily serve Mobile Ads on Android and iOS apps without having to write Java or Objective-C code. The plugin provides a C# interface for requesting ads that is used by C# scripts in your Unity project.

[![GitHub package.json version](https://img.shields.io/github/package-json/v/cleveradssolutions/CAS-Unity?label=Unity%20Package)](https://github.com/cleveradssolutions/CAS-Unity/releases/latest)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/CleverAdsSolutions/CAS-Android?label=CAS%20Android)](https://github.com/cleveradssolutions/CAS-Android)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/CleverAdsSolutions/CAS-iOS?label=CAS%20iOS)](https://github.com/cleveradssolutions/CAS-iOS)
[![App-ads.txt](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/cleveradssolutions/App-ads.txt/master/Shield.json)](https://github.com/cleveradssolutions/App-ads.txt)

## Requirements
### Unity
- Unity Editor version: 2017.4, 2018.4, 2019, 2020.1  
> You can try any version you want, however, non-recommended versions can lead to unexpected errors.
### Android
- Android version 4.4 (API level 19) and up
- Gradle 3.4.3 and up
### iOS
- XCode version 12.2 and up
- iOS version 10 and up
- Cocoapods applied
### CAS Unity Demo App
The Integration Demo application demonstrate how to integrate the CAS in your app.  
[Repository Unity Sample Application](https://github.com/cleveradssolutions/CAS-Unity-Sample)

# Table of contents
 1.  [Add the CAS package to Your Project](#step-1-add-the-cas-package-to-your-project)  
 2.  [Configuring CAS SDK](#step-2-configuring-cas-sdk)  
 3.  [Privacy Laws](#step-3-privacy-laws)  
 3.1.  [GDPR Managing Consent](#gdpr-managing-consent)  
 3.2.  [CCPA Compliance](#ccpa-compliance)  
 3.3.  [COPPA and EEA Compliance](#coppa-and-eea-compliance)  
 4.  [Initialize CAS SDK](#step-4-initialize-cas-sdk)  
 5.  [Implement CAS Events](#step-5-implement-cas-events)  
 6.  [Implement our Ad Units](#step-6-implement-our-ad-units)  
 7.  [Include native platforms](#step-7-include-native-platforms)  
 7.1 [Include Android](#include-android)  
 7.2 [Include iOS](#include-ios)   
 8.  [GitHub issue tracker](#github-issue-tracker)  
 9.  [Support](#support)  
 10.  [License](#license)  

## Step 1 Add the CAS package to Your Project
if you are using Unity 2018.4 or newer then you can add CAS SDK to your Unity project using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html), or you can import the package manually.

<details><summary><b>Unity Package Manager</b></summary>

> Allowed only if you are using Unity 2018.4 or newer.  

Add the **Game Package Registry by Google**  and CAS dependency to your Unity project.  
Modify `Packages/manifest.json`  to the following form:
```json
{
"scopedRegistries": [
  {
    "name": "Game Package Registry by Google",
    "url": "https://unityregistry-pa.googleapis.com",
    "scopes": [
      "com.google"
    ]
  }
],
"dependencies": {
    "com.cleversolutions.ads.unity" 
        : "https://github.com/cleveradssolutions/CAS-Unity.git#1.8.2"
}
}
```
> Note that some other SDKs, such as the Firebase SDK, may contain [EDM4U](https://github.com/googlesamples/unity-jar-resolver) in their .unitypackage. Check if `Assets/ExternalDependencyManager` or `Assets/PlayServicesResolver` folders exist. If these folders exist, remove them before installing any CAS SDK through Unity Package Manager.
***
</details>
<details><summary><b>Manual installation</b></summary>

1. Download latest [CleverAdsSolutions.unitypackage](https://github.com/cleveradssolutions/CAS-Unity/releases/latest)
2. In your open Unity project, navigate to **Assets > Import Package > Custom Package**.
3. In the *Import Unity Package* window, make sure all of the files are selected and click **Import**.
***
</details>
<details><summary><b>Cross Promotion</b></summary>

Cross promotion is an app marketing strategy in which app developers promote one of their titles on another one of their titles. Cross promoting is especially effective for developers with large portfolios of games as a means to move users across titles and use the opportunity to scale each of their apps. This is most commonly used by hyper-casual publishers who have relatively low retention, and use cross promotion to keep users within their app portfolio.  

Start your cross promotion campaign with CAS [here](https://cleveradssolutions.com).

Dependency of `CrossPromotion` can be disabled/enabled using Advanced Integration in `Assets > CleverAdsSolutions > Settings` window.
***
</details>
<details><summary><b>Third party mediation partners</b></summary>

- Google Ads  
Banner, Interstitial, Rewarded Video - [Home](https://admob.google.com/home) - [Privacy Policy](https://policies.google.com/technologies/ads)
- Unity Ads  
Banner, Interstitial, Rewarded Video - [Home](https://unity.com/solutions/unity-ads) - [Privacy Policy](https://unity3d.com/legal/privacy-policy)
- IronSource  
~~Banner~~, Interstitial, Rewarded Video - [Home](https://www.ironsrc.com) - [Privacy Policy](https://developers.ironsrc.com/ironsource-mobile/air/ironsource-mobile-privacy-policy/)
- AdColony  
Banner, Interstitial, Rewarded Video - [Home](https://www.adcolony.com) - [Privacy Policy](https://www.adcolony.com/privacy-policy/)
- Kidoz  
Banner, Interstitial, Rewarded Video - [Home](https://kidoz.net) - [Privacy Policy](https://kidoz.net/privacy-policy/)
- Vungle  
Banner, Interstitial, Rewarded Video - [Home](https://vungle.com) - [Privacy Policy](https://vungle.com/privacy/)
- AppLovin  
Banner, Interstitial, Rewarded Video - [Home](https://www.applovin.com) - [Privacy Policy](https://www.applovin.com/privacy/)
- StartApp  
Banner, Interstitial, Rewarded Video - [Home](https://www.startapp.com) - [Privacy Policy](https://www.startapp.com/policy/privacy-policy/)
- InMobi  
Banner, Interstitial, Rewarded Video - [Home](https://www.inmobi.com) - [Privacy Policy](https://www.inmobi.com/privacy-policy/)
- Chartboost  
Banner, Interstitial, Rewarded Video - [Home](https://www.chartboost.com) - [Privacy Policy](https://answers.chartboost.com/en-us/articles/200780269)
- SuperAwesome  
Banner, Interstitial, Rewarded Video - [Home](https://www.superawesome.com) - [Privacy Policy](https://www.superawesome.com/privacy-hub/privacy-policy/)  
*Works only for children audience*
- Facebook Audience Network  
Banner, Interstitial, Rewarded Video  - [Home](https://www.facebook.com/business/marketing/audience-network) - [Privacy Policy](https://developers.facebook.com/docs/audience-network/policy/)
- Yandex Ads  
Banner, Interstitial, Rewarded Video - [Home](https://yandex.com/dev/mobile-ads/) - [Privacy Policy](https://yandex.com/legal/mobileads_sdk_agreement/) 
#### Dependencies of Closed Beta third party partners:
> :warning:  Next dependencies in closed beta and available upon invite only. If you would like to be considered for the beta, please contact Support.
- Verizon Media  
Banner, Interstitial, Rewarded Video - [Home](https://www.verizonmedia.com/advertising/solutions#/mobile)- [Privacy Policy](https://www.verizonmedia.com/policies/us/en/verizonmedia/privacy/)
- MyTarget  
Banner, Interstitial, Rewarded Video - [Home](https://target.my.com/) - [Privacy Policy](https://legal.my.com/us/mytarget/privacy/)  
*Works to CIS countries only*
- MobFox  
Banner, Interstitial, Rewarded Video - [Home](https://www.mobfox.com) - [Privacy Policy](https://www.mobfox.com/privacy-policy/)
- Amazon Ads  
Banner, ~~Interstitial, Rewarded Video~~ - [Home](https://advertising.amazon.com) - [Privacy Policy](https://advertising.amazon.com/legal/privacy-notice)
***
</details>
<details><summary><b>Don’t forget to implement app-ads.txt (Optional)</b></summary>

For both iOS and Android integrations, we encourage our partners to adopt this file and help us combat ad fraud.  
Read detailed instructions on [how to create and upload your app-ads.txt file](https://github.com/cleveradssolutions/App-ads.txt#cleveradssolutions-app-adstxt).
***
</details>

## Step 2 Configuring CAS SDK
In your open Unity project, navigate to **Assets > CleverAdsSolutions > Settings** to create and modify build settings.

<details><summary><b>Test Ad Mode</b></summary>

The quickest way to testing is to enable Test Ad Mode. These ad are not associated with your account, so there's no risk of your account generating invalid traffic when using these ad units.
***
</details><details><summary><b>Manager Ids</b></summary>

Add your Clever Ads Solutions manager Id's.
> If you haven't created an CAS account and registered an manager yet,  now's a great time to do so at [cleveradssolutions.com](https://cleveradssolutions.com). If you're just looking to experiment with the SDK, though, you can use the Test Ad Mode above.  
***
</details><details><summary><b>Allowed ads types in app</b></summary>

To improve the performance of your application, we recommend that you only allow ad types that will actually be used in the application. For example: Banner and Interstitial ad.  

The processes of ad types can be disabled/enabled at any time using following method:
```c#
CAS.MobileAds.manager.SetEnableAd(adType, enabled);
```
***
</details><details><summary><b>Audience Tagged</b></summary>

Choose the audience your game is targeting.   
In addition to targeting ads, on Google Play has restrictions games participate in the family apps program. These games can only serve ads from certified ad networks. [More about Families Ads Program](https://support.google.com/googleplay/android-developer/answer/9283445).  
Changing this setting will change the project dependencies. Please follow the instructions provided in the settings window.
***
</details><details><summary><b>Banner Size</b></summary>

Select the banner size to initialize.  

| Size in dp (WxH) |      Description     |    Availability    |  CASSize constant |
|:----------------:|:--------------------:|:------------------:|:----------------:|
|      320x50      |    Standard Banner   | Phones and Tablets |      Banner      |
|      728x90      |    IAB Leaderboard   |       Tablets      |    Leaderboard   |
|      300x250     | IAB Medium Rectangle | Phones and Tablets | MediumRectangle |
| Adaptive | Adaptive banner | Phones and Tablets | AdaptiveBanner |
| 320x50 or 728x90 | Smart Banner | Phones and Tablets | SmartBanner |

**Adaptive banners** are the next generation of responsive ads, maximizing performance by optimizing ad size for each device.  
To pick the best ad size, adaptive banners use fixed aspect ratios instead of fixed heights. This results in banner ads that occupy a more consistent portion of the screen across devices and provide opportunities for improved performance. [You can read more in this article.](https://developers.google.com/admob/android/banner/adaptive)  

**Smart Banners** selects one of the sizes depending on the device: on phones have a `Banner` size or on tablets a `Leaderboard` size.

> We do not recommend resizing your banner frequently while playing as this will result in the loss of expensive ad impressions.  

Change the banner size using the following method:
```c#
CAS.MobileAds.manager.bannerSize = bannerSize;
```

> If you use Manual [Loading Manager Mode](#step-2-configuring-cas-sdk) then please call [Load an Ad](#step-6-implement-our-ad-units) each banner size changed.  

You can get the current banner size in pixels at any time using the following methods:
```c#
float height = CAS.MobileAds.manager.GetBannerHeightInPixels();
float width = CAS.MobileAds.manager.GetBannerWidthInPixels();
```
***
</details><details><summary><b>Banner Refresh rate</b></summary>

An ad unit’s automatic refresh rate (in seconds) determines how often a new ad request is generated for that ad unit.  
> Ad requests should not be made when the device screen is turned off.  
We recomended using refresh rate 30 seconds. However, you can choose any value you want longer than 10 seconds.  

Change the banner automatic refresh rate using the following method:
```c#
CAS.MobileAds.settings.bannerRefreshInterval = refreshInterval;
```
***
</details><details><summary><b>Interstitial impression interval</b></summary>

You can limit the posting of an interstitial ad to a period of time in seconds after the ad is closed, during which display attempts will fail.

Change the interstitial ad impression interval using the following method:
```c#
CAS.MobileAds.settings.interstitialInterval = interval;
```

**Note** that the interval starts only after the Interstitial Ad **closes** `OnInterstitialAdClosed`. If you need to wait for a period of time after the start of the game or after showing a Rewarded Ad until next Interstitial Ad impression then please call the following method:
```c#
void Start(){
    // Wait for a period of time before first impression
    CAS.MobileAds.settings.RestartInterstitialInterval();

    // Subscribe to close Rewarded Ad event
    CAS.MobileAds.manager.OnRewardedAdClosed += OnRewardedAdClosed;
}
void OnRewardedAdClosed(){
    // Wait for a period of time after Rewarded Ad Closed until next Interstitial Ad Impression
    CAS.MobileAds.settings.RestartInterstitialInterval();
}
```

> During interval after ad closed display attempts will fire event `OnInterstitialAdFailedToShow`.
***
</details><details><summary><b>Loading mode</b></summary>

|        Mode        |  Load<sup>[*1](#load-f-1)</sup>  | Impact on App performance | Memory usage |        Actual ads<sup>[*2](#actual-f-2)</sup>       |
|:------------------:|:------:|:-------------------------:|:------------:|:------------------------:|
|   FastestRequests  |  Auto  |         Very high         |     High     |       Most relevant      |
|    FastRequests    |  Auto  |            High           |    Balance   |      High relevance      |
|  Optimal *(Default)*  |  Auto  |          Balance          |    Balance   |          Balance         |
|   HighPerformance  |  Auto  |            Low            |      Low     |       Possible loss      |
| HighestPerformance |  Auto  |          Very low         |      Low     |       Possible loss      |
|       Manual      | Manual<sup>[*3](#manual-f-3)</sup> |          Very low         |      Low     | Depends on the frequency |

Change the Clever Ads Solution processing mode using the following method:
```c#
CAS.MobileAds.settings.loadingMode = mode;
```

<b id="load-f-1">^1</b>: Auto control load mediation ads starts immediately after initialization and will prepare displays automatically.  

<b id="actual-f-2">^2</b>: Potential increase in revenue by increasing the frequency of ad requests. At the same time, it greatly affects the performance of the application.  

<b id="manual-f-3">^3</b>: Manual control loading mediation ads requires manual preparation of advertising content for display. Use ad loading methods before trying to show: `CAS.MobileAds.manager.LoadAd()`.  
***
</details><details><summary><b>Debug mode</b></summary>

The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdc with tag `CAS`.  
Disabling Debug Mode may improve application performance.  

Change the Debug Mode flag at any time using the following method:
```c#
CAS.MobileAds.settings.isDebugMode = enabled;
```
Disabled by default.
***
</details><details><summary><b>Analytics Collection</b></summary>

If your application uses Google Analytics (Firebase) then Clever Ads Solutions collects ad impressions and states analytics.  
**This flag has no effect on ad revenue.**  
Disabling analytics collection may save internet traffic and improve application performance.  

Change the analytics collection flag at any time using the following method:
```c#
CAS.MobileAds.settings.analyticsCollectionEnabled = enabled;
```
Disabled by default.  
***
</details><details><summary><b>Muted Ads Sounds</b></summary>

Sounds in ads mute state.  
```c#
CAS.MobileAds.settings.isMutedAdSounds = mute;
```
Disabled by default.  
***
</details><details><summary><b>Execute Events On Unity Thread</b></summary>

Callbacks from CleverAdsSolutions are not guaranteed to be called on Unity thread.
You can use EventExecutor to schedule each calls on the next `Update()` loop:
```c#
CAS.EventExecutor.Add(callback);
```
OR enable `isExecuteEventsOnUnityThread` property to automatically schedule all calls on the next `Update()` loop.
```c#
CAS.MobileAds.settings.isExecuteEventsOnUnityThread = enable;
```
Disabled by default.  
***
</details><details><summary><b>Test Device Ids</b></summary>

Identifiers corresponding to test devices which will always request test ads.
The test device identifier for the current device is logged to the console when the first ad request is made.
```c#
CAS.MobileAds.settings.SetTestDeviceIds(testDeviceIds);
```
***
</details><details><summary><b>Allow Interstitial Ads When Video Cost Are Lower</b></summary>

This option will compare ad cost and serve regular interstitial ads when rewarded video ads are expected to generate less revenue.  
Interstitial Ads does not require to watch the video to the end, but the OnRewardedAdCompleted callback will be triggered in any case.  

Change the flag at any time using the following method:
```c#
CAS.MobileAds.settings.allowInterstitialAdsWhenVideoCostAreLower = allow;
```
Disabled by default.  
***
</details><details><summary><b>Targeting Options</b></summary>

You can now easily tailor the way you serve your ads to fit a specific audience!  
You’ll need to inform our servers of the users’ details so the SDK will know to serve ads according to the segment the user belongs to.  
```c#
// Set user gender
CAS.MobileAds.targetingOptions.gender = CAS.Gender.Male;
// Set user age. Limitation: 1-99 and 0 is 'unknown'
CAS.MobileAds.targetingOptions.age = 12;
```
***
</details><details><summary><b>Last Page Ad Content</b></summary>

The latest free ad page for your own promotion.  
This ad page will be displayed when there is no paid ad to show or internet availability.  

**Attention!** Impressions and clicks on this ad page don't make money.  

Change the `LastPageAdContent` at any time using the following method:
```c#
CAS.MobileAds.manager.lastPageAdContent = new LastPageAdContent(...);
```
By default, this page will not be displayed while the ad content is NULL.  
***
</details><details><summary><b>iOS Tracking Usage Description</b></summary>

**Property for iOS only.**  
iOS 14 and above requires publishers to obtain permission to track the user's device across applications.  
To display the App Tracking Transparency authorization request for accessing the IDFA, update your Info.plist to add the NSUserTrackingUsageDescription key with a custom message describing your usage.  

> You can leave the field blank so that CAS does not define NSUserTrackingUsageDescription.  

Below is an example description text:
- This identifier will be used to deliver personalized ads to you.
- Your data will be used to provide you a better and personalized ad experience.
- We try to show ads for apps and products that will be most interesting to you based on the apps you use.
- We try to show ads for apps and products that will be most interesting to you based on the apps you use, the device you are on, and the country you are in.  

For more information, see [Apple's developer documentation](https://developer.apple.com/documentation/bundleresources/information_property_list/nsusertrackingusagedescription) or [Google Ads documentation](https://developers.google.com/admob/ios/ios14#request).

> **Important!** CAS does not provide legal advice. Therefore, the information on this page is not a substitute for seeking your own legal counsel to determine the legal requirements of your business and processes, and how to address them.
***
</details><details><summary><b>iOS Track Location Enabled</b></summary>

**Property for iOS only.**  
The SDK automatically collects location data if the user allowed the app to track the location.
Change the flag at any time using the following method:
```c#
CAS.MobileAds.settings.trackLocationEnabled = enabled;
```
Disabled by default.  
***
</details><details><summary><b>iOS App Pause On Background</b></summary>

**Property for iOS only.**  
Indicates if the Unity app should be automatically paused when a full screen ad (Interstitial or Rewarded video ad) is displayed.
```c#
CAS.MobileAds.settings.iOSAppPauseOnBackground = pause;
```
Enabled by default.
***
</details>

## Step 3 Privacy Laws
This documentation is provided for compliance with various privacy laws. If you are collecting consent from your users, you can make use of APIs discussed below to inform CAS and all downstream consumers of this information.  

A detailed article on the use of user data can be found in the [Privacy Policy](https://github.com/cleveradssolutions/CAS-Android/wiki/Privacy-Policy).

### GDPR Managing Consent
<details>

This documentation is provided for compliance with the European Union's [General Data Protection Regulation (GDPR)](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32016R0679). In order to pass GDPR consent from your users, you should make use of the APIs and methods discussed below to inform CAS and all downstream consumers of this information.  

**Passing Consent** to CAS API, use this functions:  
User consents to behavioral targeting in compliance with GDPR.
```c#
CAS.MobileAds.settings.userConsent = ConsentStatus.Accepted;
```
User does not consent to behavioral targeting in compliance with GDPR.
```c#
CAS.MobileAds.settings.userConsent = ConsentStatus.Denied;
```
By default, user consent management is passed on to media networks. For reset state:
```c#
CAS.MobileAds.settings.userConsent = ConsentStatus.Undefined;
```
***
</details>

### CCPA Compliance
<details>

This documentation is provided for compliance with the California Consumer Privacy Act (CCPA). In order to pass CCPA opt-outs from your users, you should make use of the APIs discussed below to inform CAS and all downstream consumers of this information.  

**Passing consent to the sale personal information**
User does not consent to the sale of his or her personal information in compliance with CCPA.
```c#
CAS.MobileAds.settings.userCCPAStatus = CCPAStatus.OptOutSale;
```
User consents to the sale of his or her personal information in compliance with CCPA.
```c#
CAS.MobileAds.settings.userCCPAStatus = CCPAStatus.OptInSale;
```
By default, user consent management is passed on to media networks. For reset state:
```c#
CAS.MobileAds.settings.userCCPAStatus = CCPAStatus.Undefined;
```
***
</details>

### COPPA and EEA Compliance
<details>

This documentation is provided for additional compliance with the [Children’s Online Privacy Protection Act (COPPA)](https://www.ftc.gov/tips-advice/business-center/privacy-and-security/children%27s-privacy). Publishers may designate all inventory within their applications as being child-directed or as COPPA-applicable though our UI. Publishers who have knowledge of specific individuals as being COPPA-applicable should make use of the API discussed below to inform CAS and all downstream consumers of this information.  

You can mark your ad requests to receive treatment for users in the European Economic Area (EEA) under the age of consent. This feature is designed to help facilitate compliance with the General Data Protection Regulation (GDPR). Note that you may have other legal obligations under GDPR. Please review the European Union’s guidance and consult with your own legal counsel. Please remember that CAS tools are designed to facilitate compliance and do not relieve any particular publisher of its obligations under the law.

Call `Audience.children` indicate that user want get content treated as child-directed for purposes of COPPA or receive treatment for users in the European Economic Area (EEA) under the age of consent. 
```c#
CAS.MobileAds.settings.taggedAudience = Audience.Children;
```
Call `Audience.notChildren` to indicate that user **don't** want get content treated as child-directed for purposes of COPPA or **not** receive treatment for users in the European Economic Area (EEA) under the age of consent.
```c#
CAS.MobileAds.settings.taggedAudience = Audience.NotChildren;
```
By default, the audience is unknown and the mediation ad network will work as usual. For reset state:
```c#
CAS.MobileAds.settings.taggedAudience = Audience.Mixed;
```
***
</details>

## Step 4 Initialize CAS SDK
Before loading ads, have your app initialize the CAS SDK by calling `CAS.MobileAds.Initialize()` which initializes the SDK and calls back a completion listener once initialization is complete. 
Initialize can be called for different identifiers to create different managers (Placement).
This needs to be done only once for each manager, ideally at app launch.

<details><summary><b>Simple initialize</b></summary>

Also to initialize SDK using settings from resources, created by menu `Assets > CleverAdsSolutions > Settings`, there is the following method:
```c#
CAS.MobileAds.InitializeFromResources(
    managerIndex: 0,
    initCompleteAction: (success, error) => { 
        // CAS manager initialization done  
    });
```
***
</details><details><summary><b>Advanced initialzie</b></summary>

Here's an example of how to call `Initialize()` within the `Start()` method of a script attached to a GameObject:
```c#
using CAS;
...
public class CleverAdsSolutionsDemoScript : MonoBehaviour
{
    IMediationManager manager;
    public void Start()
    {
        // Initialize the Clever Ads Solutions SDK manager.
        manager = MobileAds.Initialize(
            // CAS manager (Placement) identifier.
            managerID, 
            // Optional set active Ad Types: 'AdFlags.Banner | AdFlags.Interstitial' for example.
            // Ad types can be enabled manually after initialize by IMediationManager.SetEnableAd
            enableAd: AdFlags.Everything,
            // Optional Enable demo mode that will always request test ads. Set FALSE for release!  
            testAdMode: !releaseBuild,
            // Optional subscribe to initialization done
            initCompleteAction: (success, error) => { 
                // CAS manager initialization done  
            });
    }
}
```
***
</details><details><summary><b>Validate of native  mediation integration (Optional)</b></summary>

The CAS SDK provides an easy way to verify that you’ve successfully integrated any additional adapters; it also makes sure all required dependencies and frameworks were added for the various mediated ad networks.   
After you have finished your integration, call the following static method and confirm that all networks you have implemented are marked as VERIFIED:  
```c#
CAS.validateIntegration();
```
Find log information by tag: **CASIntegrationHelper**

Once you’ve successfully verified your integration, please remember to **remove the integration helper from your code**.

The Integration Helper tool reviews everything, including ad networks you may have intentionally chosen NOT to include in your application. These will appear as MISSING and there is no reason for concern. In the case the ad network’s integration has not been completed successfully, it will be marked as NOT VERIFIED.  
***
</details>

## Step 5 Implement CAS Events
The CAS Unity Plugin fires several events to inform you of ad availability and states.

<details><summary><b>General Ad availability events</b></summary>

```c#
void OnEnable () {
    ...
    // Executed when AdType load ad response
    manager.OnLoadedAd += CASLoaded;
    // Executed when AdType failed to load ad response with error message
    manager.OnFailedToLoadAd += CASFailedToLoad;
}
```
***
</details><details><summary><b>Banner Ad state events</b></summary>

```c#
void OnEnable () {
    ...
    // Executed when the ad is displayed.
    manager.OnBannerAdShown += CASBannerShown;
    // Executed when the ad is failed to display.
    // The Banner may automatically appear when the Ad is ready again.
    // This will trigger the OnBannerAdShown callback again.
    manager.OnBannerAdFailedToShow += CASBannerFailedToShow;
    // Executed when the user clicks on an Ad.
    manager.OnBannerAdClicked += CASBannerClicked;
    // Executed when the ad is hidden from screen.
    manager.OnBannerAdHidden += CASBannerAdHidden;
}
```
***
</details><details><summary><b>Interstitial Ad state events</b></summary>

```c#
void OnEnable () {
    ...
    // Executed when the ad is displayed.
    manager.OnInterstitialAdShown += CASInterstitialShown;
    // Executed when the ad is failed to display.
    manager.OnInterstitialAdFailedToShow += CASInterstitialFailedToShow;
    // Executed when the user clicks on an Ad.
    manager.OnInterstitialAdClicked += CASInterstitialClicked;
    // Executed when the ad is closed.
    manager.OnInterstitialAdClosed += CASInterstitialClosed;
}
```
***
</details><details><summary><b>Rewarded Ad state events</b></summary>

```c#
void OnEnable () {
    ...
    // Executed when the ad is displayed.
    manager.OnRewardedAdShown += CASRewardedShown;
    // Executed when the ad is failed to display.
    manager.OnRewardedAdFailedToShow += CASRewardedFailedToShow;
    // Executed when the user clicks on an Ad.
    manager.OnRewardedAdClicked += CASRewardedClicked;
    // Executed when the ad is completed.
    manager.OnRewardedAdCompleted += CASRewardedCompleted;
    // Executed when the ad is closed.
    manager.OnRewardedAdClosed += CASRewardedClosed;
}
```
***
</details>

> **Note:** Do not assume the callbacks are always running on the main thread. Any UI interaction or updates resulting from CAS callbacks need to be passed to the main thread before executing. More information [Execute Events on Unity Thread](#step-2-configuring-cas-sdk).  

## Step 6 Implement our Ad Units
<details><summary><b>Load an Ad</b></summary>

If [LoadingManagerMode](#step-2-configuring-cas-sdk) with automatic loading is active, you can **skip the call to the ad loading methods**.  
Banner Ad allow you to load new ads for each LoadingManagerMode.  
You can get a callback for the successful loading of an ad by subscribe [General Ad availability events](#step-5-implement-cas-events).  

```c#
manager.LoadAd(adType);
```

> **Important!** Once you’ve successfully completed shown your user an Ad. In the case you want to serve another ad, you must repeat laod an ad.  
***
</details><details><summary><b>Check Ad Availability</b></summary>

You can ask for the ad availability directly by calling the following function:
```c#
bool loaded = manager.IsReadyAd(adType);
```
***
</details><details><summary><b>Show Banner ad</b></summary>

Please set Banner Ad Position using the following property, before show:
```c#
manager.bannerPosition = adPosition;
```

To show the ad, call the following method with AdType of ad you want.
```c#
manager.ShowAd(AdType.Banner);
```

To hide Banner Ad from screen, call the following method:
```c#
manager.HideBanner();
```
***
</details><details><summary><b>Show full screen ad</b></summary>

To show the ad, call the following method with AdType of ad you want.
```c#
manager.ShowAd(adType);
```

To further customize the behavior of your ad, you can hook into a number of events in the ad's lifecycle: loading, opening, closing, and so on. Listen for these events by registering a delegate for the appropriate event, as shown [here](#step-5-implement-cas-events).  

> Interstitial and Rewarded ads should be displayed during natural pauses in the flow of an app. Between levels of a game is a good example, or after the user completes a task.  
***
</details>

## Step 7 Include native platforms
The Clever Ads Solutions Unity plugin is distributed with the [EDM4U](https://github.com/googlesamples/unity-jar-resolver) library. This library is intended for use by any Unity plugin that requires access to Android specific libraries (e.g., AARs) or iOS CocoaPods. It provides Unity plugins the ability to declare dependencies, which are then automatically resolved and copied into your Unity project.  

### Include Android

<details><summary><b>Configure Gradle</b></summary>

Using [EDM4U](https://github.com/googlesamples/unity-jar-resolver), you will be able to avoid downloading the Android artifacts into your project. Instead, the artifacts will be added to your gradle file during the compilation. 
To enable this process, follow these steps: 
1. Go to: `Player Settings > Publishing Settings > Build`
2. Select `Custom Main Gradle Template` checkmark to configure dependencies.
3. [Unity 2019.3+] Select `Custom Launcher Gradle Template` checkmark to enable MultiDEX.  
You can read more about MuliDex on the [Android Deleveloper page](https://developer.android.com/studio/build/multidex).  
4. [Unity 2019.3+] Select `Custom Base Gradle Template` checkmark to update Gradle plugin with fix support Android 11.  
You can read more about fix Gradle plugin with support Android 11 on the [Android Deleveloper page](https://android-developers.googleblog.com/2020/07/preparing-your-build-for-package-visibility-in-android-11.html).  
5. [Unity 2019.3+] Select `Custom Gradle Properties Template` to use Jetfier by EDM4U.
6. Go to: `Assets > External Dependency Manager > Android > Settings`  
7. Select `Patch mainTemplate.gradle` checkmark
8. Select `Use Jetfier` checkmark
9. Save your changes, by pressing `OK`

In the Unity editor, select `Assets > External Dependency Manager > Android Resolver > Resolve`. The Unity External Dependency Manager library will append dependencies to `mainTemplate.gradle` of your Unity app.
***
</details><details><summary><b>Update AndroidManifest Permissions</b></summary>

Add the following permissions to your `Assets/Plugins/Android/AndroidManifest.xml` file inside the `<manifest>` tag but outside the `<application>` tag:
```xml
<manifest>
 <uses-permission android:name="android.permission.INTERNET" />
 <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
 <uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
</manifest>
```
#### Optional Permissions
This permission is used for certain ads that vibrate during play. This is a normal level permission, so this permission just needs to be defined in the manifest to enable this ad feature.
```xml
<manifest>
 <uses-permission android:name="android.permission.VIBRATE" />
</manifest>
```
This permission is used for certain ads that allow the user to save a screenshot to their phone. 
Note that with this permission on devices running Android 6.0 (API 23) or higher, this permission must be requested from the user. 
See [Requesting Permissions](https://developer.android.com/training/permissions/requesting) for more details. 
```xml
<manifest>
 <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
</manifest>
```
This permission is not a mandatory permission, however, including it will enable accurate ad targeting.
```xml
<manifest>
 <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
</manifest>
```
Some SDK may require a default permission, so please use the following lines to limit it.
```xml
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" tools:node="remove"/>
```

If you do not find the manifest file [Plugins/Android/AndroidManifest.xml](https://github.com/cleveradssolutions/CAS-Unity-Sample/blob/master/Assets/Plugins/Android/AndroidManifest.xml), you can take it from the example.  
Or Unity 2019.3+ makes it possible to activate in `Player Settings > Publishing Settings > Build > Custom Main Manifest` checkmark.  
***
</details>
<details><summary><b>Google Ads App Android ID (Automated)</b></summary>

**Automated integration during application build.**  
About Google Ads App ID [here](https://developers.google.com/admob/android/quick-start#update_your_androidmanifestxml).  
***
</details>

### Include iOS
Make sure that Cocoapods is installed. 
In the Unity editor, select: `Assets > External Dependency Manager > iOS Resolver > Install Cocoapods`  
 
 <details><summary><b>Supports Unity 2019.3 and newer</b></summary>
 
 If you get the following error while loading your application:
 ```
 Error loading [path]/Your.app/Frameworks/UnityFramework.framework/UnityFramework:  
   dlopen([path]/Your.app/Frameworks/UnityFramework.framework/UnityFramework, 265): 
      Library not loaded: @rpath/CleverAdsSolutions.framework
   Referenced from: [path]/Your.app/Frameworks/UnityFramework.framework/UnityFramework
   Reason: image not found
 ```
 Then please add `target 'Unity-iPhone'` to the Podfile in root folder of XCode project as follows:
 ```cpp
 source 'https://github.com/CocoaPods/Specs.git'
 source 'https://github.com/cleveradssolutions/CAS-Specs.git'
 platform :ios, '11.0'

 target 'UnityFramework' do
   pod 'CleverAdsSolutions-SDK', 'version'
 end

 target 'Unity-iPhone' do
 end
 ```
 Save Podfile and call terminal command
 ```
 cd [path to XCode project]
 pod install --no-repo-update
 ```
 
 > We are working with EDM4U to fix this problem in [issue #405](https://github.com/googlesamples/unity-jar-resolver/issues/405)
 ***
 </details><details><summary><b>Configuring permissions (Optional)</b></summary>

In iOS 10, Apple has extended the scope of its privacy controls by restricting access to features like the camera, photo library, etc. In order to unlock rich, immersive experiences in the SDK that take advantage of these services, please add the following entry to your apps plist:
```xml
<key>NSPhotoLibraryUsageDescription</key>
<string>Some ad content may require access to the photo library.</string>
<key>NSCameraUsageDescription</key>
<string>Some ad content may access camera to take picture.</string>
<key>NSMotionUsageDescription</key>
<string>Some ad content may require access to accelerometer for interactive ad experience.</string>
```
> You can also use the settings provided by the Unity `Player Settings > Other Settings > Usage Description`.  
***
</details>
<details><summary><b>Google Ads App iOS ID (Automated)</b></summary>

**Automated integration during application build.**  
About Google Ads App ID [here](https://developers.google.com/admob/ios/quick-start).  
***
</details>
<details><summary><b>Configuring SK Ad Networks (Automated)</b></summary>

**Automated integration during application build.**  
About SKAdNetwork [here](https://developer.apple.com/documentation/storekit/skadnetwork).  
***
</details>
<details><summary><b>Configuring URL Schemes (Automated)</b></summary>

**Automated integration during application build.**  
About URL Schemes [here](https://github.com/cleveradssolutions/CAS-iOS#step-6-configuring-url-schemes).
***
</details>

## GitHub issue tracker
To file bugs, make feature requests, or suggest improvements for the Unity Plugin SDK, please use [GitHub's issue tracker](https://github.com/cleveradssolutions/CAS-Unity/issues).

## Support
Site: [https://cleveradssolutions.com](https://cleveradssolutions.com)

Technical support: Max  
Skype: m.shevchenko_15  

Network support: Vitaly  
Skype: zanzavital  

mailto:support@cleveradssolutions.com  

## License
The CAS Unity plugin is available under a commercial license. See the LICENSE file for more info.
