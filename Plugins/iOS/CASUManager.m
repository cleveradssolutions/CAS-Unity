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

- (instancetype)initWithManager:(CASMediationManager *)manager
                         client:(CASManagerClientRef _Nonnull *)client {
    self = [super init];

    if (self) {
        _casManager = manager;
        _client = client;
        _interCallback = [[CASUCallback alloc] initWithType:kCASUType_INTER];
        _rewardCallback = [[CASUCallback alloc] initWithType:kCASUType_REWARD];
        _appReturnDelegate = [[CASUCallback alloc] initWithType:kCASUType_APP_RETURN];
        _appOpenCallback = [[CASUCallback alloc] initWithType:kCASUType_APP_OPEN];
        _appOpenAd = [CASAppOpen createWithManager:manager];

        _interCallback.manager = self;
        _rewardCallback.manager = self;
        _appReturnDelegate.manager = self;
        _appOpenCallback.manager = self;
        manager.adLoadDelegate = self;
    }

    return self;
}

- (void)dealloc {
    // Unregister for the notifications when the object is deallocated
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)loadAd:(int)type {
    switch (type) {
        case kCASUType_INTER:
            [self.casManager loadInterstitial];
            break;

        case kCASUType_REWARD:
            [self.casManager loadRewardedAd];
            break;

        case kCASUType_APP_OPEN:{
            __weak CASUManager *weakSelf = self;
            [self.appOpenAd loadAdWithCompletionHandler:^(CASAppOpen *_Nonnull ad, NSError *_Nullable error) {
                CASUManager *strongSelf = weakSelf;

                if (strongSelf) {
                    if (error) {
                        [strongSelf.appOpenCallback didAdFailedToLoadWithErrorCode:(int)error.code];
                    } else {
                        [strongSelf.appOpenCallback didAdLoaded];
                    }
                }
            }];
            break;
        }

        default:
            break;
    }
}

- (BOOL)isAdReady:(int)type {
    switch (type) {
        case kCASUType_INTER:
            return self.casManager.isInterstitialReady;

        case kCASUType_REWARD:
            return self.casManager.isRewardedAdReady;

        case kCASUType_APP_OPEN:
            return [self.appOpenAd isAdAvailable];

        default:
            return NO;
    }
}

- (void)showAd:(int)type {
    switch (type) {
        case kCASUType_INTER:
            [self.casManager presentInterstitialFromRootViewController:[CASUPluginUtil unityGLViewController]
                                                              callback:_interCallback];
            break;

        case kCASUType_REWARD:
            [self.casManager presentRewardedAdFromRootViewController:[CASUPluginUtil unityGLViewController]
                                                            callback:_rewardCallback];
            break;

        case kCASUType_APP_OPEN:
            self.appOpenAd.contentCallback = self.appOpenCallback;
            [self.appOpenAd presentFromRootViewController:[CASUPluginUtil unityGLViewController]];
            break;

        default:
            break;
    }
}

- (void)setLastPageAdFor:(NSString *)content {
    self.casManager.lastPageAdContent = [CASLastPageAdContent createFrom:content];
}

- (void)onAdLoaded:(enum CASType)adType {
    // Callback called from any thread, so swith to UI thread for Unity.
    if (adType == CASTypeInterstitial) {
        [_interCallback didAdLoaded];
    } else if (adType == CASTypeRewarded) {
        [_rewardCallback didAdLoaded];
    }
}

- (void)onAdFailedToLoad:(enum CASType) adType withError:(NSString *)error {
    // Callback called from any thread, so swith to UI thread for Unity.
    if (adType == CASTypeInterstitial) {
        [_interCallback didAdFailedToLoadWithError:error];
    } else if (adType == CASTypeRewarded) {
        [_rewardCallback didAdFailedToLoadWithError:error];
    }
}

- (void)setAutoShowAdOnAppReturn:(BOOL)enabled {
    if (enabled) {
        [self.casManager enableAppReturnAdsWith:_appReturnDelegate];
    } else {
        [self.casManager disableAppReturnAds];
    }
}

- (void)skipNextAppReturnAd {
    [self.casManager skipNextAppReturnAds];
}

@end
