//
//  CASUSettings.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUManager.h"
#import "CASUTypes.h"
#import "CASUPluginUtil.h"
#if __has_include("UnityAppController.h")
#import "UnityAppController.h"
#endif

static NSString * CASUStringFromUTF8String(const char *bytes)
{
    return bytes ? @(bytes) : nil;
}

static const char * cStringCopy(const char *string)
{
    if (!string) {
        return NULL;
    }
    char *res = (char *)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#pragma mark - CAS Settings

void CASUSetAnalyticsCollectionWithEnabled(BOOL enabled)
{
    [[CAS settings] setAnalyticsCollectionWithEnabled:enabled];
}

void CASUSetPluginPlatformWithName(const char *name, const char *version)
{
    [[CAS settings] setPluginPlatformWithName:CASUStringFromUTF8String(name) version:CASUStringFromUTF8String(version)];
}

void CASUSetTestDeviceWithIds(const char **testDeviceIDs, NSInteger testDeviceIDLength)
{
    NSMutableArray *testDeviceIDsArray = [[NSMutableArray alloc] init];
    for (int i = 0; i < testDeviceIDLength; i++) {
        [testDeviceIDsArray addObject:CASUStringFromUTF8String(testDeviceIDs[i])];
    }
    [[CAS settings] setTestDeviceWithIds:testDeviceIDsArray];
}

void CASUSetBannerRefreshWithInterval(NSInteger interval)
{
    [[CAS settings] setBannerRefreshWithInterval:interval];
}

void CASUSetInterstitialWithInterval(NSInteger interval)
{
    [[CAS settings] setInterstitialWithInterval:interval];
}

void CASURestartInterstitialInterval()
{
    [[CAS settings] restartInterstitialInterval];
}

void CASUUpdateUserConsent(NSInteger consent)
{
    [[CAS settings] updateUserWithConsent:(CASConsentStatus)consent];
}

void CASUUpdateCCPAWithStatus(NSInteger doNotSell)
{
    [[CAS settings] updateCCPAWithStatus:(CASCCPAStatus)doNotSell];
}

void CASUSetTaggedWithAudience(NSInteger audience)
{
    [[CAS settings] setTaggedWithAudience:(CASAudience)audience ];
}

void CASUSetDebugMode(BOOL mode)
{
    [[CAS settings] setDebugMode:mode ];
}

void CASUSetMuteAdSoundsTo(BOOL muted)
{
    [[CAS settings] setMuteAdSoundsTo:muted ];
}

void CASUSetLoadingWithMode(NSInteger mode)
{
    [[CAS settings] setLoadingWithMode:(CASLoadingManagerMode)mode];
}

void CASUSetInterstitialAdsWhenVideoCostAreLower(BOOL allow)
{
    [[CAS settings] setInterstitialAdsWhenVideoCostAreLowerWithAllow:allow];
}

void CASUSetiOSAppPauseOnBackground(BOOL pause)
{
    [CASUPluginUtil setPauseOnBackground:pause];
}

void CASUSetTrackLocationEnabled(BOOL enabled)
{
    [[CAS settings] setTrackLocationWithEnabled:enabled];
}

void CASUSetUserGender(NSInteger gender)
{
    [[CAS targetingOptions] setGender:(Gender)gender];
}

void CASUSetUserAge(NSInteger age)
{
    [[CAS targetingOptions] setAge:age];
}

void CASUValidateIntegration()
{
    [CAS validateIntegration];
}

void CASUOpenDebugger(CASUTypeManagerRef manager)
{
#if __has_include("UnityAppController.h")
    UIStoryboard *storyboard =
        [UIStoryboard storyboardWithName:@"CASDebugger"
                                  bundle:[NSBundle bundleForClass:[CASUManager class]]];
    if (storyboard) {
        UIViewController *vc = [storyboard instantiateViewControllerWithIdentifier:@"DebuggerController"];
        if (vc) {
            UIViewController *root = ((UnityAppController *)[UIApplication sharedApplication].delegate).rootViewController;
            
            SEL selector = NSSelectorFromString(@"setTargetManager:");
            if (![vc respondsToSelector:selector]){
                NSLog(@"[CAS] Framework bridge cant connect to CASDebugger");
                return;
            }
            
            CASUManager *internalManager = (__bridge CASUManager *)manager;
            [vc performSelector:selector withObject:[internalManager mediationManager]];
            vc.modalPresentationStyle = UIModalPresentationFullScreen;
            [root presentViewController:vc animated:YES completion:nil];
            return;
        }
    }
#endif
    NSLog(@"[CAS] Framework bridge cant find CASDebugger");
}

const char * CASUGetActiveMediationPattern()
{
    return cStringCopy([CASNetwork getActiveNetworkPattern].UTF8String);
}

BOOL CASUIsActiveMediationNetwork(NSInteger net)
{
    NSArray *values = [CASNetwork values];
    if (net > 0 && net < [values count]) {
        return [CASNetwork isActiveNetwork:[values objectAtIndex:net]];
    }
    return NO;
}

const char * CASUGetSDKVersion()
{
    return cStringCopy([CAS getSDKVersion].UTF8String);
}

#pragma mark - CAS Manager

CASUTypeManagerRef CASUCreateManager(CASUTypeManagerClientRef           *client,
                                     CASUInitializationCompleteCallback onInit,
                                     const char                         *managerID,
                                     NSInteger                          enableAd,
                                     BOOL                               demoAd)
{
    CASUManager *manager = [[CASUManager alloc]
                            initWithAppID:CASUStringFromUTF8String(managerID)
                                    enable:enableAd
                                    demoAd:demoAd
                                 forClient:client
                           mediationExtras:nil
                                    onInit:onInit];

    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    [cache saveObject:manager withKey:manager.mediationManager.managerID];
    return (__bridge CASUTypeManagerRef)manager;
}

CASUTypeManagerRef CASUCreateManagerWithExtras(CASUTypeManagerClientRef           *client,
                                               CASUInitializationCompleteCallback onInit,
                                               const char                         *managerID,
                                               NSInteger                          enableAd,
                                               BOOL                               demoAd,
                                               const char                         **extraKeys,
                                               const char                         **extraValues,
                                               NSInteger                          extrasCount)
{
    NSMutableDictionary *mediationExtras = [[NSMutableDictionary<NSString *, NSString *> alloc] init];
    for (int i = 0; i < extrasCount; i++) {
        [mediationExtras setObject:CASUStringFromUTF8String(extraKeys[i]) forKey:CASUStringFromUTF8String(extraValues[i])];
    }
    CASUManager *manager = [[CASUManager alloc]
                            initWithAppID:CASUStringFromUTF8String(managerID)
                                    enable:enableAd
                                    demoAd:demoAd
                                 forClient:client
                           mediationExtras:mediationExtras
                                    onInit:onInit];

    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    [cache saveObject:manager withKey:manager.mediationManager.managerID];
    return (__bridge CASUTypeManagerRef)manager;
}

void CASUFreeManager(CASUTypeManagerRef manager)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    internalManager.mediationManager.adLoadDelegate = nil;
    internalManager.bannerView.delegate = nil;
    internalManager.bannerCallback = nil;
    internalManager.interstitialCallback = nil;
    internalManager.rewardedCallback = nil;
    CASUPluginUtil *cache = [CASUPluginUtil sharedInstance];
    [cache removeObjectWithKey:internalManager.mediationManager.managerID];
}

void CASUSetLoadAdDelegate(CASUTypeManagerRef            manager,
                           CASUDidAdLoadedCallback       didLoaded,
                           CASUDidAdFailedToLoadCallback didFailedToLoad)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    internalManager.didAdLoadedCallback = didLoaded;
    internalManager.didAdFailedToLoadCallback = didFailedToLoad;
    internalManager.mediationManager.adLoadDelegate = internalManager;
}

void CASUSetBannerDelegate(CASUTypeManagerRef                   manager,
                           CASUWillShownWithAdCallback          willShow,
                           CASUDidShowAdFailedWithErrorCallback didShowWithError,
                           CASUDidClickedAdCallback             didClick,
                           CASUDidClosedAdCallback              didClosed)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    internalManager.bannerCallback.willShownCallback = willShow;
    internalManager.bannerCallback.didShowFailedCallback = didShowWithError;
    internalManager.bannerCallback.didClickCallback = didClick;
    internalManager.bannerCallback.didClosedCallback = didClosed;
}

void CASUSetInterstitialDelegate(CASUTypeManagerRef                   manager,
                                 CASUWillShownWithAdCallback          willShow,
                                 CASUDidShowAdFailedWithErrorCallback didShowWithError,
                                 CASUDidClickedAdCallback             didClick,
                                 CASUDidClosedAdCallback              didClosed)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    internalManager.interstitialCallback.willShownCallback = willShow;
    internalManager.interstitialCallback.didShowFailedCallback = didShowWithError;
    internalManager.interstitialCallback.didClickCallback = didClick;
    internalManager.interstitialCallback.didClosedCallback = didClosed;
}

void CASUSetRewardedDelegate(CASUTypeManagerRef                   manager,
                             CASUWillShownWithAdCallback          willShow,
                             CASUDidShowAdFailedWithErrorCallback didShowWithError,
                             CASUDidClickedAdCallback             didClick,
                             CASUDidCompletedAdCallback           didComplete,
                             CASUDidClosedAdCallback              didClosed)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    internalManager.rewardedCallback.willShownCallback = willShow;
    internalManager.rewardedCallback.didShowFailedCallback = didShowWithError;
    internalManager.rewardedCallback.didClickCallback = didClick;
    internalManager.rewardedCallback.didCompleteCallback = didComplete;
    internalManager.rewardedCallback.didClosedCallback = didClosed;
}

void CASULoadAdWithType(CASUTypeManagerRef manager, NSInteger adType)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager load:(CASType)adType];
}

void CASUShowAdWithType(CASUTypeManagerRef manager, NSInteger adType)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager show:(CASType)adType];
}

void CASUHideBanner(CASUTypeManagerRef manager)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager hideBanner];
}

BOOL CASUIsAdReadyWithType(CASUTypeManagerRef manager, NSInteger adType)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    return [internalManager.mediationManager isAdReadyWithType:(CASType)adType];
}

const char * CASUGetLastActiveMediationWithType(CASUTypeManagerRef manager, NSInteger adType)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    return cStringCopy([internalManager.mediationManager getLastActiveMediationWithType:(CASType)adType].UTF8String);
}

BOOL CASUIsAdEnabledType(CASUTypeManagerRef manager, NSInteger adType)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    return [internalManager.mediationManager isAdReadyWithType:(CASType)adType];
}

void CASUEnableAdType(CASUTypeManagerRef manager, NSInteger adType, BOOL enable)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager.mediationManager setEnabled:enable type:(CASType)adType];
}

void CASUSetBannerSize(CASUTypeManagerRef manager, NSInteger bannerSize)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [ internalManager setBannerSize:bannerSize];
}

void CASUSetBannerPosition(CASUTypeManagerRef manager, NSInteger bannerPos)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager setBannerPosition:bannerPos];
}

float CASUGetBannerHeightInPixels(CASUTypeManagerRef manager)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    return internalManager.bannerHeightInPixels;
}

float CASUGetBannerWidthInPixels(CASUTypeManagerRef manager)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    return internalManager.bannerWidthInPixels;
}

void CASUSetLastPageAdContent(CASUTypeManagerRef manager, const char *contentJson)
{
    CASUManager *internalManager = (__bridge CASUManager *)manager;
    [internalManager setLastPageAdFor:CASUStringFromUTF8String(contentJson)];
}
