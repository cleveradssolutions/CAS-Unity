# CAS.AI Unity Plugin Change Log

# [3.9.7] - 2024-11-07
- Discover the native 3.9.7 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).

# [3.9.6] - 2024-10-25
- Discover the native 3.9.6 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- [Android] Fixed an issue where the Android Gradle files was cleared before build. 

# [3.9.5] - 2024-10-14
- Discover the native 3.9.5 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).

# [3.9.4] - 2024-09-24
- Discover the native 3.9.4 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- The Manager Ad Object will no longer display an initialization error on unsupported platforms.
- The warning "Not found config file" will no longer be displayed for the test "demo" CAS ID.
### Update Adapters
> Below are important changes in the adapters that should be noted. Please refer to the native SDKs release notes for a complete overview of all adapter updates.
- [Android] Yandex Ads
  - ⚠️ [SDK] The minimum AppMetrica version is now 7.2.0 [(Unity plugin 6.3.0)](https://github.com/appmetrica/appmetrica-unity-plugin/releases).
  - ⚠️ [SDK] The minimum Android Gradle plugin version is now 7.0 (Unity 2021.3.41f+)
- [iOS] Google Ads
  - [lib] Fixed a bug related to MarketplaceKit that would cause the SDK to crash when running on MacOS.

# [3.9.3] - 2024-08-16
- Discover the native 3.9.3 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- [Android] Fixed potential ANR from AppStateNotifier initialization.

# [3.9.2] - 2024-08-01
- Discover the native 3.9.2 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- Added new methods for retrieving user consent information for individual vendors and additional providers.
```cs
ConsentStatus googleConsent = CAS.MobileAds.settings.getVendorConsent(755)
ConsentStatus audienceNetworkConsent = CAS.MobileAds.settings.getAdditionalConsent(89)
```
### Bug Fixes
- [Editor] Fixed compilation error: name `CASPostGenerateGradle` could not be found.
  > Please remove the `CASDeveloper` from the Sciripting Define Symbols if it was added to work around this issue.
- [iOS] Fixed the `EXC_BAD_ACCESS` crash that occurred when accessing `AdMetaData` slightly later.
- [Android] Added an alternative maven repository for Madex artifacts for those who had difficulties accessing the official repository.

# [3.9.1] - 2024-07-26
- Discover the native 3.9.1 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Changes
- The CASExchange adapter has been included to the Optimal Ads Solutions.
- Previously beta adapters are now available to all: CASExchange, HyprMX, and StartIO.

# [3.9.0] - 2024-07-17
- Discover the native 3.9.0 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Changes
- [Android] Requires a minimum compileSdkVersion of 34.
- [iOS] Requires apps to build with Xcode 15.3 or above.
### Bug Fixes
- [Android] Fixed gradle build error `Could not find method apply()` in Unity 2022+.
- [Editor] Fixed exception log when detecting Gradle version in Unity 6.

# [3.8.1] - 2024-06-28
- Discover the native 3.8.1 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- [iOS] Fixed a critical issue related to the position of Banner ads.
- [iOS] Fixed `NSUserTrackingUsageDescription` localization with `com.unity.localization` package.
- [iOS] Fixed XCode build error `CASResources no such file or directory` when the unity project was compiled on Windows.
- [iOS] Fixed an issue where the UIViewController is invalid for the Consent Flow.

# [3.8.0] - 2024-06-21
- Discover the native 3.8.0 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- Added a log with useful information about ad impression with disabled verbose logs.
### Update Banner Ad Position
  - Added support new `AdPosition`: `MiddleCenter`, `MiddleLeft`, `MiddleRight`.
  - Added support for offset (X, Y) position from any `AdPosition`. Previously, only `TopLeft` was supported.
  - Added optional parameter `AdPosition` to method `SetPosition(int x, int y, AdPosition position = AdPosition.TopLeft)` method.
  - Added new method `SetPositionPx(int x, int y, AdPosition position = AdPosition.TopLeft)` to set (X, Y) offset in pixels.
  - Added new method `float CAS.MobileAds.GetDeviceScreenScale()` to retrieve the device screen scale in independent pixels (DP). Use this scale to convert pixels to DP and vice versa.
  - The `rectInPixels` is now available immediately in the `OnLoaded` event.
  - The `rectInPixels` will not be reset when the banner is hidden, the values ​​remain from the last display on the screen.
### Bug Fixes
- [iOS] Fixed the position of banner ads after changing the device orientation.
- [Android] Removed gradle property `enableDexingArtifactTransform` for Unity 2022.2+ as deprecated with newest gradle versions.
### Changes
- The `ConsentFlowAdObject.showOnAwakeIfRequired` changed to `False` by default.
- The `ManagerAdObject.initializeOnAwake` changed to `False` by default.
- [Android] The `CASPlugin.androidlib` has been replaced by `CASUnityBridge.aar` for compatibility with Unity 6.
- [Android] Now Gradle file modifications will be performed in temporary build files instead of project assets.
- [iOS] The `Info.plist` configuration post-process was moved to `int.MaxValue - 10` order to workaround the issue when other plugins were overriding parameters, such as SKAdNetowkrIDs.
### New ads networks support in closed beta
- CASExchange - is a cutting-edge exchange platform designed to extend our SDK, enabling integration with Demand Side Platforms (DSPs).
- Ogury
- LoopMe

# [3.7.3] - 2024-05-23
- Discover the native 3.7.3 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- Added new `EnableOptionsButton` event for `ConsentFlowAdObject` to enable/disable Consent option button.
### Bug Fixes
- [iOS] Fixed `EXC_BAD_ACCESS` crash from `didCloseAd` callback.
### Changes
- The MyTarget network support has been removed. Our team considers MyTarget to be ineffective and excludes it from CAS mediation.
  > Please remove the MyTarget adapter if you are using it.

# [3.7.2] - 2024-05-09
- Discover the native 3.7.2 SDKs release notes for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- Introduced automatic collect impression level data to Tenjin Analytics. (Closed beta)
### Bug Fixes
- [iOS] Fixed an issue where the AppReturn ads events could be raised from outside the Unity Mein Thread.
- [iOS] Fixed the Privacy Info invalid API reason declaration for `UnityFramework` in the UserDefaults category. The issue stemmed from the SuperAwesome framework, which received an update with the fix.
- [Editor] Fixed the compilation error `The type or namespace name 'NUnit' could not be found`.
- [Editor] Fixed an error when the plugin component was not found after the unitypackage integration.
- [Editor] Fixed plugin version checking given Revision part.

# [3.7.1] - 2024-04-25
- The native SDK remains version 3.7.0.
### Bug Fixes
- [Android] Fixed `Exception: JNI: Unknown signature for type 'CAS.Android.CASConsentFlowClient'`.

# [3.7.0] - 2024-04-22
- Discover the latest features in native 3.6.1 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- Added support `AdType.AppOpen`. Read integration guides for [UnityEditor](https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Open-Ad-object) or [Script C#](https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Open-Ads)
- Added new static events in `CAS.MobileAds` class: `OnApplicationBackground` and `OnApplicationForeground`. It's important to use new events when you want to show AppOpen ads on user returning to the game.  
- Added `ConsentFlowAdObject` component.
- Added option to disable `ConsentFlow` via `ManagerAdObject` component.  
- Added `ConsentFlowAdObject` optional field for `ManagerAdObject` to configure `ConsentFlow` on Ads initialization.  
- Added `ConsentFlow.Status` enum and new event for `ConsentFlow.WithCompletionListener(Action<ConsentFlow.Status>)`.  
- Added `ConsentFlow.DebugGeography` enum and `ConsentFlow.WithDebugGeography()` method to sets debug geography for testing purposes.  
- Added `ShowIfRequired()` method for `ConsentFlow` instance to show the consent form only if it is required and the user has not responded previously.  
- Added `Show()` method for `ConsentFlow` instance with the same functionality as the `CAS.MobileAds.ShowConsentFlow(ConsentFlow)` but easier to use.  
```c#
new ConsentFlow()
    .WithDebugGeography(ConsentFlow.DebugGeography.EEA)
    .WithCompletionListener((status) =>
    {
        if (status == ConsentFlow.Status.Obtained)
        {
            // User consent obtained.
        }
    })
    .ShowIfRequired();
```
- Made `CAS.MobileAds.ShowConsentFlow()` obsolete in favor of new `ShowIfRequired()` or `Show()` methods for `ConsentFlow` instance.  
- Made `OnAdOpening` events obsolete. Please use `OnAdImpression` event to collect `AdMetaData` about the ad impression or `OnAdShown` event if `AdMetaData` is not used.
- Performance improvements when forwarding native callbacks.
- In manual ad loading mode, the SDK will no longer trigger `OnAdFailedToLoad` events before closing the Interstitial and Rewarded ads.
- [Android] Added `androidx.lifecycle:lifecycle-process:2.6.2` dependency.
- [Editor] Added `ConsentFlow` completion events implementation to testing in editor.
- [Editor] Removed `Delay measurement of the Ad SDK initialization` option, which applies in any case.

# [3.6.1] - 2024-04-11
- Discover the latest features in native 3.6.1 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- [iOS] Fixex Banner ads moving after full screen due to a safe area bug in Unity Engine.
- [Editor] The Utils class alias has been hidden to avoid conflict with the global namespace.
### New ads networks support in closed beta
- StartIO

# [3.6.0] - 2024-03-29
- Discover the latest features in native 3.6.0 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- [iOS] The minimum supported Xcode version has been increased to 15.1.
- [iOS] Added support for the Apple Privacy Manifest update to ensure publisher compliance with new App Store submission requirements.
- [iOS] Added required `AdSupport` and `AppTrackingTransparency` frameworks if `UserTrackingUsageDescription` defined in settings window.
- Added `CAS.MobileAds.targetingOptions.locationCollectionEnabled` property to collect from the device the latitude and longitude coordinated truncated to the hundredths decimal place. And `Location targeting if allowed` default value for both platforms to settings window. Collect occurs only if your application already has the relevant end-user permissions and if the target audience is not children.
- Make `CAS.MobileAds.settings.trackLocationEnabled` obsolete in favor of locationCollectionEnabled in targetingOptions for both platforms.
### Bug Fixes
- Fixed the mapping of `AdNetwork.DTExchange` to the native CAS for the `AdMetaData` structure.

# [3.5.6] - 2024-02-28
- Discover the latest features in native 3.5.6 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).

# [3.5.5] - 2024-02-22
- Discover the latest features in native 3.5.5 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
### Bug Fixes
- [Android] Fixed `NullPointerException` when initializing CAS if `WithMediationExtras()` was not used. (experienced on 3.5.4)
### New ads networks support in closed beta
- Madex

# [3.5.4] - 2024-02-20
- Discover the latest features in native 3.5.4 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
## Bug Fixes
- Fixed invoke of `InterstitialAdObject.OnAdShown` event.
- [Android] Added workaround to migrate Kotlin 1.8.0 since it no longer supports JVM targets 1.6 and 1.7.

# [3.5.2] - 2024-01-10
- Discover the latest features in native 3.5.2 SDKs for [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases).
- [Android] Added `packagingOptions` to gradle files to pick the first occurrence of `META-INF/kotlinx_coroutines_core.version` and `META-INF/core-utils_release.kotlin_module` files. This will fix the duplicate issue.
- [iOS] Added a delay before calling `UnityUpdateMuteState()` to fix an issue with Unity losing audio after closing a fullscreen ad.

# [3.5.1] - 2023-12-21
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.5.1 SDK

# [3.5.0] - 2023-12-06
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.5.0 SDK
- Added new `CAS.MobileAds.targetingOptions.SetKeywords()` to sets a list of keywords, interests, or intents related to your application. Words or phrase describing the current activity of the user for targeting purposes.
- Added new `CAS.MobileAds.targetingOptions.contentUrl` to sets the content URL for a web site whose content matches the app's primary content. This web site content is used for targeting and brand safety purposes.
- [Android] Added new Editor Option to Optimize initialization and Google Ad loading. Look for the new option in `CAS Android Settings > Other Settings` window. By default, optimization is active.
- [Android] Added Editor Options to removing the property tag from the Android Manifest of the Google Mobile Ads Android SDK. This is enabled for projects using Android Gradle Plugin version 4.2.1 and lower. 
  > GMA Android SDK 22.4.0 and above introduces a property tag in its Android Manifest that is not compatible with lower versions of Android Gradle Plugin (used by Unity 2022.1 and below).
- [Android] The update to `CASPlugin.androidlib/AndroidManifest.xml` occurs not in Project Assets but in the generated Gradle project.
- [Editor] Cleaned up scripts by removing redundant code.
## Changes
- The Meta Audience Network has been included to the Optimal Ads Solutions. 
  > Please complete [Additional integration steps](https://github.com/cleveradssolutions/CAS-Unity/wiki/Additional-Meta-AudienceNetwork-steps) to enable Meta monetization. 
- The Bigo Ads has been included to the Optimal Ads Solutions. 
- The AdColony network support has been removed. The AdColony will sunset and migrate to DTExchange.
  > Please remove the AdColony adapter if you are using it.
- [Android] Now `CASPlugin.androidlib` is placed in `CleverAdsSolution/Plugins/Android` instead of `Assets/Plugins/Android`. The obsolete `CASPlugin.androidlib` will be automatically removed during the application build.
## New ads networks support in closed beta
- BidMachine

# [3.4.2] - 2023-11-20
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.4.2 SDK
- [Editor] Added disabled Unity Editor Domain Reloading support.
## Bug Fixes
- [iOS] Fixed rare crash from `AdViewRectCallback`.

# [3.4.1] - 2023-11-10
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.4.1 SDK
- A new `CAS.MobileAds.settings.trialAdFreeInterval` property has been introduced to defines the time interval, in seconds, starting from the moment of the initial app installation, during which users can use the application without ads being displayed while still retaining access to the Rewarded Ads and App Open Ads formats.
  > Within this interval, users enjoy privileged access to the application's features without intrusive advertisements.
- The `CAS.MobileAds.settings` will be return a more correct Privacy states.
## Changes
- [iOS] Updated minimum supported version to iOS 13.
- The MyTarget has been removed from the Optimal/Families Ads Solutions, as the MyTarget is focused only on the CIS region.
  > If you want to continue using MyTarget monetization, just include the adapter to your app.
- The AdColony has been removed from the Optimal/Families Ads Solutions. The AdColony will sunset and migrate to DTExchange on January 3, 2024.
  > If you want to continue using AdColony monetization, just include the adapter to your app.  
- Now the Interstitial Ad load callback will only be fired after the interval between impressions has expired.
## Bug Fixes
- [iOS] Added `UnityUpdateMuteState()` invoke after closing the fullscreen ad to fix an issue with Unity losing audio.

# [3.3.2] - 2023-10-17
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.3.2 SDK
- Added `AdNetwork.CASExchange`
- [Android] Optimized calling native methods for managing banners.
## Bug Fixes
- [Android] Fixed an issue with an invisible banner ad with Unity 2021.3.31f1 and 2022.3.10f1.
- [iOS] Fixed `YandexMobileAdsBundle.bundle is not added` with ussed `use_frameworks! :linkage => :static` in Podfile.
  > Due to an bug in the Yandex Ads SDK, we had to add the bundle to the app target.
- [iOS] Fixed `Multiple commands produce` with Dynamic frameworks if `use_frameworks!` in Podfile.
- [iOS] Fixed issue with missing `CFBundleShortVersionString` in App target if `use_frameworks!` in Podfile.
> We are working to resolve issues with `use_frameworks!`, however we still recommend set `use_frameworks! :linkage => :static` only in Podfile.


# [3.3.1] - 2023-10-06
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.3.1 SDK
## Changes
- [iOS] The Pangle has been removed from the Families Ads Solution.  
  > The Pangle source does not provide ads under COPPA restrictions. The Pangle is still included in the Optimal Ads Solution.
## Bug Fixes
- [iOS] Fixed issue with missing `CFBundleShortVersionString` in UnityFramework.
- [iOS] Fixed issue with copying dynamic frameworks to app archive.
- [Android] Fixed a rare issue with the banner could be hidden by another view.
- Fixed automatic removal of the Tapjoy adapter that could cause build errors. 
## New ads networks support in closed beta
- Bigo

# [3.3.0] - 2023-09-26
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.3.0 SDK
- Added the option to switch to using Google User Messaging Platform instead of CAS Consent Flow.  
Please contact support to migrate to a certified CMP.  
  > You also have the opportunity to independently use any certified CMP before CAS initialization and CAS SDK will transmit information about the user’s consent for mediation.
- Added the options to define the **Limited Data Use** and **Advertising Tracking** flags for Meta Audience Network in the inspector of `ManagerAdObject`.
- Added an `OnInitialziationFailed` event to the `ManagerAdObject` inspector.
- Added an `InitializationError` class with const strings to compare with `InitialConfiguration.error` values.
## Changes
- The Meta Audience Network has been removed from the optimal advertising solution.  
  > If you want to continue using the Audience Network for monetization, then add the adapter dependency and comply with its privacy requirements.
- The Network names have been replaced with more conventional ones in the Firebase `ad_impression` event.
- The `AdMetaData.identifier` property now returns the Placement ID from the network on which the ad was shown.
- The `AdNetwork.Vungle` renamed to LiftoffMonetize.
- The `AdNetwork.DigitalTurbine` renamed to DTExchange.
## New ads networks support in closed beta
- HyprMX - Focused to USA region only.
- Smaato - Support Banner ad only.

# [3.2.5] - 2023-09-05
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.5 SDK
## Changes
- [iOS] Updated ATT dialog text to be consistent with Apple policy (3.2.2 Unacceptable).
- Removed Tapjoy network support.
  > Video product Tapjoy is now integrated as a demand partner on the ironSource, iSX exchange.
- Added debug price ($1) and creative ID for test ads impression.
## Fixes
- [Editor] Fixed `Metadata file could not be found` during version package update.
- [Editor] Disabled of Gradle Tools version update for Used Gradle wrapper version in build.

# [3.2.4] - 2023-07-15
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.4 SDK

# [3.2.3] - 2023-07-04
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.3 SDK

# [3.2.2] - 2023-07-03
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.2 SDK
### Fixes
- [Android] Fixed `Exception: No such proxy method: onCASInitialized` with Test Ad Mode.

## [3.2.1] - 2023-06-25
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.1 SDK
### Fixes
- [Android] Fixed signature exception from `CAS.Android.CASConsentFlowClient`.
- [Android] Fixed rare `UnsupportedOperationException` from Test Ads Activity.
- [iOS] Fixed fatal error `Library not loaded` for DigitalTurbine. 
- [iOS] Fixed a critical performance bug for AudienceNetwork, Tapjoy, UnityAds.

## [3.2.0] - 2023-06-20
- Wraps [Android](https://github.com/cleveradssolutions/CAS-Android/releases) and [iOS](https://github.com/cleveradssolutions/CAS-iOS/releases) 3.2.0 SDK
- Added `ConsentFlow.WithCompletionListener()` to invoke Action when the dialog is dismissed.
- Added `MobileAds.ShowConsentFlow()` method to manually display ConsentFlow, before and after CAS initialization. 
  > On CAS initialization, the ConsentFlow still can be displayed automatically when conditions are met. 
```csharp
MobileAds.ShowConsentFlow(
  new ConsentFlow()
    .WithCompletionListener(() => Debug.Log("The dialog is dismissed."))
);
```
- Added `IManagerBuilder.WithCompletionListener()` with new `InitialConfiguration` parameter:
```csharp
MobileAds.BuildManager().WithCompletionListener((config) =>
{
  string initErrorOrNull = config.error;
  string userCountryISO2OrNull = config.countryCode;
  bool protectionApplied = config.isConsentRequired;
  IMediationManager manager = config.manager;
}).Build();
```
## Changes
- The list of networks that are included in the Clever solutions has been changed.: 
  - Optimal solution: Added Chartboost and DTExchange.
  - Families solution: Added Chartboost and DTExchange.
- The CAS ConsentFlow dialog will not be present to users who have seen the Google User Messaging Platform form. The user's choice will apply to all networks in the mediation.
- The `AdsSettings.analyticsCollectionEnabled` is deprecated and enabled by default for CrossPromo. 
- [Android] The Launcher Gradle template file is no longer required.
- [Android] The Base Gradle template file is no longer required for Unity 2022.2 or newer.
- [Android] The Multidex is no longer required.
- [Android] The Gradle version update is no longer required for Unity 2022.2 or newer.
- [Android] The Queries is no longer required for CrossPromo.
- [iOS] The `Unity-iPhone` target is no longer required in the Podfile.
- [iOS] The Queries Schemes is no longer required in the Info.plist.
- [Editor] Added one click plugin version update for `.unitypackage` integration in Editor Ads Settings window.
- [Editor] Added option to `Include Ad dependency versions` in Editor Ads Settings window. The Ads SDK versions are no longer visible in Gradle and Podfile by default.
- [Editor] The `Most popular country of users` option is no longer required.
- [Editor] The `CASEditorSettings` inspector is hidden.
### Fixes
- [Android] Fixed Unity Activity freezing after Return to App Ads closed.
- [Editor] Fixed infinity import assets bug on Windows Unity Editor.

## [3.1.9] - 2023-06-12
### Dependencies
- [Android] Wraps [3.1.9 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.9 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [3.1.8] - 2023-05-31
### Dependencies
- [Android] Wraps [3.1.8 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.8 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [3.1.7] - 2023-05-24
### Dependencies
- [Android] Wraps [3.1.7 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.7 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [3.1.6] - 2023-05-17
### Dependencies
- [Android] Wraps [3.1.6 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.6 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- Changes to `AdsSettings.userConsent` and `AdsSettings.userCCPAStatus` are now saved even after initialization CAS. You still need to restart the application to apply changes to mediation.
### Bug Fixes
- [Editor] Typo in Settings Template Util Field (#3)

## [3.1.5] - 2023-05-05
### Dependencies
- [Android] Wraps [3.1.5 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.5 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [3.1.4] - 2023-04-24
### Dependencies
- [Android] Wraps [3.1.4 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.4 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.176](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.176)
### Features
- [Android] Unity 2022.2+ Support with EDM4U 1.2.176 update. Requires `settingsTemplate.gradle` to be enabled.

## [3.1.3] - 2023-04-24
### Dependencies
- [Android] Wraps [3.1.3 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.3 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
- External Dependency Manager for Unity [1.2.175](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.175)

## [3.1.2] - 2023-03-28
### Dependencies
- [Android] Wraps [3.1.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

## [3.1.1] - 2023-03-20
### Dependencies
- [Android] Wraps [3.1.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.1.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [Editor] Added feature to check and remove Legacy Unity Ads package.
- [Editor] The CAS plugin version query has been moved to the background.
- [Editor] Simplify your External Dependency Manager installation process with a single click.
### Bug Fixes
- [iOS] Fixed the initial position of the banner ads.
- [iOS] Fixed publishing app with new list of app tracking descriptions from CAS 3.0.2.

## [3.0.2] - 2023-03-09
### Dependencies
- [Android] Wraps [3.0.2 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.0.2 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)
### Features
- [Editor] Compiled important plugin scripts to provide access to documentation when integrated by Unity Package Manager.
- [Editor] Improved unity editor plugin cache. 
- [Editor] The dialog of new version of the plugin before build has been replaced with a warning log.
- [Android] Improved stability of full-screen ads when restarting the application from the desktop.
- [Android] Added automatic management of the Advertiser ID (`AD_ID`) permission depending on the selected audience. You have the option to Add/Remove the permission under Other settings section.
### Bug Fixes
- [Android] Fixed `IAdView.rectInPixels` values.

## [3.0.1] - 2023-02-06
### Dependencies
- [Android] Wraps [3.0.1 SDK](https://github.com/cleveradssolutions/CAS-Android/releases)
- [iOS] Wraps [3.0.1 SDK](https://github.com/cleveradssolutions/CAS-iOS/releases)

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