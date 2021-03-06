# Clever Ads Solutions Unity Plugin Change Log

## [2.4.0] - 2021-07-05
### Dependencies
- [Android] Wraps [2.4.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.4.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Features
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
## Bug Fixes
- [iOS] Fixed bug with ReturnToPlayAd.
- [Editor] Fixed bug when building with `-batchmode` and disabled ads for the application.

## [2.3.0] - 2021-06-11
### Dependencies
- [Android] Wraps [2.3.0 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.3.0 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Features
- Added the ability to enable automatic display of [interstitial ads for users who return to the open application](https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Return-Ads).
## Changes
> Ads Solution names have been changed to match terms with native platforms.
- [Android] The `NotChildrenAds` solution has been renamed to `OptimalAds` solutions.
- [Android] Removed the `MixedAudience` solution in favor of `OptimalAds` solutions.
- [iOS] The `Recomended` solution has been renamed to `OptimalAds` solutions.
- [iOS] Removed the `Main` solution in favor of `OptimalAds` solutions.
## Bug Fixes
- [Android] Fixed an issue due to which sometimes the banner ads was not visible on the screen.
- [iOS] Fixed duplication of Google Utilities framework using Firebase and Unity 2019.4+.

## [2.2.4] - 2021-05-31
### Dependencies
- [Android] Wraps [2.2.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [2.2.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
## Changes
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
