//
//  CASUSettings.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <CleverAdsSolutions/CleverAdsSolutions.h>
#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"
#import "CASUTypes.h"
#import "CASUView.h"


/// Returns an NSString copying the characters from |bytes|, a C array of UTF8-encoded bytes.
/// Returns nil if |bytes| is NULL.
static NSString * CASUStringFromUnity(const char *bytes) {
    return bytes ? @(bytes) : nil;
}

/// Returns a C string from a C array of UTF8-encoded bytes.
const char * CASUStringToUnity(NSString *str) {
    if (!str) {
        return NULL;
    }

    const char *string = str.UTF8String;
    char *res = (char *)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#pragma mark - CAS Settings

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

int CASUGetVendorConsent(int vendorId) {
    return (int)[CAS.settings getVendorConsentWithVendorId:vendorId];
}

int CASUGetAdditionalConsent(int providerId) {
    return (int)[CAS.settings getAdditionalConsentWithProviderId:providerId];
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
    [CAS.targetingOptions setLocationCollectionEnabled:enabled];
}

BOOL CASUGetTrackLocationEnabled(void) {
    return [CAS.targetingOptions getLocationCollectionEnabled];
}

#pragma mark - User targeting options

void CASUSetUserGender(int gender) {
    [CAS.targetingOptions setGender:(CASGender)gender];
}

int CASUGetUserGender(void) {
    return (int)[CAS.targetingOptions getGender];
}

void CASUSetUserAge(int age) {
    [CAS.targetingOptions setAge:age];
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

float CASUGetDeviceScreenScale(void) {
    return [UIScreen mainScreen].scale;
}

#pragma mark - CAS Manager

void CASUCreateBuilder(NSInteger  enableAd,
                       BOOL       demoAd,
                       const char *unityVersion,
                       const char *userID) {
    CASManagerBuilder *builder = [CAS buildManager];

    [builder withAdFlags:(CASTypeFlags)enableAd];
    [builder withTestAdMode:demoAd];
    [builder withFramework:@"Unity" version:CASUStringFromUnity(unityVersion)];
    [builder withUserID:CASUStringFromUnity(userID)];

    [CASUPluginUtil sharedInstance].builder = builder;
}

void CASUSetMediationExtras(const char **extraKeys,
                            const char **extraValues,
                            NSInteger  extrasCount) {
    CASManagerBuilder *builder = [CASUPluginUtil sharedInstance].builder;

    for (int i = 0; i < extrasCount; i++) {
        [builder withMediationExtras:CASUStringFromUnity(extraValues[i])
                              forKey:CASUStringFromUnity(extraKeys[i])];
    }
}

void CASUSetConsentFlow(BOOL                      isEnabled,
                        int                       geography,
                        const char                *policyUrl,
                        CASUConsentFlowCompletion completion) {
    CASManagerBuilder *builder = [CASUPluginUtil sharedInstance].builder;

    CASConsentFlow *flow = [[CASConsentFlow alloc] initWithEnabled:isEnabled];

    flow.privacyPolicyUrl = CASUStringFromUnity(policyUrl);
    flow.debugGeography = (CASUserDebugGeography)geography;

    if (completion) {
        flow.completionHandler = ^(enum CASConsentFlowStatus status) {
            completion((int)status);
        };
    }

    [builder withConsentFlow:flow];
}

void CASUDisableConsentFlow(void) {
    CASManagerBuilder *builder = [CASUPluginUtil sharedInstance].builder;

    [builder withConsentFlow:[[CASConsentFlow alloc]initWithEnabled:false]];
}

CASUManagerRef CASUBuildManager(CASManagerClientRef                *client,
                                CASUInitializationCompleteCallback onInit,
                                const char                         *identifier) {
    NSString *nsIdentifier = CASUStringFromUnity(identifier);

    CASManagerBuilder *builder = [CASUPluginUtil sharedInstance].builder;

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
    CASUManager *wrapper = [[CASUManager alloc] initWithManager:manager client:client];
    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    cache.builder = nil;
    [cache saveObject:wrapper withKey:nsIdentifier];
    return (__bridge CASUManagerRef)wrapper;
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

void CASUSetDelegates(CASUManagerRef         managerRef,
                      CASUActionCallback     actionCallback,
                      CASUImpressionCallback impressionCallback) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    manager.interCallback.actionCallback = actionCallback;
    manager.interCallback.impressionCallback = impressionCallback;

    manager.rewardCallback.actionCallback = actionCallback;
    manager.rewardCallback.impressionCallback = impressionCallback;

    manager.appOpenCallback.actionCallback = actionCallback;
    manager.appOpenCallback.impressionCallback = impressionCallback;

    manager.appReturnDelegate.actionCallback = actionCallback;
    manager.appReturnDelegate.impressionCallback = impressionCallback;
}

void CASUShowAd(CASUManagerRef managerRef, int type) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager showAd:type];
}

void CASULoadAd(CASUManagerRef managerRef, int type) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    [manager loadAd:type];
}

BOOL CASUIsAdReady(CASUManagerRef managerRef, int type) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;

    return [manager isAdReady:type];
}

#pragma mark - AdView

CASUViewRef CASUCreateAdView(CASUManagerRef   managerRef,
                             CASViewClientRef *client,
                             int              adSizeCode) {
    CASUManager *manager = (__bridge CASUManager *)managerRef;
    CASUView *view = [manager createViewWithSize:adSizeCode client:client];

    return (__bridge CASUViewRef)view;
}

void CASUDestroyAdView(CASUViewRef viewRef) {
    if(viewRef){
        CASUView *view = (__bridge CASUView *)viewRef;
        
        if (view) {
            dispatch_async(dispatch_get_main_queue(), ^{
                [view destroy];
            });
        }
    }
}

void CASUAttachAdViewDelegate(CASUViewRef                viewRef,
                              CASUViewActionCallback     actionCallback,
                              CASUViewImpressionCallback impressionCallback,
                              CASUViewRectCallback       rectCallback) {
    CASUView *view = (__bridge CASUView *)viewRef;

    view.actionCallback = actionCallback;
    view.impressionCallback = impressionCallback;
    view.rectCallback = rectCallback;
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

void CASUSetAdViewPositionPx(CASUViewRef viewRef, int posCode, int x, int y) {
    CASUView *view = (__bridge CASUView *)viewRef;

    if (x == 0 && y == 0) {
        [view setPositionCode:posCode withX:0 withY:0];
        return;
    }

    CGFloat scale = [UIScreen mainScreen].scale;
    [view setPositionCode:posCode withX:(x / scale) withY:(y / scale)];
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

void CASUSetAutoShowAdOnAppReturn(CASUManagerRef manager, BOOL enabled) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;

    [internalManager setAutoShowAdOnAppReturn:enabled];
}

void CASUSkipNextAppReturnAds(CASUManagerRef manager) {
    CASUManager *internalManager = (__bridge CASUManager *)manager;

    [internalManager skipNextAppReturnAd];
}

#pragma mark - Ad Impression

int CASUGetImpressionNetwork(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
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
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return internalImp.cpm;
    }

    return 0.0;
}

int CASUGetImpressionPrecission(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return (int)internalImp.priceAccuracy;
    }

    return (int)CASPriceAccuracyUndisclosed;
}

const char * CASUGetImpressionCreativeId(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return CASUStringToUnity(internalImp.creativeIdentifier);
    }

    return NULL;
}

const char * CASUGetImpressionIdentifier(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return CASUStringToUnity(internalImp.identifier);
    }

    return NULL;
}

int CASUGetImpressionDepth(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return (int)internalImp.impressionDepth;
    }

    return 0;
}

double CASUGetImpressionLifetimeRevenue(CASImpressionRef impression) {
    if (impression) {
        id<CASStatusHandler> internalImp = (__bridge id<CASStatusHandler>)impression;
        return internalImp.lifetimeRevenue;
    }

    return 0.0;
}

#pragma mark - Consent Flow

void CASUShowConsentFlow(BOOL                      ifRequired,
                         BOOL                      testing,
                         int                       geography,
                         const char                *policy,
                         CASUConsentFlowCompletion completion) {
    CASConsentFlow *flow = [[CASConsentFlow alloc] init];

    flow.privacyPolicyUrl = CASUStringFromUnity(policy);
    flow.debugGeography = (CASUserDebugGeography)geography;
    flow.forceTesting = testing;

    if (completion) {
        flow.completionHandler = ^(enum CASConsentFlowStatus status) {
            completion((int)status);
        };
    }

    if (ifRequired) {
        [flow presentIfRequired];
    } else {
        [flow present];
    }
}

#pragma mark - ATT API

void CASURequestATT(CASUConsentFlowCompletion completion) {
    [CASInternalUtils trackingAuthorizationRequest:^(NSUInteger status) {
        completion((int)status);
    }];
}

NSUInteger CASUGetATTStatus(void) {
    NSUInteger status = [CASInternalUtils adTrackingStatus];

    if (status > 3) {
        return 0;
    }

    return status;
}
