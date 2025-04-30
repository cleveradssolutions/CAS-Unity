//
//  CASUCallback.m
//  CASUnityPlugin
//
//  Copyright © 2025 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import "CASUCallback.h"
#import "CASUManager.h"
#import "CASUPluginUtil.h"

const char * CASUStringToUnity(NSString *str);

@implementation CASUCallback

- (instancetype)initWithType:(int)type
                      client:(CASManagerClientRef _Nonnull *)client {
    self = [super init];

    if (self) {
        _adType = type;
        _client = client;
    }

    return self;
}


- (void)didCompletedAd {
    if (self.adType == kCASUType_REWARD) {
        extern bool _didResignActive;

        if (_didResignActive) {
            return;
        }

        if (self.actionCallback) {
            self.actionCallback(self.client, kCASUAction_COMPLETED, self.adType, 0, NULL);
        }
    }
}
- (void)didAdNotReadyToPresent{
    if (self.actionCallback) {
        CASError* error = CASError.notReady;
        self.actionCallback(self.client, kCASUAction_SHOW_FAILED, self.adType, (int)error.code, CASUStringToUnity(error.description));
    }
    
    // Inter type sets only for true Inter impression/
    // After impression done need reset type to AppReturn
    if (self.adType == kCASUType_INTER) {
        self.adType = kCASUType_APP_RETURN;
    }
}

// MARK: CASScreenContentDelegate
- (void)screenAdDidLoadContent:(id<CASScreenContent>)ad {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_LOADED,
                            self.adType == kCASUType_APP_RETURN ? kCASUType_INTER : self.adType,
                            0, NULL);
    }
}

- (void)screenAd:(id<CASScreenContent>)ad didFailToLoadWithError:(CASError *)error {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_FAILED,
                            self.adType == kCASUType_APP_RETURN ? kCASUType_INTER : self.adType,
                            (int)error.code, CASUStringToUnity(error.description));
    }
}

- (void)screenAdWillPresentContent:(id<CASScreenContent>)ad {
    [CASUPluginUtil onAdsWillPressent];

    if (self.impressionCallback) {
        self.impression = ad.contentInfo;
        self.impressionCallback(self.client, kCASUAction_SHOWN, self.adType, (__bridge CASImpressionRef)self.impression);
    }
}

- (void)screenAdDidClickContent:(id<CASScreenContent>)ad {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_CLICKED, self.adType, 0, NULL);
    }
}

- (void)screenAd:(id<CASScreenContent>)ad didFailToPresentWithError:(CASError *)error {
    extern bool _didResignActive;

    if (_didResignActive) {
        return;
    }

    [CASUPluginUtil onAdsDidClosed];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_SHOW_FAILED, self.adType, (int)error.code, CASUStringToUnity(error.description));
    }

    // Inter type sets only for true Inter impression/
    // After impression done need reset type to AppReturn
    if (self.adType == kCASUType_INTER) {
        self.adType = kCASUType_APP_RETURN;
    }
}

- (void)screenAdDidDismissContent:(id<CASScreenContent>)ad {
    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

    [CASUPluginUtil onAdsDidClosed];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_CLOSED, self.adType, 0, NULL);
    }

    // Inter type sets only for true Inter impression/
    // After impression done need reset type to AppReturn
    if (self.adType == kCASUType_INTER) {
        self.adType = kCASUType_APP_RETURN;
    }
}

// MARK: CASImpressionDelegate
- (void)adDidRecordImpressionWithInfo:(CASContentInfo *)info {
    extern bool _didResignActive;

    if (_didResignActive) {
        return;
    }

    if (self.impressionCallback) {
        self.impression = info;
        self.impressionCallback(self.client, kCASUAction_IMPRESSION, self.adType, (__bridge CASImpressionRef)info);
    }
}

@end
