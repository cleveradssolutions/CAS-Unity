# Clever Ads Solutions Unity Plugin Change Log

## [3.0.0] - 2023-01-31
### Dependencies
- [Android] Wraps [3.0.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.0.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Added a built-in consent flow to automatically present users with GDPR and Apple ATT dialogue.  
The dialog will not be shown if at least one of the following conditions is true:
  - If `CAS.MobileAds.settings.taggedAudience` equals `Audience.Children`.
  - If `CAS.MobileAds.settings.userConsent` not equals `ConsentStatus.Undefined`.
  - If the user is located in countries that do not require information protection.
  - If ConsentFlow is disabled:
 ```csharp
CAS.MobileAds.BuildManager().WithConsentFlow(
      new ConsentFlow(isEnabled: false)
).Build()
```
- A new `OnAdImpression` event has been added to get a more precise revenue per ad impression than from the ad opening event.
- Update `InterstitialAdObject`, `RewardedAdObject` and `ReturnToPlayAdObject`:
  - `event OnAdClosed` is now called after `event OnAdFailedToShow` automatically.
  - Added `event OnAdImpression` - event with `AdMetaData` and more precise revenue.
  - Added `bool isAdReady` - check ready ad to present.
  - Added `void LoadAd()` - manual load ad.
- Added `double revenue` property to `AdMetaData` to get revenue for each impression.
- Added alternative names for `IManagerBuilder CAS.MobileAds.BuildManager()` methods:
  - `Initialize()` -> `Build()`
  - `WithManagerIdAtIndex(int)` -> `WithCASId(int)`
  - `WithManagerId(string)` -> `WithCASId(string)`
### Changes
- [Android] The minimum Android API level supports is 21.
- [iOS] The minimum iOS version supports is 12.0.
- The list of networks that are included in the Clever solutions has been changed: 
  - Optimal solution: Added Yandex Ads.
  - [Android] Families solution: Removed AppLovin.
  - [iOS] Families solution: Removed AppLovin, Added Yandex Ads.
    > AppLovin no longer has advertising for a children's audience.
- Removed deprecated functions from CAS version 2.x. See [Migrate from 2.x to 3.0](https://github.com/cleveradssolutions/CAS-Unity/wiki/SDK-Migration) page.
- Enabled by default `CAS.MobileAds.settings.allowInterstitialAdsWhenVideoCostAreLower` option.
- The dependency on native CAS SDK is now built-in Unity Package.

## [2.9.9] - 2023-01-12
### Dependencies
- [Android] Wraps [2.9.9 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.9 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.9.8] - 2022-12-12
### Dependencies
- [Android] Wraps [2.9.8 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.8 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Changes
- [iOS] Disable bitcode, as Apple deprecated it in Xcode 14.

## [2.9.7] - 2022-11-25
### Dependencies
- [Android] Wraps [2.9.7 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.7 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.9.6] - 2022-11-8
### Dependencies
- [Android] Wraps [2.9.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.9.5] - 2022-11-1
### Dependencies
- [Android] Wraps [2.9.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.5 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [Android] Fixed build issue with `jcenter()` repository.
- [iOS] Fixed Main Thread Checker issue when app using Firebase Dynamic Links.

## [2.9.4] - 2022-10-18
### Dependencies
- [Android] Wraps [2.9.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [iOS] Fixed the position of banner ad on the screen.
- [iOS] Fixed `WithInitListener()` call.
- [iOS] Fixed disabling Bitcode for UnityFramework.

## [2.9.3] - 2022-10-13
### Dependencies
- [Android] Wraps [2.9.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Improved stability of the `AdMetaData` class to get information about ad impressions.
- Added `IAdView.OnImpression` event with `AdMetaData` structure and invoked only once for each new ad impression.
- Added `impressionDepth` property for the `AdMetaData` structure  to get the amount of impressions of all ad formats to the current user.
- Added `lifetimeRevenue` property for the `AdMetaData` structure  to get the total revenue in USD from impressions of all ad formats to the current user.
- Added new `AdSize.ThinBanner`. Thin banners have a smaller height, taller banners compared to anchored adaptive banners.
- Allowed simultaneous collection of `ad_impression` and `CAS_Impression` analytics events with the same parameters.
- [iOS] Added option to enable build Bitcode in `Ads Settings > Other`. By default disabled.
### Changes
- The minimum iOS version supports is iOS 11.
- The `MobileAds.settings` are saved between sessions.
- The `MobileAds.settings.isExecuteEventsOnUnityThread` is enabled by default.
- The option to use the Medium Rectangle size has been removed from the settings window. Just select the Banner format to use the Medium Rectangle size.
- The `IAdView.rectInPixels` return `Rect.zero` when ad view is not active.
- Plugin caching the value of `IAdView.rectInPixels` on C# side to reduse number of calls to the native side.
- The ID of the current device for `MobileAds.settings.SetTestDeviceIds()` will be printed in the logs even if the `CAS.settings.isDebugMode` is disabled.
- Mediation is no longer used for devices defined in `MobileAds.settings.SetTestDeviceIds()`. List of test devices should be defined before first MediationManager initialized.
- Deprecated `IAdView.OnPresented` and `IAdViwe.OnHidden` events. These events are invoked when the ad view's changes `IAdView.SetActive()`.
- Deprecated `IMediationManager.OnLoadedAd` event in favor of the new separate `IMediationManager.OnInterstitialAdLoaded` and `IMediationManager.OnRewardedAdLoaded` events. 
- Deprecated `IMediationManager.OnFailedToLoadAd` event in favor of the new separate `IMediationManager.OnInterstitialAdFailedToLoad` and `IMediationManager.OnRewardedAdFailedToLoad` events with `AdError` enum. Use `AdError.GetMessage()` to get the error text.

## [2.8.6] - 2022-09-05
### Dependencies
- [Android] Wraps [2.8.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.8.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
[iOS] For iOS 12.2.0 or earlier builds, added `/usr/lib/swift` to `XCode > Build Settings > Runpath Search Paths` to prevent any issues with `libswiftCore.dylib`.
[Android] Added update of the `mainTemplate.gradle` version if the file was created by Unity 2019.2 or older.

## [2.8.5] - 2022-08-08
### Dependencies
- [Android] Wraps [2.8.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.8.5 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- Fixed `AdSize.AdaptiveBanner` in portrait orientation.
- Fixed refresh `AdSize.AdaptiveFullWidth` on orientation changed.
- [iOS] Fixed crash from `[CASUView bannerAdView:willPresent:]` after application closed.
- [Android] Fixed the Banner ad position in center of the screen with cutouts.
- [Editor] Fixed compilation errors for Unity 2017.
  > Unity 2017 is not officially supported and is not recommended for use. Please let us know about the issues and we will try to fix them.

## [2.8.4] - 2022-07-21
### Dependencies
- [Android] Wraps [2.8.4-rc1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.8.4-rc1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Added new `AdSize.AdaptiveFullWidth` to pick the best ad size for full full screen width.  
  > `AdSize.AdaptiveBanner` can pick the ad size for screen width but not more than 728dp.

## [2.8.3] - 2022-07-14
### Dependencies
- [Android] Wraps [2.8.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.8.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Improved algorithm for processing bids with undisclosed prices.
- Improved ad serving algorithm to increase the average revenue of Banner Ads.
- Restored support for Fyber advertising network in closed beta.
- No longer supported `IMediationManager.GetLastActiveMediation(AdType)` feature.
- [Android] Fixed `FormatException` from `AdMetaData.GetDouble()`.
- [iOS] Force disable `ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES` for Framework target. Cause build error when embed for both targets.
### Bug Fixes
- [Android] Fixed Android Proguard issue with `NoSuchMethodError`.
- [iOS] Fixed simultaneous access crash on `BiddingManager.init(_:_:)`.
- [iOS] Fixed index out of range crash. 
- [Editor] Fixed Ads Settings inspector bug.
- Fixed internal reflection issue.

## [2.8.1] - 2022-05-26
### Dependencies
- [Android] Wraps [2.8.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.8.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Features
- Added new property `AdMetaData.creativeIdentifier` to get the creative id tied to the ad, if available.
  > You can report creative issues to our Ad review team using this id.
- Added new property `AdMetaData.identifier` to get internal demand source name in CAS database.
- Added feature to link your CAS app to Google Analytics to automatic measurement of ad revenue. Read more on [our wiki page](https://github.com/cleveradssolutions/CAS-Unity/wiki/Impression-Level-Data#measure-ad-revenue).(beta)
- Added new `AdError.Configuration` when configuration error has been detected in one of the mediation ad networks.
- Added new `CAS.MobileAds.BuildManager().WithUserID(userID)` to set user id. The userID is a unique identifier supplied by your application and must be static for each user across sessions.
- Improved initialization performance.
- [Android] All events from the native CAS SDK are called on a Background Thread instead of the Android UI Thread to avoid ANRs on unity reflection.   
  > Use the `MobileAds.settings.isExecuteEventsOnUnityThread` for all events or `EventExecutor.Add()` for each event to switch to the Unity Thread.
- [Android] The Banner position inside the container now depends on the selected `AdPosition`, if the actual ad size is different.
## Android Proguard issue
If the Proguard is active, then you have `NoSuchMethodError` on device. Fix it by following steps:
1. Enable Custom Proguard file in Player Settings window.
2. Add keep line to the file: `-keep interface com.cleversolutions.ads.android.CAS$ManagerBuilder { *; }` 
## Changes
- ⚠️ [iOS] Updated minimum supported Xcode version to 13.2.1.
- [Android] Removed option to disable `ExoPlayer`. AppLovin added the dependency on ExoPlayer so that it is always used.
- Migration from `CASInitSettings` class to `IManagerBuilder` interface.
## Update Cross promotion
- Added feature to get the `creativeIdentifier`.
- Clicking on ad analytics event will be fired once per impression.
- Clicking on ad skips the video ad.
- Improved banner ads to promote non-app ad.
- [Android] Fixed video restart feature.

## [2.7.3] - 2022-04-22
### Dependencies
- [Android] Wraps [2.7.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.7.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixe
- [Android] Fixed build error `FileNotFoundException: gradleOut.aab` for Unity 2018 with Gradle Wrapper 6.5+.
- [iOS] Fixed XCode Parse issue for `eventName` selector.
- Minor editor logic fixes.

## [2.7.2] - 2022-04-19
### Dependencies
- [Android] Wraps [2.7.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.7.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [Editor] Added option to disabled `Build preprocess` in `CleverAdsSlutions > Android Settings`.
- [Editor] Added `Assets > CleverAdsSolutions > Configure project` menu to manually invoke the configuration project for successful build. The configuration can be called automatically before the build process when option `Build preprocess` enabled.
- [Editor] Disabled error log for unregistered CAS ID when test mode is active.
- [Android] Improved automatic activation of MultiDex and AndroidX.
- [Android] The automatic `Update Gradle Plugin` occurs depending on selected version of the Gradle Wrapper in Unity preferences of External Tools. Added support for Gradle Wrapper 6.5.
- [Android] Added option to disable `Update Gradle Plugin` in `CleverAdsSlutions > Android Settings`.  
  > Without updating the gradle plugin, the build may not succeed due to known issues.
- [Android] Added option to disable `ExoPlayer` in `CleverAdsSlutions > Android Settings` if you only want to use MediaPlayer.
- [Android] Added feature to activate the required Gradle Templates without going to the Player Settings.
- [Android] Added generation `CASPlugin.androidlib/res/xml/meta_network_security_config.xml` to use `android:networkSecurityConfig` in `AndroidManifest.xml` for Meta Audience Network.
- [Android] Disabled declare `JavaVersion.VERSION_1_8` in gradle file. Unity already defines JavaVersion by default.
### Changes
- The `CAS_Fail` event analytic is no longer collected when the `IAdsSettings.analyticsCollectionEnabled` is disabled.
- The `PSV_AdEvent` event analytic is no longer collected. 
  > Contact support if you still want to collect this event.
### Bug Fixe
- [Android] Fixed ProGuard rules for native callbacks.

## [2.7.1] - 2022-04-07
### Dependencies
- [Android] Wraps [2.7.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.7.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [iOS] Added Chartboost support(beta).
### Bug Fixes
- [Editor] Disabled warning about new CAS version on build if `Auto check for CAS updates` option is disabled.

## [2.7.0] - 2022-04-05
### Dependencies
- [Android] Wraps [2.7.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.7.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.170](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.169)
### Features
- [Android] Notches on the screen no longer offset the banner position in the center.
- [Android] Added a dependency on a `com.google.android.exoplayer:exoplayer` to avoid ANR from `android.media.MediaPlayer`. 
  - Also added `android.enableDexingArtifactTransform=false` property to avoid `java.lang.AbstractMethodError` crash from ExoPlayer interface conflict.
### Changes
- Improved error messages while showing ads. Instead of the `No Fill` message, will be `Ad are not ready` or `No internet connection detected`.
- Before closing fullscreen ad no longer fires `OnFailedToLoadAd`. It should be considered that the shown ad cannot be shown again. Wait for a new ad loading notification to show it.
- Banner auto refresh no longer fires `OnFailed` when the current ad is still valid.
- Fixed banner fires `OnLoaded` when Admob auto refresh.
- For a potential increase revenue, we disabled filtering of ineffective floors from the waterfall, after CAS 2.5.0 update.

## [2.6.7] - 2022-03-25
### Dependencies
- [Android] Wraps [2.6.7 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.7 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.6.6] - 2022-03-07
### Dependencies
- [Android] Wraps [2.6.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.6.5] - 2022-02-22
### Dependencies
- [Android] Wraps [2.6.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)

## [2.6.4] - 2022-02-21
### Dependencies
- [Android] Wraps [2.6.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Bug Fixes
- [Editor] Fixed Top Left banner position with ussing `BannerAdObject`.

## [2.6.3] - 2022-01-25
### Dependencies
- [Android] Wraps [2.6.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.169](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.169)
### Features
- [Android] You no longer need to add a dependency on AppSetId because it is always applied.
### Bug Fixes
-  Fixed a bug where the set of the banner position in the Top Center was not applied.

## [2.6.2] - 2022-01-17
### Dependencies
- [Android] Wraps [2.6.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [Android] Automatic deletion of the deprecated `jCenter()` repository to avoid Timeout error when the repository is not responding.
- [Android] Improved automatic gradle file configuration behavior.
### Bug Fixes
- Fixed saving ads settings for Windows Editor.

## [2.6.1] - 2022-01-12
### Dependencies
- [Android] Wraps [2.6.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.6.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- Unity Consent dialog [2.0.0](https://github.com/cleveradssolutions/CAS-Unity-Consent/releases)
### Features
- Added support for [Android 12](https://developer.android.com/about/versions/12) devices for apps targeting API 31.
- Added [CAS.ATTrackingStatus.Request(callback)](https://github.com/cleveradssolutions/CAS-Unity/wiki/Asking-Permissions), which provides a Tracking Authorization request and the Tracking Authorization status of the iOS application.
- Added [Delay measurement of the Google SDK initialization](https://developers.google.com/admob/ump/android/quick-start#delay_app_measurement_optional) option in `CleverAdsSlutions > Settings` window.
- Added option to disable `Auto check for CAS updates` in `CleverAdsSlutions > Settings` window when you would like to check for updates yourself.
- Added option to select `Most popular country of users` in `CleverAdsSlutions > Settings` window to prepare a CAS resources for the selected country..
- [Android] Improved Proguard compatibility. Reduced the SDK size.
- [Android] Added option to disable `Multi DEX` in `Android Settings` window when your app does not require splitting into multiple dex.
- [iOS] Added `Set User Tracking Usage desciption` option in `iOS Settings` to configure localizad description of Tracking Authorization Request.
### Changes
- The CAS no longer affects to Facebook Audience network [Data processing options for Users in California](https://developers.facebook.com/docs/marketing-apis/data-processing-options/) and [iOS Advertising Tracking Enabled](https://developers.facebook.com/docs/audience-network/setting-up/platform-setup/ios/advertising-tracking-enabled)
:warning: You will need to implement the flag irrespective of the use of mediation.  
```cs
CAS.ATTrackingStatus.Request((status) => {
  CAS.MobileAds.BuildManager()
    .WithMediationExtras(MediationExtras.facebookDataProcessing, "LDU_1_1000")
    .WithMediationExtras(MediationExtras.facebookAdvertiserTracking, track ? "1" : "0")
    .Initialize();
});
```
- [Android] With Android 12 many mediation partners are starting to include the `com.google.android.gms.permission.AD_ID` permission.  
:warning: Use new `Remove permission to use Advertising ID` option in `Android Settings` window to remove the permission for all manifests.  
Also, some mediation partners are already working with a new `App set ID` that you can add to your game in the `Android Settings > Advanced Integration` section.
### Banner Ads update
- Improved calculation of the `AdSize.AdaptiveBanner`.
- We had to rethink the structure of banners and banner sizes to make their work clearer and more efficient.
- Each a `AdSize` has its own unique ad view. ~~Single `AdSize` instance for `IMediationManager`~~.
- Added a new `IAdView` interface to manage the single `AdSize` instance.
```cs
IMediationManager manager = MobileAds.BuildManager().Initialize();
IAdView adView = manager.GetAdView( AdSize.MediumRectangle );
```
- Separated the `AdSize.MediumRectangle` from `AdFlags.Banner` format to new `AdFlags.MediumRectangle` format to better configuration of the mediation manager.
### Bug Fixes
- Fixed compilation error with Unity 2021.2 and newer.
- Fixed compilation error with .NET 3.5.
### Update Cross promotion
- Improved stability and performance.
- [Android] Improved detection of installed applications that have cross-promo SDK version 2.6+ on Android 11+. 
- Added load error "Impression cap" when ad creative has reached its daily cap for user.
- Added option of creative to lock rotation of full-screen ads.
- Added option of creative to disable background for full-screen ads.
- Added option of creative to disable banner for full-screen ads.
### Mediation partners update
- The composition of the `Optimal` solution has been changed: + Pangle, + TapJoy, + MyTarget, ~~Kidoz~~, ~~FairBid~~
- [iOS] Create new a Families solution designed for applications aimed at a children's audience.
- Removed support for the following networks: FairBid, Start.IO, Smaato, MoPub 

## [2.5.3] - 2021-09-29
### Dependencies
- [Android] Wraps [2.5.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [iOS] Official release for iOS 15 support.
- [iOS] Improvements in the framework structure to avoid problems with classes implemented in both targets.
- [iOS] Added overwriting of App Transport Security settings in `Info.plist`.
- [iOS] Added recording of all used SDKs to the Podfile.
- [iOS] Added warning when Link frameworks statically disabled.
  > We recommend enabling `Add use_frameworks!` and `Link frameworks statically` found under `Assets -> External Dependency Manager -> iOS Resolver -> Settings` menu.
  > Failing to do this step may result in undefined behavior of the plugin and doubled import of frameworks.
### Bug Fixes
- Fixed a rare error in the name of the cas settings file.
- Fixed native dependency management when moving `Dependencies.xml` files from `Assets/CleverAdsSolutions` folder. 
- [iOS] Fixes for iOS build with Unity 2019.3+ and XCode 13.

## [2.5.2] - 2021-09-07
### Dependencies
- [Android] Wraps [2.5.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Added new mediation partner
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
[Smaato](https://www.smaato.com/) - [Privacy Policy](https://www.smaato.com/privacy/)  
> Available for Android only. Coming soon for iOS.

## [2.5.1] - 2021-08-28
### Dependencies
- [Android] Wraps [2.5.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
### Features
- Only Test Ads are used with the Development Build to avoid changing the Test Ads Mode checkbox.
  > Make sure you disable Development build and use real ad manager ID before publishing your app!
- [Editor] The Test Banner simulation is more similar to real size.
- [Editor] The Test Banenr size and position can be changed in play mode using the inspector.
- [Editor] The application is allowed to build when the server is not available. However, still not recommended and may reduce mediation revenue.
- [Editor] Improved Admob App Id filling.
### Bug Fixes
- [Android] Fixed load the Banner Ads automatically after initialization.
### Changes
- Deprecated `CAS.MobileAds.BuildManager().WithManagerId()` in favor of `Assets>CleverAdsSolutions>Settings` menu and `CAS.MobileAds.BuildManager().WithManagerIdAtIndex()`.
  > Defining Manager Id in the builder is not enough to setup the project correctly.
### Added new mediation partner
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
[Pangle](https://www.pangleglobal.com/) - [Privacy Policy](https://www.pangleglobal.com/privacy/enduser-en)  
> Available for Android only. Coming soon for iOS.

## [2.5.0] - 2021-08-17
### Dependencies
- [Android] Wraps [2.5.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Changes
- Chartboost is no longer supported. At the moment, Chartboost architecture does not allow us to effectively compete with other networks. 
We will keep an eye on changes in the future and look forward to receiving support back.
- Deprecated `CAS.MobileAds.BuildManager().WithTestAdMode(false)` in favor of `Assets>CleverAdsSolutions>Settings` menu.
  > Defining test mode in the builder is not enough to get true test ads.
- Deprecated `CAS.MobileAds.Initialize()` in favor of `BuildManager()`.
  > Since the test ad mode is obsolete, we recommend using the builder with new initialization options.


> Read more release notes in the [Github releases](https://github.com/cleveradssolutions/CAS-Unity/releases)