# Clever Ads Solutions Unity Plugin Change Log

## [2.9.2] - 2022-09-06
### Dependencies
- [Android] Wraps [2.9.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.9.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Now `AdsSettings` are saved between sessions.
- Improved stability of the `AdMetaData` class to get information about ad impressions.
- The option to use the Medium Rectangle size has been removed from the settings window. Just select the Banner format to use the Medium Rectangle size.
- The `IAdView.rectInPixels` return `Rect.zero` when ad view is not active.
- Caching the value of `IAdView.rectInPixels` on C# side to reduse number of calls to the native side.
- Mediation is no longer used for devices defined in `MobileAds.settings.SetTestDeviceIds()`. List of test devices should be defined before first MediationManager initialized.
- The ID of the current device for `MobileAds.settings.SetTestDeviceIds()` will be printed in the logs even if the `CAS.settings.isDebugMode` is disabled.
- Simultaneous collection of `ad_impression` and `CAS_Impression` analytics events with the same parameters is allowed.
- [iOS] Added option to disable build Bitcode in `Ads Settings > Other`. By default disabled.
### Changes
- The minimum iOS version supports is iOS 11.
- Now `MobileAds.settings.isExecuteEventsOnUnityThread` is enabled by default.
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

## [2.4.1] - 2021-08-03
### Dependencies
- [Android] Wraps [2.4.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [Android] Fixed update gradle version before build.

## [2.4.0] - 2021-07-05
### Dependencies
- [Android] Wraps [2.4.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Added alternative events `OnBannerAdOpening, OnInterstitialAdOpening, OnRewardedAdOpening` with ad display metadata.
```csharp
manager.OnInterstitialAdOpening += ( metadata ) =>
{
    if (metadata.priceAccuracy == PriceAccuracy.Undisclosed)
    {
        Debug.Log( "Begin impression " + metadata.type + " ads with undisclosed cost from " + metadata.network );
    }
    else
    {
        string accuracy = metadata.priceAccuracy == PriceAccuracy.Floor ? "a floor" : "an average";
        Debug.Log( "Begin impression " + metadata.type + " ads with " + accuracy + " cost of " + metadata.cpm + " CPM from " + metadata.network );
    }
};
```
- The [ReturnToPlayAdObject](https://github.com/cleveradssolutions/CAS-Unity/wiki/Return-To-Play-Ad-Object) component has been added, which allows displaying ads when returning to the game without using scripts.
- [iOS] Added check for minimum deployment version of iOS 10.0 before build.
### Bug Fixes
- [iOS] Fixed bug with ReturnToPlayAd.
- [Editor] Fixed bug when building with `-batchmode` and disabled ads for the application.

## [2.3.0] - 2021-06-11
### Dependencies
- [Android] Wraps [2.3.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.3.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Added the ability to enable automatic display of [interstitial ads for users who return to the open application](https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Return-Ads).
### Changes
> Ads Solution names have been changed to match terms with native platforms.
- [Android] The `NotChildrenAds` solution has been renamed to `OptimalAds` solutions.
- [Android] Removed the `MixedAudience` solution in favor of `OptimalAds` solutions.
- [iOS] The `Recomended` solution has been renamed to `OptimalAds` solutions.
- [iOS] Removed the `Main` solution in favor of `OptimalAds` solutions.
### Bug Fixes
- [Android] Fixed an issue due to which sometimes the banner ads was not visible on the screen.
- [iOS] Fixed duplication of Google Utilities framework using Firebase and Unity 2019.4+.

## [2.2.4] - 2021-05-31
### Dependencies
- [Android] Wraps [2.2.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.2.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Changes
- Yandex Ads removed from the recommended solutions as it supports banner ads only.
- Mintegral added to the recommended solutions.
> Recommended solutions for Android is `MixedAudience` and `NotChildrenAds`.

## [2.2.3] - 2021-05-20
### Dependencies
- [iOS] Wraps [2.2.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.2.2] - 2021-05-18
### Dependencies
- [Android] Wraps [2.2.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.2.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Features
- [Editor] Improved prebuild operations with [-batchmode](https://docs.unity3d.com/Manual/CommandLineArguments.html).
- Reduced memory usage throughout the entire life cycle.
> ⭐ Our main goal to current update was to reduce the amount of memory used. 
> We have done a great job optimizing various aspects of our SDK. It should reduce the number of ANR and make CAS work more stable.
## Changes
- [iOS 14.5 and SKAdNetwork 2.2](https://developer.apple.com/news/?id=ib31uj1j) including view through attribution. **Bumped minimum Xcode version to 12.5.** :warning: 
- [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver#external-dependency-manager-for-unity) is no longer distributed with the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).  
You should import the latest [external-dependency-manager.unitypackage](https://github.com/googlesamples/unity-jar-resolver/releases) into your project to use the third party SDK correctly.
> For more context, see [firebase/quickstart-unity#1030](https://github.com/firebase/quickstart-unity/issues/1030#issuecomment-825095383)
## Added new mediation partner
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [Mintegral](https://www.mintegral.com) - [Privacy Policy](https://www.mintegral.com/en/privacy/)  
## Removed support for the following networks
- The [Amazon Mobile Ads](https://developer.amazon.com/docs/mobile-ads/mb-overview.html) Network will be disabled on July 15, 2021.
- The **MobFox** Mobile SDK has been deprecated and no longer conforms to the new global policies.
- The **Verizon Media** is currently not a priority for further compatibility support.
- The **Fyber Marketplace** is no longer supported in favor of bidding with **Fyber FairBid**.

## [2.1.7] - 2021-04-21
### Dependencies
- [Android] Wraps [2.1.7 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.1.7 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes from Android native 2.1.7
- Downgrade Kidoz version to 8.9.0 for fixing ANR's

## [2.1.6] - 2021-04-17
### Dependencies
- [Android] Wraps [2.1.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.1.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.1.5] - 2021-04-15
### Dependencies
- [Android] Wraps [2.1.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.1.5 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [2.1.4] - 2021-04-06
### Dependencies
- [Android] Wraps [2.1.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.1.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Now it is possible to test advertisements in the Unity Editor using the Standardone taregt build.
- Audience Network migrate to Bidding with FairBid. Read more on [Audience Network blog post](https://www.facebook.com/audiencenetwork/news-and-insights/audience-network-to-become-bidding-only-beginning-with-ios-in-2021/).
### Changes
- [FairBid] Included with Android `MixedAudience`, `NotChildrenAds` and iOS `Recommended` solutions.
- [Fyber] Has been replaced with an enhanced version of FairBid to use bidding.
### Added new partner mediation
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [FairBid](https://www.fyber.com) - 13.3.0

## [2.1.3] - 2021-03-30
### Dependencies
- [iOS] Wraps [2.1.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes from iOS native 2.1.3
- Fixed invoke `OnBannerAdClicked`, `OnInterstitialAdClicked`, `OnRewardedAdClicked`, `OnRewardedAdCompleted` events.
- Fixed multiple call `OnBannerAdHidden`, `OnInterstitialAdClosed`, `OnRewardedAdClosed` events per impression.

## [2.1.2] - 2021-03-25
### Dependencies
- [Android] Wraps [2.1.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.1.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [Android] Optimizing Android UI Thread.
- [iOS] Update SKAdNetwork ID's.
- Added `SetAdPositionEnumIndex(int)` and `SetAdSizeEnumIndex(int)` methods to `BannerAdObject` for setting the banner position and size by index in enums from the inspector.
- [Editor] Added a button in CAS Settings to update all native dependencies.
- [Editor] Added links to the wiki in the inspector.
### Changes
- Test Ad Mode requires the definition of the manager ID. In this case, the manager identifier can be any string, for example `demo`.
- [iOS] The build property `ENABLE_BITCODE` does not change anymore.
### Bug Fixes
- [Editor] Fixed `ArgumentNullException` on Standalone platform.
- [Editor] Fixed ads callbacks in Unity Editor without calling the `EventExecutor.Initialize()` method. You still need to call initialization to use `EventExecutor`.

### Added new optional third party mediation
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [Fyber](https://www.fyber.com) - 7.8.2

## [2.0.1] - 2021-03-09
### Dependencies
- [Android] Wraps [2.0.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.0.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [iOS] Update SKAdNetwork ID's.
### Bug Fixes
- [iOS] Default CAS settings file in XCode.
### Added new optional third party mediation
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [Tapjoy](https://www.tapjoy.com) - 12.7.1
> Please contact us if you intend to use any additional third party mediation.

## [2.0.0] - 2021-03-02
### Dependencies
- [Android] Wraps [2.0.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.0.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.164](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.164)
### Features
- Added AdObject components to implement ads by unity inspector whit out custom scripts.
- Added Analytics collection toggle to CAS Settings window.
### Changes 
- [Android] Banner ad is now rendered only in the safe area of the screen.  
> Be careful, this change may offset the banner ad by the size of cutout screen.
- [Android] StartApp removed from [Families ads program](https://support.google.com/googleplay/android-developer/answer/9283445) integration with FamiliesAds solution.
- [Android] MoPub is moving from beta to integration with NotChildrenAds solution.
- [iSO] Removed [App Tracking Transparency request](https://developer.apple.com/documentation/apptrackingtransparency) and `AppTrackingTransparency` dependency from CAS Unity plguin.  
> You can still use this functionality in the new plugin [CAS Unity Consent](https://github.com/cleveradssolutions/CAS-Unity-Consent).
- [iOS] MoPub is moving from beta to integration with Recomended solution.
### Bug Fixes
- [iOS] Compilation error on Unity 2019+.

## [1.9.9] - 2021-02-05
### Dependencies
- [Android] Wraps [1.9.9 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.9 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [iOS] Crash on use `MediationExtras`.
- [iOS] Exception on build with Cross promotion and Unity 2019+.
- [MoPub] Initialization error.

## [1.9.8] - 2021-02-04
### Dependencies
- [Android] Wraps [1.9.8 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.8 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [1.9.7] - 2021-01-28
### Dependencies
- [Android] Wraps [1.9.7 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.7 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Migrate Clever Ads Solutions Privacy policy to https://cleveradssolutions.com/privacy-policy
### Bug Fixes
- [iOS] Fix wrong warning message `You have activated the collection of advertising analytics`.

## [1.9.6] - 2021-01-27
### Dependencies
- [Android] Wraps [1.9.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [CrossPromotion] Added support for new functionality to override ad destination link using a web page. In the future, we will detail these possibilities when we have everything ready.  
- [CrossPromotion] Added a new meta flag `CAS.MediationExtras.crossPromoEndless` to disable endless display ad. 
- [MyTarget] Added a new meta flags `CAS.MediationExtras.myTargetGDPRConsent` and `CAS.MediationExtras.myTargetCCPAOptedOut` to override GDPR and CCPA status. 
- [Yandex Ads] Added a new meta flags `CAS.MediationExtras.yandexAdsGDPRConsent` to override GDPR status. 
- [Editor] Implemented `CAS.MobileAds.GetActiveNetworks()` in Editor to get list of active network dependencies.
### Bug Fixes
- [Editor] Fix renderer progress bar of check for updates on open `Assets > CleverAdsSolutions > Settings` window.

## [1.9.5] - 2021-01-25
### Dependencies
- [Android] Wraps [1.9.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.5 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [1.9.4] - 2021-01-20
### Dependencies
- [Android] Wraps [1.9.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Changes
- Chartboost has been removed from all solutions, however you can still integrate it at will with advanced integration.
- [Android] Added `com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT` to delay app measurement until Google Ads initialize.

## [1.9.3] - 2021-01-19
### Dependencies
- [Android] Wraps [1.9.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [Android] Fixed StartApp advanced dependency.

## [1.9.2] - 2021-01-05
### Dependencies
- [Android] Wraps [1.9.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Added new optional third party mediation
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [MoPub](https://www.mopub.com/en) - 5.15.0
> Please contact us if you intend to use any additional third party mediation.

## [1.9.1] - 2020-12-28
### Dependencies
- [Android] Wraps [1.9.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.9.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Added `CAS.MobileAds.BuildManager().Initialize()` [builder](https://github.com/cleveradssolutions/CAS-Unity#step-4-initialize-cas-sdk) to initialize the `IMediationManager` for alternative to `CAS.MobileAds.Initialize(...)` method.
- Added [Mediation extras](https://github.com/cleveradssolutions/CAS-Unity#mediation-extras) options.
- [iOS] Added `CAS.iOS.AppTrackingTransparency` class that provides a [tracking authorization request](https://github.com/cleveradssolutions/CAS-Unity#include-ios).
### Changes
- Migrated Cross-promotion dependency from `Advanced Integration` to `Solutions` section.
- `CAS.MobileAds.InitializeFromResources()` is obsolete in favor of `BuildManager()`.
- [iOS] `SKAdNetworkItems` are added to end of the Plist array instead of replacing all items in the array.
### Bug Fixes
- [iOS] `Library not loaded` while laoding application with Unity 2019.3+. The `Unity-iPhone` pod target is added automatically after build.
- [Android] `java.lang.NoSuchMethodError` when call `CAS.MobileAds.ValidateIntegration()`
- [Editor] Select the correct CAS Settings asset from resources by the build target platform.

## [1.8.3] - 2020-12-18
### Dependencies
- [Android] Wraps [1.8.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.8.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Bug Fixes
- [Facebook AN] Fix CCPA DoNotSell filter to increase ad filling. CAS incorrectly assigned all users from California. More about [California Consumer Privacy Act (CCPA) Compliance](https://developers.facebook.com/docs/audience-network/guides/ccpa/)

## [1.8.2] - 2020-12-16
### Dependencies
- [Android] Wraps [1.8.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.8.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.163](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.163)
### Features
- [iOS] Added more flexibility in advanced dependency integration.

## [1.8.1] - 2020-12-07
### Dependencies
- [Android] Wraps [1.8.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
### Changes
- The `IsAdReady` check now takes into account the interval between Interstitial ad impressions.
### Bug Fixes
- [Android] Lost point to AdCallback of Interstitial and Rewarded ad.

## [1.8.0] - 2020-12-07
### Dependencies
- [Android] Wraps [1.8.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [1.8.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [iOS] Add [Validate of native Integration](https://github.com/cleveradssolutions/CAS-Unity#step-4-initialize-cas-sdk) in runtime. Find log information by tag: `CASIntegrationHelper`
- [Android] Add write all promotion bundles of Cross Promo to manifest queris to support Android 11 package info.
### Bug Fixes
- [Android] No such proxy method of `OnInitializationListener` for Unity 2019.4 and newer.

## [1.7.2] - 2020-11-24
### Dependencies
- Wraps [iOS 1.7.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.162](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.162)
### Features
- Add version checking of dependencies before building the application.

## [1.7.1] - 2020-11-18
### Dependencies
- Wraps [Android 1.7.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- Wraps [iOS 1.7.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Add Advanced Integration to the `Assets > CleverAdsSolutions > Settings` window.  
CAS support partial integration of the third party mediation sdk you really need.  
To do this, use any combination of partial dependencies. No additional code is required for each partner network.
> Please contact us if you intend to use any additional third party mediation.
- Add targeting options to inform our servers of the users details:
```csharp
CAS.MobileAds.targetingOptions.gender = CAS.Gender.Male;
CAS.MobileAds.targetingOptions.age = 12;
```
- [iOS] Add `trackLocationEnabled` to enable automatically collects location data if the user allowed the app to track the location.
```csharp
CAS.MobileAds.settings.trackLocationEnabled = true;
```
- [iOS] Optimization of event calling in main thread.
- [Android] Add [Validate of native Integration](https://github.com/cleveradssolutions/CAS-Android#step-7-verify-your-integration) in runtime. Find log information by tag: `CASIntegrationHelper`
```csharp
CAS.MobileAds.ValidateIntegration();
```
> Once you’ve successfully verified your integration, please remember to remove the integration helper from your code.
- [Android] Add Resolve Android Dependencies by EDM4U button to the `Assets > CleverAdsSolutions > Adnroid Settings`.
> Changin dependencies will change the project settings. Please use Android Resolve after the change complete.
### Changes
- Deprecation of `CASGeneral, CASTeen, CASPromo` dependencies in favor of the new system.
- Cross Promotion dependency moved to Advanced Integration.
- [iOS] NSUserTrackingUsageDescription is empty by default.
- [iOS] The storage location for temporary settings for iOS build has been changed to `Assets/CleverAdsSolutions/Editor/ios_cas_settings`.
### Added new optional third party mediation
You can integrate any set of networks using the advanced integration in the `Assets > CleverAdsSolutions > Settings` window.
- [MyTarget](https://target.my.com/) 
- [MobFox](https://www.mobfox.com/)
- [Amazon Ads](https://advertising.amazon.com/)

## [1.6.12] - 2020-11-02
### Bug Fixes
- `Assets > CleverAdsSolutions > Settings` are applied in any case, even when `CAS.MobileAds.settings` is not used.

## [1.6.11] - 2020-10-27
### Dependencies
- Wraps [Android 1.6.11+ SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- Wraps [iOS 1.6.11+ SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Android wrapper optimizations.
### Bug Fixes
- `LastPageAdContent` passe to Android CAS.

## [1.6.10] - 2020-10-27
### Bug Fixes
- iOS build errors.
- Update Package Manager version from Settings Window.

## [1.6.9] - 2020-10-23
### Features
- Added check for updates of CAS Unity in Settings window.
- Added warnings about incorrect position and size values of the banner for the Unity Editor.
### Changes
- The Android CAS wrapper for Unity has been moved to a separate library: `Plugins/Android/libs/cas-unity.jar`
- The default Banner Ad position is `BottomCenter` instead of `Undefined`. 
### Bug Fixes
- iOS Banner Ad callbacks.

## [1.6.8] - 2020-10-21
### Dependencies
- Wraps [Android 1.6.8+ SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- Wraps [iOS 1.6.8+ SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.161](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.161)
### Bug Fixes
- Last Page Ad Content transferring data to native CAS.

## [1.6.1] - 2020-10-13
### Dependencies
- Wraps [Android 1.6.4+ SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- Wraps [iOS 1.6.2+ SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [1.6.0] - 2020-10-07
### Dependencies
- Wraps [Android 1.6.2+ SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- Wraps [iOS 1.6.1+ SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.160](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.160)
### Features
- Added settings to [redirect rewarded video ad impressions](https://github.com/cleveradssolutions/CAS-Unity#allow-interstitial-ads-when-video-cost-are-lower) to interstitial ads at higher cost per impression:
```c#
CAS.MobileAds.settings.allowInterstitialAdsWhenVideoCostAreLower = allow;
```
- Added [CAS Last Page Ad](https://github.com/cleveradssolutions/CAS-Unity#last-page-ad) to your own promotion when there is no paid ad to show or internet availability:
```c#
CAS.MobileAds.manager.lastPageAdContent = new LastPageAdContent(...);
```
- Added methods to get the current banner size in pixels:
```c#
float height = CAS.MobileAds.manager.GetBannerHeightInPixels();
float width = CAS.MobileAds.manager.GetBannerWidthInPixels();
```
- Added more pre-build validation points to easily get a successful build.
### Changes
- Support Unity 2019.3, 2019.4, 2020.1. [New step to include Android](https://github.com/cleveradssolutions/CAS-Unity#support-of-unity-20193) native platform.
- Support Android 11.
- Support iOS 14.
- Update minimum Android API level (minSdkVersion) to 19 (KitKat)
### Bug Fixes
- iOS platform Pause Unity Application while Ad impression

## [1.5.0] - 2020-09-23
### Dependencies
- Wraps Android 1.5.0 SDK
- Wraps iOS 1.5.2 SDK
- External Dependency Manager for Unity 1.2.159
