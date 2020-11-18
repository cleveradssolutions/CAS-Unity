# Clever Ads Solutions Unity Plugin Change Log

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
