//
//  CASUManager.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"

@interface CASUManager () {
    BOOL _appReturnEnabled;
}

@property (nonatomic, strong) NSMutableDictionary *viewReferences;

@end

@implementation CASUManager

- (instancetype)initWithManager:(CASMediationManager *)manager
                         client:(CASManagerClientRef _Nonnull *)client {
    self = [super init];

    if (self) {
        _casManager = manager;
        _viewReferences = [[NSMutableDictionary alloc] init];
        _interCallback = [[CASUCallback alloc] initWithType:kCASUType_APP_RETURN client:client];
        _rewardCallback = [[CASUCallback alloc] initWithType:kCASUType_REWARD client:client];
        _appOpenCallback = [[CASUCallback alloc] initWithType:kCASUType_APP_OPEN client:client];
    }

    return self;
}

- (void)dealloc {
    // Unregister for the notifications when the object is deallocated
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)enableAd:(int)type {
    BOOL isAutoload = [CAS.settings getLoadingMode] != CASLoadingManagerModeManual;

    switch (type) {
        case kCASUType_INTER:

            if (self.interstitialAd) {
                return;
            }

            self.interstitialAd = [[CASInterstitial alloc] initWithCasID:self.casManager.managerID];
            self.interstitialAd.delegate = self.interCallback;
            self.interstitialAd.impressionDelegate = self.interCallback;
            self.interstitialAd.isAutoshowEnabled = _appReturnEnabled;
            self.interstitialAd.isAutoloadEnabled = isAutoload;
            break;

        case kCASUType_REWARD:

            if (self.rewardedAd) {
                return;
            }

            self.rewardedAd = [[CASRewarded alloc] initWithCasID:self.casManager.managerID];
            self.rewardedAd.delegate = self.rewardCallback;
            self.rewardedAd.impressionDelegate = self.rewardCallback;
            self.rewardedAd.isAutoloadEnabled = isAutoload;
            break;

        case kCASUType_APP_OPEN:

            if (self.appOpenAd) {
                return;
            }

            self.appOpenAd = [[CASAppOpen alloc] initWithCasID:self.casManager.managerID];
            self.appOpenAd.delegate = self.appOpenCallback;
            self.appOpenAd.impressionDelegate = self.appOpenCallback;
            self.appOpenAd.isAutoloadEnabled = NO;         // disable by default
            break;

        default:
            NSLog(kCASULOGTAG kCASUMETHOD_NOT_SUPPORT @"%d", type);
            break;
    }
}

- (void)loadAd:(int)type {
    switch (type) {
        case kCASUType_INTER:

            if (!self.interstitialAd) {
                [self enableAd:type];

                if (self.interstitialAd.isAutoloadEnabled) {
                    return;
                }
            }

            [self.interstitialAd loadAd];
            break;

        case kCASUType_REWARD:

            if (!self.rewardedAd) {
                [self enableAd:type];

                if (self.rewardedAd.isAutoloadEnabled) {
                    return;
                }
            }

            [self.rewardedAd loadAd];
            break;

        case kCASUType_APP_OPEN:{
            if (!self.appOpenAd) {
                [self enableAd:type];

                if (self.appOpenAd.isAutoloadEnabled) {
                    return;
                }
            }

            [self.appOpenAd loadAd];
            break;
        }

        default:
            NSLog(kCASULOGTAG kCASUMETHOD_NOT_SUPPORT @"%d", type);
            break;
    }
}

- (BOOL)isAdReady:(int)type {
    switch (type) {
        case kCASUType_INTER:
            return self.interstitialAd && self.interstitialAd.isAdLoaded;

        case kCASUType_REWARD:
            return self.rewardedAd && self.rewardedAd.isAdLoaded;

        case kCASUType_APP_OPEN:
            return self.appOpenAd && self.appOpenAd.isAdLoaded;

        default:
            NSLog(kCASULOGTAG kCASUMETHOD_NOT_SUPPORT @"%d", type);
            return NO;
    }
}

- (void)showAd:(int)type {
    switch (type) {
        case kCASUType_INTER:
            // By default ad type sets to App Return for auto impressions.
            self.interCallback.adType = kCASUType_INTER;

            if (self.interstitialAd) {
                [self.interstitialAd presentFromViewController:[CASUPluginUtil unityWindow].rootViewController];
            } else {
                [self.interCallback didAdNotReadyToPresent];
            }

            break;

        case kCASUType_REWARD:

            if (self.rewardedAd) {
                CASUCallback *rewardCallback = self.rewardCallback;
                [self.rewardedAd presentFromViewController:[CASUPluginUtil unityWindow].rootViewController
                                  userDidEarnRewardHandler:^(CASContentInfo *_Nonnull info) {
                [rewardCallback didCompletedAd];
            }];
            } else {
                [self.rewardCallback didAdNotReadyToPresent];
            }

            break;

        case kCASUType_APP_OPEN:

            if (self.appOpenAd) {
                [self.appOpenAd presentFromViewController:[CASUPluginUtil unityWindow].rootViewController];
            } else {
                [self.appOpenCallback didAdNotReadyToPresent];
            }

            break;

        default:
            NSLog(kCASULOGTAG kCASUMETHOD_NOT_SUPPORT @"%d", type);
            break;
    }
}

- (void)destroyAd:(int)type {
    switch (type) {
        case kCASUType_INTER:

            if (self.interstitialAd) {
                [self.interstitialAd destroy];
                self.interstitialAd = nil;
            }

            break;

        case kCASUType_REWARD:

            if (self.rewardedAd) {
                [self.rewardedAd destroy];
                self.rewardedAd = nil;
            }

            break;

        case kCASUType_APP_OPEN:

            if (self.appOpenAd) {
                [self.appOpenAd destroy];
                self.appOpenAd = nil;
            }

            break;

        default:
            NSLog(kCASULOGTAG kCASUMETHOD_NOT_SUPPORT @"%d", type);
            break;
    }
}

- (CASUView *)createViewWithSize:(int)adSize client:(CASViewClientRef _Nullable *)adViewClient {
    CASUView *view = [[CASUView alloc] initWithCASID:self.casManager.managerID forClient:adViewClient size:adSize];

    self.viewReferences[[NSString stringWithFormat:@"%d", adSize]] = view;
    return view;
}

- (void)setLastPageAdFor:(NSString *)content {
    self.casManager.lastPageAdContent = [CASLastPageAdContent createFrom:content];
}

- (void)setAutoShowAdOnAppReturn:(BOOL)enabled {
    if (_appReturnEnabled == enabled) {
        return;
    }

    _appReturnEnabled = enabled;

    if (self.interstitialAd) {
        self.interstitialAd.isAutoshowEnabled = enabled;
    }
}

- (void)skipNextAppReturnAd {
    [self.casManager skipNextAppReturnAds];
}

@end
