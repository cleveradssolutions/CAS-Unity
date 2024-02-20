//
//  CASUManager.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"

@implementation CASUManager

- (instancetype)initWithManager:(CASMediationManager *)manager forClient:(CASManagerClientRef _Nullable *)client {
    self = [super init];

    if (self) {
        _casManager = manager;
        _interCallback = [[CASUCallback alloc] initWithComplete:false];
        _interCallback.client = client;
        _rewardCallback = [[CASUCallback alloc] initWithComplete:true];
        _rewardCallback.client = client;
        _appReturnDelegate = [[CASUCallback alloc] initWithComplete:false];
        _appReturnDelegate.client = client;
        manager.adLoadDelegate = self;
    }

    return self;
}

- (void)presentInter {
    [_casManager presentInterstitialFromRootViewController:[CASUPluginUtil unityGLViewController]
                                                  callback:_interCallback];
}

- (void)presentReward {
    [_casManager presentRewardedAdFromRootViewController:[CASUPluginUtil unityGLViewController]
                                                callback:_rewardCallback];
}

- (void)setLastPageAdFor:(NSString *)content {
    self.casManager.lastPageAdContent = [CASLastPageAdContent createFrom:content];
}

- (void)onAdLoaded:(enum CASType)adType {
    // Callback called from any thread, so swith to UI thread for Unity.
    if (adType == CASTypeInterstitial) {
        [_interCallback callInUITheradLoadedCallback];
    } else if (adType == CASTypeRewarded) {
        [_rewardCallback callInUITheradLoadedCallback];
    }
}

- (void)onAdFailedToLoad:(enum CASType) adType withError:(NSString *)error {
    // Callback called from any thread, so swith to UI thread for Unity.
    if (adType == CASTypeInterstitial) {
        [_interCallback callInUITheradFailedToLoadCallbackWithError:error];
    } else if (adType == CASTypeRewarded) {
        [_rewardCallback callInUITheradFailedToLoadCallbackWithError:error];
    }
}

- (void)enableReturnAds {
    [_casManager enableAppReturnAdsWith:_appReturnDelegate];
}

- (void)disableReturnAds {
    [_casManager disableAppReturnAds];
}

- (void)skipNextAppReturnAd {
    [_casManager skipNextAppReturnAds];
}

@end
