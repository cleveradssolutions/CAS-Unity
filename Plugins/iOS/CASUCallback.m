//
//  CASUCallback.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import "CASUCallback.h"
#import "CASUManager.h"
#import "CASUPluginUtil.h"

@implementation CASUCallback{
    int _adType;
}

- (instancetype)initWithType:(int)type {
    self = [super init];

    if (self) {
        _adType = type;
    }

    return self;
}

- (void)willShownWithAd:(id<CASStatusHandler>)adStatus {
    [CASUPluginUtil onAdsWillPressent];

    if (self.manager.willOpeningCallback) {
        self.impression = (NSObject<CASStatusHandler> *)adStatus;
        self.manager.willOpeningCallback(self.manager.client, _adType, (__bridge CASImpressionRef)self.impression);
    }
}

- (void)didPayRevenueFor:(id<CASStatusHandler>)ad {
    if (self.manager.didImpressionCallback) {
        self.impression = (NSObject<CASStatusHandler> *)ad;
        self.manager.didImpressionCallback(self.manager.client, _adType, (__bridge CASImpressionRef)self.impression);
    }
}

- (void)didShowAdFailedWithError:(NSString *)error {
    [CASUPluginUtil onAdsDidClosed];

    if (self.manager.didShowFailedCallback) {
        self.manager.didShowFailedCallback(self.manager.client, _adType, (int)[CAS getErrorFor:error]);
    }
}

- (void)didCompletedAd {
    if (_adType == kCASUType_REWARD) {
        if (self.manager.didCompleteCallback) {
            self.manager.didCompleteCallback(self.manager.client, _adType);
        }
    }
}

- (void)didClickedAd {
    if (self.manager.didClickCallback) {
        self.manager.didClickCallback(self.manager.client, _adType);
    }
}

- (void)didClosedAd {
    // Escape from callback when App on background. Not supported for Cross Promo logic.
    //    extern bool _didResignActive;
    //    if (_didResignActive) {
    //        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
    //        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
    //        return;
    //    }

    [CASUPluginUtil onAdsDidClosed];

    if (self.manager.didClosedCallback) {
        self.manager.didClosedCallback(self.manager.client, _adType);
    }
}

- (UIViewController *)viewControllerForPresentingAppReturnAd {
    return [CASUPluginUtil unityGLViewController];
}

- (void)didAdLoaded {
    if (self.manager.didLoadedCallback) {
        int adType = _adType;
        dispatch_async(dispatch_get_main_queue(), ^{
            if (self.manager.didLoadedCallback) {
                self.manager.didLoadedCallback(self.manager.client, adType);
            }
        });
    }
}

- (void)didAdFailedToLoadWithErrorCode:(int)error {
    if (self.manager.didFailedCallback) {
        self.manager.didFailedCallback(self.manager.client, _adType, error);
    }
}

- (void)didAdFailedToLoadWithError:(NSString *)error {
    if (self.manager) {
        dispatch_async(dispatch_get_main_queue(), ^{
            [self didAdFailedToLoadWithErrorCode:(int)[CAS getErrorFor:error]];
        });
    }
}

@end
