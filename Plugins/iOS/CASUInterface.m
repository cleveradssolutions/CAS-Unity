//
//  CASUSettings.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"
#import "CASUTypes.h"
#import "CASUView.h"
@import CleverAdsSolutions;


/// Returns an NSString copying the characters from |bytes|, a C array of UTF8-encoded bytes.
/// Returns nil if |bytes| is NULL.
static NSString * CASUStringFromUnity(const char *bytes) {
    return bytes ? @(bytes) : nil;
}

/// Returns a C string from a C array of UTF8-encoded bytes.
static const char * CASUStringToUnity(NSString *str) {
    if (!str) {
        return NULL;
    }

    const char *string = str.UTF8String;
    char *res = (char *)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#pragma mark - CAS Settings

void CASUSetAnalyticsCollectionWithEnabled(BOOL enabled) {
}

void CASUSetTestDeviceWithIds(const char **testDeviceIDs, int testDeviceIDLength) {
    NSMutableArray *testDeviceIDsArray = [[NSMutableArray alloc] init];

    for (int i = 0; i < testDeviceIDLength; i++) {
        [testDeviceIDsArray addObject:CASUStringFromUnity(testDeviceIDs[i])];
    }

    [CAS.settings setTestDeviceWithIds:testDeviceIDsArray];
}

void CASUSetTrialAdFreeInterval(int interval) {
    CAS.settings.trialAdFreeInterval = interval;
}

int CASUGetTrialAdFreeInterval(void) {
    return (int)CAS.settings.trialAdFreeInterval;
}

void CASUSetBannerRefreshRate(int interval) {
    CAS.settings.bannerRefreshInterval = interval;
}

int CASUGetBannerRefreshRate(void) {
    return (int)CAS.settings.bannerRefreshInterval;
}

void CASUSetInterstitialInterval(int interval) {
    CAS.settings.interstitialInterval = interval;
}

int CASUGetInterstitialInterval(void) {
    return (int)CAS.settings.interstitialInterval;
}

void CASURestartInterstitialInterval(void) {
    [CAS.settings restartInterstitialInterval];
}

void CASUSetUserConsent(int consent) {
    CAS.settings.userConsent = (CASConsentStatus)consent;
}

int CASUGetUserConsent(void) {
    return (int)CAS.settings.userConsent;
}

void CASUSetCCPAStatus(int doNotSell) {
    CAS.settings.userCCPAStatus = (CASCCPAStatus)doNotSell;
}

int CASUGetCCPAStatus(void) {
    return (int)CAS.settings.userCCPAStatus;
}

void CASUSetAudienceTagged(int audience) {
    CAS.settings.taggedAudience = (CASAudience)audience;
}

int CASUGetAudienceTagged(void) {
    return (int)CAS.settings.taggedAudience;
}

void CASUSetDebugMode(BOOL mode) {
    CAS.settings.debugMode = mode;
}

BOOL CASUGetDebugMode(void) {
    return CAS.settings.debugMode;
}

void CASUSetMuteAdSounds(BOOL muted) {
    CAS.settings.mutedAdSounds = muted;
}

BOOL CASUGetMuteAdSounds(void) {
    return CAS.settings.mutedAdSounds;
}

void CASUSetLoadingWithMode(int mode) {
    [CAS.settings setLoadingWithMode:(CASLoadingManagerMode)mode];
}

int CASUGetLoadingMode(void) {
    return (int)[CAS.settings getLoadingMode];
}

void CASUSetInterstitialAdsWhenVideoCostAreLower(BOOL allow) {
    [CAS.settings setInterstitialAdsWhenVideoCostAreLowerWithAllow:allow];
}

BOOL CASUGetInterstitialAdsWhenVideoCostAreLower(void) {
    return [CAS.settings isInterstitialAdsWhenVideoCostAreLowerAllowed];
}

void CASUSetiOSAppPauseOnBackground(BOOL pause) {
    [CASUPluginUtil setPauseOnBackground:pause];
}

BOOL CASUGetiOSAppPauseOnBackground(void) {
    return [CASUPluginUtil pauseOnBackground];
}

void CASUSetTrackLocationEnabled(BOOL enabled) {
    [CAS.settings setTrackLocationWithEnabled:enabled];
}

BOOL CASUGetTrackLocationEnabled(void) {
    return [CAS.settings isTrackLocationEnabled];
}

#pragma mark - User targeting options

void CASUSetUserGender(int gender) {
    [[CAS targetingOptions] setGender:(Gender)gender];
}

int CASUGetUserGender(void) {
    return (int)[[CAS targetingOptions] getGender];
}

void CASUSetUserAge(int age) {
    [[CAS targetingOptions] setAge:age];
}

int CASUGetUserAge(void) {
    return (int)[[CAS targetingOptions] getAge];
}

void CASUSetContentURL(const char *contentURL) {
    [CAS.targetingOptions setContentUrl:CASUStringFromUnity(contentURL)];
}

const char * CASUGetContentURL(void) {
    return CASUStringToUnity([CAS.targetingOptions getContentUrl]);
}

void CASUSetKeywords(const char **keywords, int keywordsLength) {
    NSMutableArray *keywordsArray = [[NSMutableArray alloc] init];

    for (int i = 0; i < keywordsLength; i++) {
        [keywordsArray addObject:CASUStringFromUnity(keywords[i])];
    }

    [CAS.targetingOptions setKeywords:keywordsArray];
}

#pragma mark - Utils

void CASUValidateIntegration(void) {
    [CAS validateIntegration];
}

void CASUOpenDebugger(CASUManagerRef manager) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    UIViewController *root = [CASUPluginUtil unityGLViewController];

    Class testSuit = NSClassFromString(@"CASTestSuit");

    if (testSuit) {
        SEL presentSelector = NSSelectorFromString(@"presentFromController:manager:");

        if ([testSuit respondsToSelector:presentSelector]) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
            [testSuit performSelector:presentSelector
                           withObject:root
                           withObject:[internalManager casManager]];
#pragma clang diagnostic pop
            return;
        }
    }

    UIStoryboard *storyboard =
        [UIStoryboard storyboardWithName:@"CASTestSuit"
                                  bundle:[NSBundle bundleForClass:[CASUManager class]]];

    if (!storyboard) {
        storyboard = [UIStoryboard storyboardWithName:@"CASDebugger"
                                               bundle:[NSBundle bundleForClass:[CASUManager class]]];
    }

    if (storyboard) {
        UIViewController *vc = [storyboard instantiateViewControllerWithIdentifier:@"DebuggerController"];

        if (vc) {
            SEL selector = NSSelectorFromString(@"setTargetManager:");

            if (![vc respondsToSelector:selector]) {
                NSLog(@"[CAS.AI] Framework bridge cant connect to CASTestSuit");
                return;
            }

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
            [vc performSelector:selector withObject:[internalManager casManager]];
#pragma clang diagnostic pop
            vc.modalPresentationStyle = UIModalPresentationFullScreen;
            [root presentViewController:vc animated:YES completion:nil];
            return;
        }
    }

    NSLog(@"[CAS.AI] Framework bridge cant find CASDebugger");
}

const char * CASUGetActiveMediationPattern(void) {
    return CASUStringToUnity([CASNetwork getActiveNetworkPattern]);
}

BOOL CASUIsActiveMediationNetwork(int net) {
    NSArray *values = [CASNetwork values];

    if (net > 0 && net < [values count]) {
        return [CASNetwork isActiveNetwork:[values objectAtIndex:net]];
    }

    return NO;
}

const char * CASUGetSDKVersion(void) {
    return CASUStringToUnity([CAS getSDKVersion]);
}

#pragma mark - CAS Manager

CASManagerBuilderRef CASUCreateBuilder(NSInteger  enableAd,
                                       BOOL       demoAd,
                                       const char *unityVersion,
                                       const char *userID) {
    CASManagerBuilder *builder = [CAS buildManager];

    [builder withAdFlags:(CASTypeFlags)enableAd];
    [builder withTestAdMode:demoAd];
    [builder withFramework:@"Unity" version:CASUStringFromUnity(unityVersion)];
    [builder withUserID:CASUStringFromUnity(userID)];

    [[CASUPluginUtil sharedInstance] saveObject:builder withKey:@"lastBuilder"];

    return (__bridge CASManagerBuilderRef)builder;
}

void CASUSetMediationExtras(CASManagerBuilderRef builderRef,
                            const char           **extraKeys,
                            const char           **extraValues,
                            NSInteger            extrasCount) {
    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    for (int i = 0; i < extrasCount; i++) {
        [builder withMediationExtras:CASUStringFromUnity(extraValues[i])
                              forKey:CASUStringFromUnity(extraKeys[i])];
    }
}

void CASUSetConsentFlow(CASManagerBuilderRef      builderRef,
                        BOOL                      isEnabled,
                        const char                *policyUrl,
                        CASUConsentFlowCompletion completion) {
    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    CASConsentFlow *flow = [[CASConsentFlow alloc] initWithEnabled:isEnabled];

    flow.privacyPolicyUrl = CASUStringFromUnity(policyUrl);

    if (completion) {
        flow.completionHandler = ^(enum CASConsentFlowStatus status) {
            completion();
        };
    }

    flow.viewControllerToPresent = [CASUPluginUtil unityGLViewController];
    [builder withConsentFlow:flow];
}

void CASUDisableConsentFlow(CASManagerBuilderRef builderRef) {
    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    [builder withConsentFlow:[[CASConsentFlow alloc]initWithEnabled:false]];
}

CASUManagerRef CASUInitializeManager(CASManagerBuilderRef               builderRef,
                                     CASManagerClientRef                *client,
                                     CASUInitializationCompleteCallback onInit,
                                     const char                         *identifier) {
    NSString *nsIdentifier = CASUStringFromUnity(identifier);

    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];

    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    if (onInit) {
        [builder withCompletionHandler:^(CASInitialConfig *config) {
            onInit(client,
                   CASUStringToUnity(config.error),
                   CASUStringToUnity(config.countryCode),
                   config.isConsentRequired,
                   config.manager.isDemoAdMode);
        }];
    }

    CASMediationManager *manager = [builder createWithCasId:nsIdentifier];
    CASUManager *wrapper = [[CASUManager alloc] initWithManager:manager forClient:client];
    [cache removeObjectWithKey:@"lastBuilder"];
    [cache saveObject:wrapper withKey:nsIdentifier];
    return (__bridge CASUManagerRef)wrapper;
}

void CASUFreeManager(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    manager.casManager.adLoadDelegate = nil;
    manager.interCallback = nil;
    manager.rewardCallback = nil;
    manager.appReturnDelegate = nil;
    [manager disableReturnAds];
    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    [cache removeObjectWithKey:manager.casManager.managerID];
}

#pragma mark - General Ads functions
BOOL CASUIsAdEnabledType(CASUManagerRef managerRef, int adType) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    return [manager.casManager isEnabledWithType:(CASType)adType];
}

void CASUEnableAdType(CASUManagerRef managerRef, int adType, BOOL enable) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager.casManager setEnabled:enable type:(CASType)adType];
}

void CASUSetLastPageAdContent(CASUManagerRef managerRef, const char *contentJson) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager setLastPageAdFor:CASUStringFromUnity(contentJson)];
}

#pragma mark - Interstitial Ads

void CASUSetInterstitialDelegate(CASUManagerRef                       managerRef,
                                 CASUDidLoadedAdCallback              didLoaded,
                                 CASUDidFailedAdCallback              didFailed,
                                 CASUWillPresentAdCallback            willPresent,
                                 CASUWillPresentAdCallback            didImpression,
                                 CASUDidShowAdFailedWithErrorCallback didShowWithError,
                                 CASUDidClickedAdCallback             didClick,
                                 CASUDidClosedAdCallback              didClosed) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    manager.interCallback.didLoadedCallback = didLoaded;
    manager.interCallback.didFailedCallback = didFailed;
    manager.interCallback.willOpeningCallback = willPresent;
    manager.interCallback.didImpressionCallback = didImpression;
    manager.interCallback.didShowFailedCallback = didShowWithError;
    manager.interCallback.didClickCallback = didClick;
    manager.interCallback.didClosedCallback = didClosed;
}

void CASULoadInterstitial(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager.casManager loadInterstitial];
}

BOOL CASUIsInterstitialReady(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    return manager.casManager.isInterstitialReady;
}

void CASUPresentInterstitial(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager presentInter];
}

#pragma mark - Rewarded Ads

void CASUSetRewardedDelegate(CASUManagerRef                       managerRef,
                             CASUDidLoadedAdCallback              didLoaded,
                             CASUDidFailedAdCallback              didFailed,
                             CASUWillPresentAdCallback            willPresent,
                             CASUWillPresentAdCallback            didImpression,
                             CASUDidShowAdFailedWithErrorCallback didShowWithError,
                             CASUDidClickedAdCallback             didClick,
                             CASUDidCompletedAdCallback           didComplete,
                             CASUDidClosedAdCallback              didClosed) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    manager.rewardCallback.didLoadedCallback = didLoaded;
    manager.rewardCallback.didFailedCallback = didFailed;
    manager.rewardCallback.willOpeningCallback = willPresent;
    manager.rewardCallback.didImpressionCallback = didImpression;
    manager.rewardCallback.didShowFailedCallback = didShowWithError;
    manager.rewardCallback.didClickCallback = didClick;
    manager.rewardCallback.didCompleteCallback = didComplete;
    manager.rewardCallback.didClosedCallback = didClosed;
}

void CASULoadReward(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager.casManager loadRewardedAd];
}

BOOL CASUIsRewardedReady(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    return manager.casManager.isRewardedAdReady;
}

void CASUPresentRewarded(CASUManagerRef managerRef) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager presentReward];
}

#pragma mark - AdView

CASUViewRef CASUCreateAdView(CASUManagerRef   managerRef,
                             CASViewClientRef *client,
                             int              adSizeCode) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;
    CASUView *view = [[CASUView alloc] initWithManager:manager.casManager forClient:client size:adSizeCode];
    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];

    [cache saveObject:view withKey:[NSString stringWithFormat:@"%@_%d", manager.casManager.managerID, adSizeCode]];
    return (__bridge CASUViewRef)view;
}

void CASUDestroyAdView(CASUViewRef viewRef, const char *key) {
    CASUView *view = (__bridge CASUView *)viewRef;

    if (view) {
        [view destroy];
    }

    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    [cache removeObjectWithKey:CASUStringFromUnity(key)];
}

void CASUAttachAdViewDelegate(CASUViewRef                 viewRef,
                              CASUViewDidLoadCallback     didLoad,
                              CASUViewDidFailedCallback   didFailed,
                              CASUViewWillPresentCallback willPresent,
                              CASUViewDidClickedCallback  didClicked,
                              CASUViewDidRectCallback     didRect) {
    CASUView *view = (__bridge CASUView *)viewRef;

    view.adLoadedCallback = didLoad;
    view.adFailedCallback = didFailed;
    view.adPresentedCallback = willPresent;
    view.adClickedCallback = didClicked;
    view.adRectCallback = didRect;
    [view attach];
}

void CASUPresentAdView(CASUViewRef viewRef) {
    CASUView *view = (__bridge CASUView *)viewRef;

    [view present];
}

void CASUHideAdView(CASUViewRef viewRef) {
    CASUView *view = (__bridge CASUView *)viewRef;

    [view hide];
}

void CASUSetAdViewPosition(CASUViewRef viewRef, int posCode, int x, int y) {
    CASUView *view = (__bridge CASUView *)viewRef;

    [view setPositionCode:posCode withX:x withY:y];
}

void CASUSetAdViewRefreshInterval(CASUViewRef viewRef, int interval) {
    CASUView *view = (__bridge CASUView *)viewRef;

    [view setRefreshInterval:interval];
}

void CASULoadAdView(CASUViewRef viewRef) {
    CASUView *view = (__bridge CASUView *)viewRef;

    [view load];
}

BOOL CASUIsAdViewReady(CASUViewRef viewRef) {
    CASUView *view = (__bridge CASUView *)viewRef;

    return [view isReady];
}

int CASUGetAdViewRefreshInterval(CASUViewRef viewRef) {
    CASUView *view = (__bridge CASUView *)viewRef;

    return [view getRefreshInterval];
}

#pragma mark - App Return Ads

void CASUSetAppReturnDelegate(CASUManagerRef                       managerRef,
                              CASUWillPresentAdCallback            willPresent,
                              CASUWillPresentAdCallback            didImpression,
                              CASUDidShowAdFailedWithErrorCallback didShowWithError,
                              CASUDidClickedAdCallback             didClick,
                              CASUDidClosedAdCallback              didClosed) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    manager.appReturnDelegate.willOpeningCallback = willPresent;
    manager.appReturnDelegate.didImpressionCallback = didImpression;
    manager.appReturnDelegate.didShowFailedCallback = didShowWithError;
    manager.appReturnDelegate.didClickCallback = didClick;
    manager.appReturnDelegate.didClosedCallback = didClosed;
}

void CASUEnableAppReturnAds(CASUManagerRef manager) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;

    [internalManager enableReturnAds];
}

void CASUDisableAppReturnAds(CASUManagerRef manager) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;

    [internalManager disableReturnAds];
}

void CASUSkipNextAppReturnAds(CASUManagerRef manager) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;

    [internalManager skipNextAppReturnAd];
}

#pragma mark - Ad Impression

int CASUGetImpressionNetwork(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        NSString *network = internalImp.network;

        if ([network isEqualToString:CASNetwork.casExchange]) {
            return CASNetworkIdDSPExchange;
        }

        if ([network isEqualToString:CASNetwork.lastPageAd]) {
            return CASNetworkIdLastPageAd;
        }

        NSUInteger netIndex = [[CASNetwork values] indexOfObject:network];

        if (netIndex != NSNotFound) {
            return (int)netIndex;
        }
    }

    return -1;
}

double CASUGetImpressionCPM(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return internalImp.cpm;
    }

    return 0.0;
}

int CASUGetImpressionPrecission(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return (int)internalImp.priceAccuracy;
    }

    return (int)CASPriceAccuracyUndisclosed;
}

const char * CASUGetImpressionCreativeId(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return CASUStringToUnity(internalImp.creativeIdentifier);
    }

    return NULL;
}

const char * CASUGetImpressionIdentifier(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return CASUStringToUnity(internalImp.identifier);
    }

    return NULL;
}

int CASUGetImpressionDepth(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return (int)internalImp.impressionDepth;
    }

    return 0;
}

double CASUGetImpressionLifetimeRevenue(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return internalImp.lifetimeRevenue;
    }

    return 0.0;
}

#pragma mark - Consent Flow

void CASUShowConsentFlow(BOOL                      enabled,
                         const char                *policy,
                         CASUConsentFlowCompletion completion) {
    CASConsentFlow *flow = [[CASConsentFlow alloc] initWithEnabled:enabled];

    flow.privacyPolicyUrl = CASUStringFromUnity(policy);

    if (completion) {
        flow.completionHandler = ^(enum CASConsentFlowStatus status) {
            completion();
        };
    }

    flow.viewControllerToPresent = [CASUPluginUtil unityGLViewController];
    [flow present];
}

#pragma mark - ATT API

void CASURequestATT(CASUATTCompletion completion) {
    [CASInternalUtils trackingAuthorizationRequest:^(NSUInteger status) {
        completion(status);
    }];
}

NSUInteger CASUGetATTStatus(void) {
    NSUInteger status = [CASInternalUtils adTrackingStatus];

    if (status > 3) {
        return 0;
    }

    return status;
}
