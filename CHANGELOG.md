# Clever Ads Solutions Unity Plugin Change Log

## [1.9.10] - 2021-02-15
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
