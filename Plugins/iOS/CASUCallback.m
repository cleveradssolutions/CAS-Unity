//
//  CASUCallback.m
//  CASUnityPlugin
//
//  Copyright © 2022 Clever Ads Solutions. All rights reserved.
//

#import "CASUCallback.h"
#import "CASUPluginUtil.h"

@implementation CASUCallback{
    BOOL withComplete;
    NSObject<CASStatusHandler> *_lastImpression;
}

- (instancetype)initWithComplete:(BOOL)complete {
    self = [super init];

    if (self) {
        withComplete = complete;
    }

    return self;
}

- (void)willShownWithAd:(id<CASStatusHandler>)adStatus {
    [CASUPluginUtil onAdsWillPressent];

    if (self.client) {
        if (self.willOpeningCallback) {
            _lastImpression = (NSObject<CASStatusHandler> *)adStatus;
            self.willOpeningCallback(self.client, (__bridge CASImpressionRef)_lastImpression);
        }
    }
}

- (void)didShowAdFailedWithError:(NSString *)error {
    [CASUPluginUtil onAdsDidClosed];

    if (self.didShowFailedCallback) {
        if (self.client) {
            self.didShowFailedCallback(self.client, [error cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }
}

- (void)didCompletedAd {
    if (!withComplete) {
        return;
    }

    if (self.didCompleteCallback) {
        if (self.client) {
            self.didCompleteCallback(self.client);
        }
    }
}

- (void)didClickedAd {
    if (self.didClickCallback) {
        if (self.client) {
            self.didClickCallback(self.client);
        }
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

    if (self.didClosedCallback) {
        if (self.client) {
            self.didClosedCallback(self.client);
        }
    }
}

- (UIViewController *)viewControllerForPresentingAppReturnAd {
    return [CASUPluginUtil unityGLViewController];
}

- (void)callInUITheradLoadedCallback {
    if (self.didLoadedCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.didLoadedCallback(self.client);
        });
    }
}

- (void)callInUITheradFailedToLoadCallbackWithError:(NSString *)error {
    if (self.didFailedCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.didFailedCallback(self.client, [self getErrorCodeFromString:error]);
        });
    }
}

- (NSInteger)getErrorCodeFromString:(NSString *)error {
    if (!error) {
        return CASErrorInternalError;
    }

    return [error isEqualToString:@"No internet connection detected"] ? CASErrorNoConnection
    : [error isEqualToString:@"No Fill"] ? CASErrorNoFill
    : [error isEqualToString:@"Invalid configuration"] ? CASErrorConfigurationError
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
