//
//  CASUCallback.m
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import "CASUCallback.h"
#import "CASUPluginUtil.h"
#if __has_include("UnityInterface.h")
#import "UnityInterface.h"
#endif
#if __has_include(<FirebaseAnalytics/FIRAnalytics.h>)
#import <FirebaseAnalytics/FIRAnalytics.h>
#endif

@implementation CASUCallback
{
    BOOL fullScreenAd;
}

- (id)initForFullScreen:(BOOL)isFullScreen {
    self = [super init];
    if (self) {
        fullScreenAd = isFullScreen;
    }
    return self;
}

- (void)willShownWithAd:(id<CASStatusHandler>)adStatus {
#if __has_include("UnityInterface.h")
    if (fullScreenAd) {
        if ([CASUPluginUtil pauseOnBackground]) {
            UnityPause(YES);
        }
    }
#endif

    if (self.willShownCallback) {
        if (self.client) {
            self.willShownCallback(self.client);
        }
    }
}

- (void)didShowAdFailedWithError:(NSString *)error {
#if __has_include("UnityInterface.h")
    if (fullScreenAd) {
        if (UnityIsPaused()) {
            UnityPause(NO);
        }
    }
#endif
    if (self.didShowFailedCallback) {
        if (self.client) {
            self.didShowFailedCallback(self.client, [error cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }
}

- (void)didCompletedAd {
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
    // didClosedAd is called from CAS internal thread
#if __has_include("UnityInterface.h")
    if (fullScreenAd) {
        if (UnityIsPaused()) {
            UnityPause(NO);
        }
    }
#endif
    if (self.didClosedCallback) {
        if (self.client) {
            self.didClosedCallback(self.client);
        }
    }
}

- (void)log:(NSString *)eventName:(NSDictionary<NSString *, id> *)map {
#if __has_include(<FirebaseAnalytics/FIRAnalytics.h>)
    [FIRAnalytics logEventWithName:eventName parameters:map];
#else
    NSLog(@"[CAS] Framework bridge cant find Firebase Analytics");
#endif
}

@end
