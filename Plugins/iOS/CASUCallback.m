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

- (instancetype)initWithType:(int)type
                      client:(CASManagerClientRef _Nonnull *)client {
    self = [super init];

    if (self) {
        _adType = type;
        _client = client;
    }

    return self;
}

- (void)willShownWithAd:(id<CASStatusHandler>)adStatus {
    [CASUPluginUtil onAdsWillPressent];

    if (self.impressionCallback) {
        self.impression = (NSObject<CASStatusHandler> *)adStatus;
        self.impressionCallback(self.client, kCASUAction_SHOWN, _adType, (__bridge CASImpressionRef)self.impression);
    }
}

- (void)didPayRevenueFor:(id<CASStatusHandler>)ad {
    if (self.impressionCallback) {
        self.impression = (NSObject<CASStatusHandler> *)ad;
        self.impressionCallback(self.client, kCASUAction_IMPRESSION, _adType, (__bridge CASImpressionRef)self.impression);
    }
}

- (void)didShowAdFailedWithError:(NSString *)error {
    [CASUPluginUtil onAdsDidClosed];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_SHOW_FAILED, _adType, (int)[CAS getErrorFor:error]);
    }
}

- (void)didCompletedAd {
    if (_adType == kCASUType_REWARD) {
        if (self.actionCallback) {
            self.actionCallback(self.client, kCASUAction_COMPLETED, _adType, 0);
        }
    }
}

- (void)didClickedAd {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_CLICKED, _adType, 0);
    }
}

- (void)didClosedAd {
#if __has_include("UnityInterface.h")
    extern bool _didResignActive;
    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }
#endif

    [CASUPluginUtil onAdsDidClosed];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_CLOSED, _adType, 0);
    }
}

- (UIViewController *)viewControllerForPresentingAppReturnAd {
    return [CASUPluginUtil unityGLViewController];
}

- (void)didAdLoaded {
    int adType = _adType;

    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.actionCallback) {
            self.actionCallback(self.client, kCASUAction_LOADED, adType, 0);
        }
    });
}

- (void)didAdFailedToLoadWithErrorCode:(int)error {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_FAILED, _adType, error);
    }
}

- (void)didAdFailedToLoadWithError:(NSString *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self didAdFailedToLoadWithErrorCode:(int)[CAS getErrorFor:error]];
    });
}

@end
