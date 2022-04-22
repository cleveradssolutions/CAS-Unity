//
//  CASUManager.m
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import "CASUPluginUtil.h"

@implementation CASUManager

- (id)initWithAppID:(NSString *)appID
             enable:(NSUInteger)types
             demoAd:(BOOL)demoAd
          forClient:(CASUTypeManagerClientRef *)client
    mediationExtras:(NSMutableDictionary<NSString *, NSString *> *_Nullable)extras
             onInit:(CASUInitializationCompleteCallback)onInit {
    self = [super init];
    if (self) {
        _client = client;
        _interCallback = [[CASUCallback alloc] initWithComplete:false];
        _interCallback.client = client;
        _rewardCallback = [[CASUCallback alloc] initWithComplete:true];
        _rewardCallback.client = client;
        _appReturnDelegate = [[CASUCallback alloc] initWithComplete:false];
        _appReturnDelegate.client = client;

        [CASAnalytics setDelegate:_interCallback]; // Require before create manager

//        NSMutableDictionary *mediationExtras;
//        if (extras) {
//            mediationExtras = extras;
//        } else {
//            mediationExtras = [[NSMutableDictionary<NSString *, NSString *> alloc] init];
//        }
        
        self.casManager =
            [CAS createWithManagerID:appID
                         enableTypes:types
                          demoAdMode:demoAd
                     mediationExtras:extras
                              onInit:^(BOOL succses, NSString *_Nullable error) {
                                  if (onInit) {
                                      if (error) {
                                          onInit(client, succses, [error cStringUsingEncoding:NSUTF8StringEncoding]);
                                      } else {
                                          onInit(client, succses, NULL);
                                      }
                                  }
                              }];
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
    if (adType == CASTypeInterstitial) {
        if (self.interCallback) {
            if (self.interCallback.didLoadedCallback) {
                self.interCallback.didLoadedCallback(self.client);
            }
        }
    } else if (adType == CASTypeRewarded) {
        if (self.rewardCallback) {
            if (self.rewardCallback.didLoadedCallback) {
                self.rewardCallback.didLoadedCallback(self.client);
            }
        }
    }
}

- (void)onAdFailedToLoad:(enum CASType) adType withError:(NSString *)error {
    if (adType == CASTypeInterstitial) {
        if (self.interCallback) {
            if (self.interCallback.didFailedCallback) {
                self.interCallback.didFailedCallback(self.client, [self getErrorCodeFromString:error]);
            }
        }
    } else if (adType == CASTypeRewarded) {
        if (self.rewardCallback) {
            if (self.rewardCallback.didFailedCallback) {
                self.rewardCallback.didFailedCallback(self.client, [self getErrorCodeFromString:error]);
            }
        }
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

- (NSInteger)getErrorCodeFromString:(NSString *)error {
    if (!error) {
        return CASErrorInternalError;
    }

    return [error isEqualToString:@"No internet connection detected"] ? CASErrorNoConnection
        : [error isEqualToString:@"No Fill"] ? CASErrorNoFill
        : [error isEqualToString:@"Ad are not ready. You need to call Load ads or use one of the automatic cache mode."] ? CASErrorNotReady
        : [error isEqualToString:@"Manager is disabled"] ? CASErrorManagerIsDisabled
        : [error isEqualToString:@"Reached cap for user"] ? CASErrorReachedCap
        : [error isEqualToString:@"The interval between impressions Ad has not yet passed."] ? CASErrorIntervalNotYetPassed
        : [error isEqualToString:@"Ad already displayed"] ? CASErrorAlreadyDisplayed
        : [error isEqualToString:@"Application is paused"] ? CASErrorAppIsPaused
        : [error isEqualToString:@"Not enough space to display ads"] ? CASErrorNotEnoughSpace
        : CASErrorInternalError;
}

@end
