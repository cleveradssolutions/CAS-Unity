# CleverAdsSolutions-Unity SDK Integration
The Clever Ads Solutions Unity plugin enables Unity developers to easily serve Mobile Ads on Android and iOS apps without having to write Java or Objective-C code. The plugin provides a C# interface for requesting ads that is used by C# scripts in your Unity project.

[![GitHub package.json version](https://img.shields.io/github/package-json/v/cleveradssolutions/CAS-Unity?label=Unity%20Package)](https://github.com/cleveradssolutions/CAS-Unity/releases/latest)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/CleverAdsSolutions/CAS-Android?label=CAS%20Android)](https://github.com/cleveradssolutions/CAS-Android)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/CleverAdsSolutions/CAS-iOS?label=CAS%20iOS)](https://github.com/cleveradssolutions/CAS-iOS)
[![App-ads.txt](https://img.shields.io/endpoint?url=https://cleveradssolutions.com/AppAdsTxtShiled.json)](https://cleveradssolutions.com/app-ads.txt)

## Requirements
### Unity
- Unity Editor version: 2017.4, 2018.4, 2019, 2020.1  
> You can try any version you want, however, non-recommended versions can lead to unexpected errors.
### Android
- Android version 4.4 (API level 19) and up
- Gradle 3.4.3 and up
### iOS
- XCode version 12 and up
- iOS version 10 and up
- Cocoapods applied

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
 6.1 [Load an Ad](#load-an-ad)  
 6.2 [Check Ad Availability](#check-ad-availability)  
 6.3 [Show the ad](#show-the-ad)  
 7.  [Include native platforms](#step-7-include-native-platforms)  
 7.1 [Include Android](#include-android)  
 7.2 [Include iOS](#include-ios)  
 8.  [Adding App-ads.txt file of our partners (Optional)](#step-8-adding-app-ads-txt-file-of-our-partners)  
 9.  [Mediation partners](#mediation-partners)  
 10.  [GitHub issue tracker](#github-issue-tracker)  
 11.  [Support](#support)  
 12.  [License](#license)  

## Step 1 Add the CAS package to Your Project
if you are using Unity 2018.4 or newer then you can add CAS SDK to your Unity project using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html), or you can import the package manually.
### Unity Package Manager
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
    "com.cleversolutions.ads.unity" : "https://github.com/cleveradssolutions/CAS-Unity.git#1.6.9",
    "Other" : "dependencies"
}
}
```
> Note that some other SDKs, such as the Firebase SDK, may contain [EDM4U](https://github.com/googlesamples/unity-jar-resolver) in their .unitypackage. Check if `Assets/ExternalDependencyManager` or `Assets/PlayServicesResolver` folders exist. If these folders exist, remove them before installing any CAS SDK through Unity Package Manager.

### Manual installation
1. Download latest [CleverAdsSolutions.unitypackage](https://github.com/cleveradssolutions/CAS-Unity/releases/latest)
2. In your open Unity project, navigate to **Assets > Import Package > Custom Package**.
3. In the *Import Unity Package* window, make sure all of the files are selected and click **Import**.

## CAS Unity Demo App
The Integration Demo application demonstrate how to integrate the CAS in your app.  
[Repository Unity Sample Application](https://github.com/cleveradssolutions/CAS-Unity-Sample)

## Step 2 Configuring CAS SDK
In your open Unity project, navigate to **Assets > CleverAdsSolutions > Settings** to create and modify build settings.

### Test Ad Mode  
The quickest way to testing is to enable Test Ad Mode. These ad are not associated with your account, so there's no risk of your account generating invalid traffic when using these ad units.

### Manager Ids  
Add your Clever Ads Solutions manager Id's.
> If you haven't created an CAS account and registered an manager yet,  now's a great time to do so at [cleveradssolutions.com](https://cleveradssolutions.com). If you're just looking to experiment with the SDK, though, you can use the Test Ad Mode above.  

### Allowed ads types in app
To improve the performance of your application, we recommend that you only allow ad types that will actually be used in the application. For example: Banner and Interstitial ad.  

The processes of ad types can be disabled/enabled at any time using following method:
```c#
CAS.MobileAds.manager.SetEnableAd(adType, enabled);
```

### Audience Tagged
Choose the audience your game is targeting.   
In addition to targeting ads, on Google Play has restrictions games participate in the family apps program. These games can only serve ads from certified ad networks. [More about Families Ads Program](https://support.google.com/googleplay/android-developer/answer/9283445).  
Changing this setting will change the project dependencies. Please follow the instructions provided in the settings window.

### Banner Size
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

> If you use Manual [Loading Manager Mode](#loading-mode) then please call [Load an Ad](#load-an-ad) each banner size changed.  

You can get the current banner size in pixels at any time using the following methods:
```c#
float height = CAS.MobileAds.manager.GetBannerHeightInPixels();
float width = CAS.MobileAds.manager.GetBannerWidthInPixels();
```

### Banner Refresh rate
An ad unit’s automatic refresh rate (in seconds) determines how often a new ad request is generated for that ad unit.  
> Ad requests should not be made when the device screen is turned off.  
We recomended using refresh rate 30 seconds. However, you can choose any value you want longer than 10 seconds.  

Change the banner automatic refresh rate using the following method:
```c#
CAS.MobileAds.settings.bannerRefreshInterval = refreshInterval;
```

### Interstitial impression interval
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

### Loading mode
Select CAS mediation processing mode of ad requests.
|        Mode        |  Load*  | Impact on App performance | Memory usage |        Actual ads*       |
|:------------------:|:------:|:-------------------------:|:------------:|:------------------------:|
|   FastestRequests  |  Auto  |         Very high         |     High     |       Most relevant      |
|    FastRequests    |  Auto  |            High           |    Balance   |      High relevance      |
|  Optimal(Default)  |  Auto  |          Balance          |    Balance   |          Balance         |
|   HighPerformance  |  Auto  |            Low            |      Low     |       Possible loss      |
| HighestPerformance |  Auto  |          Very low         |      Low     |       Possible loss      |
|       Manual      | Manual |          Very low         |      Low     | Depends on the frequency |

> Actual ads column* - Potential increase in revenue by increasing the frequency of ad requests. At the same time, it greatly affects the performance of the application.   

> Load column*  
> Auto control load mediation ads starts immediately after initialization and will prepare displays automatically.  
> Manual control loading mediation ads requires manual preparation of advertising content for display. Use ad loading method before trying to show: `CAS.MobileAds.manager.LoadAd()`  

Change the Clever Ads Solution processing mode using the following method:
```c#
CAS.MobileAds.settings.loadingMode = mode;
```

### Debug mode
The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdc with tag `CAS`.  
Disabling Debug Mode may improve application performance.  

Change the Debug Mode flag at any time using the following method:
```c#
CAS.MobileAds.settings.isDebugMode = enabled;
```
Disabled by default.

### Cross Promotion enabled
Cross promotion is an app marketing strategy in which app developers promote one of their titles on another one of their titles. Cross promoting is especially effective for developers with large portfolios of games as a means to move users across titles and use the opportunity to scale each of their apps. This is most commonly used by hyper-casual publishers who have relatively low retention, and use cross promotion to keep users within their app portfolio.  

Start your cross promotion campaign with CAS [here](https://cleveradssolutions.com).

> Changing this flag will change the project dependencies. Please use **Assets > External Dependency Manager > Android Resolver > Resolve** after the change.  

### Analytics Collection
If your application uses Google Analytics (Firebase) then Clever Ads Solutions collects ad impressions and states analytics.  
**This flag has no effect on ad revenue.**  
Disabling analytics collection may save internet traffic and improve application performance.  

Change the analytics collection flag at any time using the following method:
```c#
CAS.MobileAds.settings.analyticsCollectionEnabled = enabled;
```
Disabled by default.  

### Muted Ads Sounds
Sounds in ads mute state.  
```c#
CAS.MobileAds.settings.isMutedAdSounds = mute;
```
Disabled by default.  

### Execute Events On Unity Thread
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

### Test Device Ids
Identifiers corresponding to test devices which will always request test ads.
The test device identifier for the current device is logged to the console when the first ad request is made.
```c#
CAS.MobileAds.settings.SetTestDeviceIds(testDeviceIds);
```

### Allow Interstitial Ads When Video Cost Are Lower
This option will compare ad cost and serve regular interstitial ads when rewarded video ads are expected to generate less revenue.  
Interstitial Ads does not require to watch the video to the end, but the OnRewardedAdCompleted callback will be triggered in any case.  

Change the flag at any time using the following method:
```c#
CAS.MobileAds.settings.allowInterstitialAdsWhenVideoCostAreLower = allow;
```
Disabled by default.  

### Last Page Ad
The latest free ad page for your own promotion.  
This ad page will be displayed when there is no paid ad to show or internet availability.  

**Attention!** Impressions and clicks on this ad page don't make money.  

Change the `LastPageAdContent` at any time using the following method:
```c#
CAS.MobileAds.manager.lastPageAdContent = new LastPageAdContent(...);
```
By default, this page will not be displayed while the ad content is NULL.  

### iOS Location Usage Description
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

### iOS App Pause On Background
**Property for iOS only.**  
Indicates if the Unity app should be automatically paused when a full screen ad (Interstitial or Rewarded video ad) is displayed.
```c#
CAS.MobileAds.settings.iOSAppPauseOnBackground = pause;
```
Enabled by default.

## Step 3 Privacy Laws
This documentation is provided for compliance with various privacy laws. If you are collecting consent from your users, you can make use of APIs discussed below to inform CAS and all downstream consumers of this information.  

A detailed article on the use of user data can be found in the [Privacy Policy](https://github.com/cleveradssolutions/CAS-Android/wiki/Privacy-Policy).

### GDPR Managing Consent
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

### CCPA Compliance
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

### COPPA and EEA Compliance
This documentation is provided for additional compliance with the [Children’s Online Privacy Protection Act (COPPA)](https://www.ftc.gov/tips-advice/business-center/privacy-and-security/children%27s-privacy). Publishers may designate all inventory within their applications as being child-directed or as COPPA-applicable though our UI. Publishers who have knowledge of specific individuals as being COPPA-applicable should make use of the API discussed below to inform CAS and all downstream consumers of this information.  

You can mark your ad requests to receive treatment for users in the European Economic Area (EEA) under the age of consent. This feature is designed to help facilitate compliance with the General Data Protection Regulation (GDPR). Note that you may have other legal obligations under GDPR. Please review the European Union’s guidance and consult with your own legal counsel. Please remember that CAS tools are designed to facilitate compliance and do not relieve any particular publisher of its obligations under the law.

Call `Audience.children` indicate that user want get content treated as child-directed for purposes of COPPA or receive treatment for users in the European Economic Area (EEA) under the age of consent. 
```c#
CAS.MobileAds.settings.taggedAudience = Audience.Children)
```
Call `Audience.notChildren` to indicate that user **don't** want get content treated as child-directed for purposes of COPPA or **not** receive treatment for users in the European Economic Area (EEA) under the age of consent.
```c#
CAS.MobileAds.settings.taggedAudience = Audience.NotChildren)
```
By default, the audience is unknown and the mediation ad network will work as usual. For reset state:
```c#
CAS.MobileAds.settings.taggedAudience = Audience.Mixed
```

## Step 4 Initialize CAS SDK
Before loading ads, have your app initialize the CAS SDK by calling `CAS.MobileAds.Initialize()` which initializes the SDK and calls back a completion listener once initialization is complete. 
Initialize can be called for different identifiers to create different managers (Placement).
This needs to be done only once for each manager, ideally at app launch.

To initialize SDK using settings from resources, created by menu `Assets/CleverAdsSolutions/Settings`, there is the following method:
```c#
CAS.MobileAds.InitializeFromResources(
    managerIndex: 0,
    initCompleteAction: (success, error) => { 
        // CAS manager initialization done  
    });
```

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
            testAdMode: true,
            // Optional subscribe to initialization done
            initCompleteAction: (success, error) => { 
                // CAS manager initialization done  
            });
    }
}
```

## Step 5 Implement CAS Events
The CAS Unity Plugin fires several events to inform you of ad availability and states.

#### General Ad availability events
```c#
void OnEnable () {
    ...
    // Executed when AdType load ad response
    manager.OnLoadedAd += CASLoaded;
    // Executed when AdType failed to load ad response with error message
    manager.OnFailedToLoadAd += CASFailedToLoad;
}
```

#### Banner Ad state events
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
#### Interstitial Ad state events
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
#### Rewarded Ad state events
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

> **Note:** Do not assume the callbacks are always running on the main thread. Any UI interaction or updates resulting from CAS callbacks need to be passed to the main thread before executing. More information [here](#execute-events-on-unity-thread).  

## Step 6 Implement our Ad Units
### Load an Ad
If [LoadingManagerMode](#loading-mode) with automatic loading is active, you can **skip the call to the ad loading methods**.  
Banner Ad allow you to load new ads for each [LoadingManagerMode](#loading-mode).  
You can get a callback for the successful loading of an ad by subscribe [General Ad availability events](#general-ad-availability-events).  

```c#
manager.LoadAd(adType);
```

> **Important!** Once you’ve successfully completed shown your user an Ad. In the case you want to serve another ad, you must repeat laod an ad.  

### Check Ad Availability
You can ask for the ad availability directly by calling the following function:
```c#
bool loaded = manager.IsReadyAd(adType);
```

### Show the ad
Please set Banner Ad Position using the following property, before show:
```c#
manager.bannerPosition = adPosition;
```

To show the ad, call the following method with AdType of ad you want.
```c#
manager.ShowAd(adType);
```

To hide Banner Ad from screen, call the following method:
```c#
manager.HideBanner();
```

To further customize the behavior of your ad, you can hook into a number of events in the ad's lifecycle: loading, opening, closing, and so on. Listen for these events by registering a delegate for the appropriate event, as shown [here](#banner-ad-state-events).  

> Interstitial and Rewarded ads should be displayed during natural pauses in the flow of an app. Between levels of a game is a good example, or after the user completes a task.  

## Step 7 Include native platforms
The Clever Ads Solutions Unity plugin is distributed with the [EDM4U](https://github.com/googlesamples/unity-jar-resolver) library. This library is intended for use by any Unity plugin that requires access to Android specific libraries (e.g., AARs) or iOS CocoaPods. It provides Unity plugins the ability to declare dependencies, which are then automatically resolved and copied into your Unity project.  

### Include Android
#### Configure Gradle
Using [EDM4U](https://github.com/googlesamples/unity-jar-resolver), you will be able to avoid downloading the Android artifacts into your project. Instead, the artifacts will be added to your gradle file during the compilation. 
To enable this process, follow these steps: 
1. Go to: `Player Settings > Publishing Settings > Build`
2. Select `Custom Main Gradle Template` checkmark
3. Go to: `Assets > External Dependency Manager > Android > Settings`  
4. Select `Patch mainTemplate.gradle` checkmark
4. Select `Use Jetfier` checkmark
5. Save your changes, by pressing `OK`

In the Unity editor, select `Assets > External Dependency Manager > Android Resolver > Resolve`. The Unity External Dependency Manager library will append dependencies to `mainTemplate.gradle` of your Unity app.

#### Support of Unity 2019.3+ 
Since Unity 2019.3+ the Gradle build system has been redesigned and this requires some additional steps:  
1. Go to: `Player Settings > Publishing Settings > Build`  
2. Select `Custom Launcher Gradle Template` checkmark to enable MultiDEX.  
> You can read more about MuliDex on the [Android Deleveloper page](https://developer.android.com/studio/build/multidex).  
3. Select `Custom Base Gradle Template` checkmark to update Gradle plugin with fix support Android 11.  
> You can read more about fix Gradle plugin with support Android 11 on the [Android Deleveloper page](https://android-developers.googleblog.com/2020/07/preparing-your-build-for-package-visibility-in-android-11.html).  
4. Select `Custom Gradle Properties Template` to use Jetfier by EDM4U.  

#### Update AndroidManifest Permissions
Add the following permissions to your `Assets/Plugins/Android/AndroidManifest.xml` file inside the `manifest` tag but outside the tag `application`:

```xml
<manifest>
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
   
    <!--Optional Permissions-->
    
    <!--This permission is used for certain ads that vibrate during play. 
    This is a normal level permission, so this permission just needs to be defined in the manifest to enable this ad feature.-->
    <uses-permission android:name="android.permission.VIBRATE" />
    
    <!--This permission is used for certain ads that allow the user to save a screenshot to their phone. 
    Note that with this permission on devices running Android 6.0 (API 23) or higher, 
    this permission must be requested from the user. 
    See Requesting Permissions for more details. https://developer.android.com/training/permissions/requesting -->
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
    
    <!--This permission is not a mandatory permission, however, including it will enable accurate ad targeting-->
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    ...
</manifest>
```
If you do not find the manifest file [Plugins/Android/AndroidManifest.xml](https://github.com/cleveradssolutions/CAS-Unity-Sample/blob/master/Assets/Plugins/Android/AndroidManifest.xml), you can take it from the example.  
Or Unity 2019.3+ makes it possible to activate in `Player Settings > Publishing Settings > Build > Custom Main Manifest` checkmark.  

Some SDK may require a default permission, so please use the following lines to limit it.
```xml
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" tools:node="remove"/>
```

#### Google Ads App Android ID
**Automated integration during application build.**  
About Google Ads App ID [here](https://github.com/cleveradssolutions/CAS-Android#admob-app-id).  

### Include iOS
Make sure that Cocoapods is installed. 
In the Unity editor, select: `Assets > External Dependency Manager > iOS Resolver > Install Cocoapods`  

[Request App Tracking Transparency authorization](#ios-location-usage-description)  

#### Configuring App Transport Security
With the release of iOS 9 Apple introduced ATS, which requires apps to make secure network connections via SSL and enforces HTTPS connections through its requirements on the SSL version, encryption cipher, and key length. At this time, CAS highly recommends disabling ATS in your application. Please note that, while CAS fully supports HTTPS, some of our advertisers and 3rd party ad tracking providers do not. Therefore enabling ATS may result in a reduction in fill rate.

From the options mentioned below, please choose either option for seemless ad delivery and monetization.

In order to prevent your ads (and your revenue) from being impacted by ATS, please disable it by adding the following to your info.plist:

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsArbitraryLoads</key>
    <true/>
    <key>NSAllowsArbitraryLoadsForMedia</key>
    <true/>
    <key>NSAllowsArbitraryLoadsInWebContent</key>
    <true/>
</dict>
```

The `NSAllowsArbitraryLoads` exception is required to make sure your ads are not impacted by ATS on iOS 9 devices, while `NSAllowsArbitraryLoadsForMedia` and `NSAllowsArbitraryLoadsInWebContent` are required to make sure your ads are not impacted by ATS on iOS 10 and later devices.

#### Configuring SK Ad Networks
**Automated integration during application build.**  
About SKAdNetwork [here](https://github.com/cleveradssolutions/CAS-iOS#step-4-configuring-sk-ad-networks).  

#### Configuring URL Schemes
**Automated integration during application build.**  
About URL Schemes [here](https://github.com/cleveradssolutions/CAS-iOS#step-6-configuring-url-schemes).

#### Configuring Optional permissions
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

#### Google Ads App iOS ID
**Automated integration during application build.**  
About Google Ads App ID [here](https://github.com/cleveradssolutions/CAS-iOS#step-7-google-ads-app-id).  

## Step 8 Adding App-ads txt file of our partners  
### "App-ads.txt: How to Make It & Why You Need It"

Last year, the ad tech industry struck back at one of its most elusive problems — widespread domain spoofing that let unauthorized developers sell premium inventory they didn’t actually have. The solution? Over two million developers adopted ads.txt — a simple-text public record of Authorized Digital Sellers for a particular publisher’s inventory — to make sure they didn’t lose money from DSPs and programmatic buyers who avoid noncompliant publishers. Thanks to buyers’ ability to [crawl ads.txt and verify seller authenticity](https://iabtechlab.com/ads-txt-about/), this has quickly become a standard for protecting brands. Ad fraud reduced by 11% in 2019 due to these efforts and publisher’s ability to implement more fraud prevention techniques.  

The time has come for ads.text to evolve in-app. The introduction of apps-ads.txt is an important method for mobile app devs to similarly eliminate fraud and improve transparency.

### What is app-ads.txt?

Like ads.txt, apps-ads.txt is a text file that app devs upload to their publisher website. It lists all ad sources authorized to sell that publisher’s inventory. [The IAB created a system](https://iabtechlab.com/press-releases/app-ads-txt-released-for-public-comment-as-next-step-to-fight-digital-advertising-inventory-fraud/) that allows buyers to distinguish the authorized sellers for specific in-app inventory, weeding out the undesirables.

### How does app-ads.txt work for mobile apps?

A DSP wanting to bid on an app’s inventory crawls the app-ads.txt file on a developer’s website to verify which ad sources are authorized to sell that app’s inventory. The DSP will only accept bid requests from ad sources listed on the file and authorized by the app developer.

### How does app-ads.txt help mobile app developers capture more ad revenue?

**Authorized in-app inventory**. An ever-increasing amount of brands are looking to advertise in-app today. Brand buyers now rely on an adherence to app-ads.txt to make sure they don’t buy unauthorized inventory from app developers and negatively impact campaign performance. Developers who don’t implement app-ads.txt can be removed from any brand buyer’s target media list. That’s why joining the app-ads.txt movement is crucial for publishers to maintain their revenue.

**Ad fraud prevention**. App-ads.txt blocks unauthorized developers who impersonate legitimate apps and mislead DSPs into spending brand budgets on fake inventory. With fraud instances minimized, authentic developers can retain more of the ad revenue from inventory genuinely targeted to their app.

### How do I create an app-ads.txt?

You must list your **Developer Website URL** in the GooglePlay and iTunes app stores. There must be a valid developer website URL in all app stores hosting your apps.

Make sure that your publisher website URL (not app specific URL)  is added in your app store listings. Advertising platforms will use this site to verify the app-ads.txt file.

We have made it easier for you to include CAS list of entries so that don’t have to construct it on your own. Please copy and paste the following text block and include in your txt file along with entries you may have from your other monetization partners:  

**[App-ads.txt](https://cleveradssolutions.com/app-ads.txt)**

## Mediation partners
* [Admob](https://admob.google.com/home)  
* [AppLovin](https://www.applovin.com)  
* [Chartboost](https://www.chartboost.com)  
* [KIDOZ](https://kidoz.net)  
* [UnityAds](https://unity.com/solutions/unity-ads)  
* [Vungle](https://vungle.com)  
* [AdColony](https://www.adcolony.com)  
* [StartApp](https://www.startapp.com)  
* [SuperAwesome](https://www.superawesome.com)  
* [IronSource](https://www.ironsrc.com)  
* [InMobi](https://www.inmobi.com)  
* [Facebook Audience](https://www.facebook.com/business/marketing/audience-network)  
* [Yandex Ad](https://yandex.ru/dev/mobile-ads)  

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
The CAS iOS-SDK is available under a commercial license. See the LICENSE file for more info.

