//
//  CASUSettings.h
//  CASUnityPlugin
//
//  Copyright © 2023 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"
#import "CASUTypes.h"
#import "CASUView.h"
@import CleverAdsSolutions;

#pragma mark - CAS Settings

void CASUSetAnalyticsCollectionWithEnabled(BOOL enabled) {
    [[CAS settings] setAnalyticsCollectionWithEnabled:enabled];
}

void CASUSetTestDeviceWithIds(const char **testDeviceIDs, int testDeviceIDLength) {
    NSMutableArray *testDeviceIDsArray = [[NSMutableArray alloc] init];

    for (int i = 0; i < testDeviceIDLength; i++) {
        [testDeviceIDsArray addObject:[CASUPluginUtil stringFromUnity:testDeviceIDs[i]]];
    }

    [[CAS settings] setTestDeviceWithIds:testDeviceIDsArray];
}

void CASUSetBannerRefreshRate(int interval) {
    [[CAS settings] setBannerRefreshWithInterval:interval];
}

int CASUGetBannerRefreshRate(void) {
    return (int)[[CAS settings] getBannerRefreshInterval];
}

void CASUSetInterstitialInterval(int interval) {
    [[CAS settings] setInterstitialWithInterval:interval];
}

int CASUGetInterstitialInterval(void) {
    return (int)[[CAS settings] getInterstitialInterval];
}

void CASURestartInterstitialInterval(void) {
    [[CAS settings] restartInterstitialInterval];
}

void CASUSetUserConsent(int consent) {
    [[CAS settings] updateUserWithConsent:(CASConsentStatus)consent];
}

int CASUGetUserConsent(void) {
    return (int)[[CAS settings] getUserConsent];
}

void CASUSetCCPAStatus(int doNotSell) {
    [[CAS settings] updateCCPAWithStatus:(CASCCPAStatus)doNotSell];
}

int CASUGetCCPAStatus(void) {
    return (int)[[CAS settings] getCCPAStatus];
}

void CASUSetAudienceTagged(int audience) {
    [[CAS settings] setTaggedWithAudience:(CASAudience)audience];
}

int CASUGetAudienceTagged(void) {
    return (int)[[CAS settings] getTaggedAudience];
}

void CASUSetDebugMode(BOOL mode) {
    [[CAS settings] setDebugMode:mode];
}

void CASUSetMuteAdSoundsTo(BOOL muted) {
    [[CAS settings] setMuteAdSoundsTo:muted];
}

void CASUSetLoadingWithMode(int mode) {
    [[CAS settings] setLoadingWithMode:(CASLoadingManagerMode)mode];
}

int CASUGetLoadingMode(void) {
    return (int)[[CAS settings] getLoadingMode];
}

void CASUSetInterstitialAdsWhenVideoCostAreLower(BOOL allow) {
    [[CAS settings] setInterstitialAdsWhenVideoCostAreLowerWithAllow:allow];
}

void CASUSetiOSAppPauseOnBackground(BOOL pause) {
    [CASUPluginUtil setPauseOnBackground:pause];
}

BOOL CASUGetiOSAppPauseOnBackground(void) {
    return [CASUPluginUtil pauseOnBackground];
}

void CASUSetTrackLocationEnabled(BOOL enabled) {
    [[CAS settings] setTrackLocationWithEnabled:enabled];
}

#pragma mark - User targeting options

void CASUSetUserGender(int gender) {
    [[CAS targetingOptions] setGender:(Gender)gender];
}

void CASUSetUserAge(int age) {
    [[CAS targetingOptions] setAge:age];
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
                NSLog(@"[CAS] Framework bridge cant connect to CASTestSuit");
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

    NSLog(@"[CAS] Framework bridge cant find CASDebugger");
}

const char * CASUGetActiveMediationPattern(void) {
    return [CASUPluginUtil stringToUnity:[CASNetwork getActiveNetworkPattern]];
}

BOOL CASUIsActiveMediationNetwork(int net) {
    NSArray *values = [CASNetwork values];

    if (net > 0 && net < [values count]) {
        return [CASNetwork isActiveNetwork:[values objectAtIndex:net]];
    }

    return NO;
}

const char * CASUGetSDKVersion(void) {
    return [CASUPluginUtil stringToUnity:[CAS getSDKVersion]];
}

#pragma mark - CAS Manager

CASUManagerRef CASUCreateBuilder(NSInteger  enableAd,
                                 BOOL       demoAd,
                                 const char *unityVersion,
                                 const char *userID) {
    CASManagerBuilder *builder = [CAS buildManager];

    [builder withAdFlags:(CASTypeFlags)enableAd];
    [builder withTestAdMode:demoAd];
    [builder withFramework:@"Unity" version:[CASUPluginUtil stringFromUnity:unityVersion]];
    [builder withUserID:[CASUPluginUtil stringFromUnity:userID]];

    [[CASUPluginUtil sharedInstance] saveObject:builder withKey:@"lastBuilder"];

    return (__bridge CASUManagerRef)builder;
}

void CASUSetMediationExtras(CASManagerBuilderRef builderRef,
                            const char           **extraKeys,
                            const char           **extraValues,
                            NSInteger            extrasCount) {
    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    for (int i = 0; i < extrasCount; i++) {
        [builder withMediationExtras:[CASUPluginUtil stringFromUnity:extraValues[i]]
                              forKey:[CASUPluginUtil stringFromUnity:extraKeys[i]]];
    }
}

void CASUSetConsentFlow(CASManagerBuilderRef builderRef,
                        BOOL                 isEnabled,
                        const char           *policyUrl) {
    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    CASConsentFlow *flow = [[CASConsentFlow alloc] initWithEnabled:isEnabled];

    flow.privacyPolicyUrl = [CASUPluginUtil stringFromUnity:policyUrl];
    [builder withConsentFlow:flow];
}

CASUManagerRef CASUInitializeManager(CASManagerBuilderRef               builderRef,
                                     CASManagerClientRef                *client,
                                     CASUInitializationCompleteCallback onInit,
                                     const char                         *identifier) {
    NSString *nsIdentifier = [CASUPluginUtil stringFromUnity:identifier];

    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];

    CASManagerBuilder *builder = (__bridge CASManagerBuilder *)builderRef;

    if (onInit) {
        [builder withCompletionHandler:^(CASInitialConfig *config) {
            onInit(client,
                   config.error ? [config.error cStringUsingEncoding:NSUTF8StringEncoding] : NULL,
                   NO,
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

    [manager setLastPageAdFor:[CASUPluginUtil stringFromUnity:contentJson]];
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
    [cache removeObjectWithKey:[CASUPluginUtil stringFromUnity:key]];
}

void CASUAttachAdViewDelegate(CASUViewRef               viewRef,
                              CASUViewDidLoadCallback   didLoad,
                              CASUViewDidFailedCallback   didFailed,
                              CASUViewWillPresentCallback willPresent,
                              CASUViewDidClickedCallback  didClicked,
                              CASUViewDidRectCallback   didRect) {
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

        if (![network isEqualToString:CASNetwork.lastPageAd]) {
            if ([network isEqualToString:CASNetwork.fyber]) {
                network = CASNetwork.fairBid;
            }

            NSUInteger netIndex = [[CASNetwork values] indexOfObject:network];

            if (netIndex != NSNotFound) {
                return (int)netIndex;
            }
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
        return [CASUPluginUtil stringToUnity:internalImp.creativeIdentifier];
    }

    return NULL;
}

const char * CASUGetImpressionIdentifier(CASImpressionRef impression) {
    NSObject<CASStatusHandler> *internalImp = (__bridge NSObject<CASStatusHandler> *)impression;

    if (internalImp) {
        return [CASUPluginUtil stringToUnity:internalImp.identifier];
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

#pragma mark - ATT API

void CASURequestATT(CASUATTCompletion completion) {
    [CASInternalUtils trackingAuthorizationRequest:^(NSUInteger status) {
        completion(status);
    }];
}

NSUInteger CASUGetATTStatus(void) {
    return [CASInternalUtils adTrackingStatus];
}
