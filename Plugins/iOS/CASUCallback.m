//
//  CASUCallback.m
//  CASUnityPlugin
//
//  Copyright © 2024 CAS.AI. All rights reserved.
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

- (void)didPayRevenueFor:(id<CASStatusHandler>)ad {
    if (self.client) {
        if (self.didImpressionCallback) {
            _lastImpression = (NSObject<CASStatusHandler> *)ad;
            self.didImpressionCallback(self.client, (__bridge CASImpressionRef)_lastImpression);
        }
    }
}

- (void)didShowAdFailedWithError:(NSString *)error {
    [CASUPluginUtil onAdsDidClosed];

    if (self.client) {
        if (self.didShowFailedCallback) {
            
            self.didShowFailedCallback(self.client, (int)[CAS getErrorFor:error]);
        }
    }
}

- (void)didCompletedAd {
    if (!withComplete) {
        return;
    }

    if (self.client) {
        if (self.didCompleteCallback) {
            self.didCompleteCallback(self.client);
        }
    }
}

- (void)didClickedAd {
    if (self.client) {
        if (self.didClickCallback) {
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

    if (self.client) {
        if (self.didClosedCallback) {
            self.didClosedCallback(self.client);
        }
    }
}

- (UIViewController *)viewControllerForPresentingAppReturnAd {
    return [CASUPluginUtil unityGLViewController];
}

- (void)callInUITheradLoadedCallback {
    if (self.client) {
        if (self.didLoadedCallback) {
            dispatch_async(dispatch_get_main_queue(), ^{
                self.didLoadedCallback(self.client);
            });
        }
    }
}

- (void)callInUITheradFailedToLoadCallbackWithError:(NSString *)error {
    if (self.client) {
        if (self.didFailedCallback) {
            dispatch_async(dispatch_get_main_queue(), ^{
                self.didFailedCallback(self.client, (int)[CAS getErrorFor:error]);
            });
        }
    }
}

@end
