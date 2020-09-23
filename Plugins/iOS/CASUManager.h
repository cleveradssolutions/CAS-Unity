//
//  CASUManager.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUCallback.h"

@interface CASUManager : NSObject<CASLoadDelegate>

- (id)initWithAppID:(NSString *)appID
             enable:(NSUInteger)types
             demoAd:(BOOL)demoAd
          forClient:(CASUTypeManagerClientRef *)client
             onInit:(nullable CASUInitializationCompleteCallback)onInit;

@property (nonatomic, strong, nonnull) CASMediationManager *mediationManager;
@property (nonatomic, strong, nonnull) CASBannerView *bannerView;

@property (nonatomic, strong, nullable) CASUCallback *bannerCallback;
@property (nonatomic, strong, nullable) CASUCallback *interstitialCallback;
@property (nonatomic, strong, nullable) CASUCallback *rewardedCallback;

@property (nonatomic, assign) CASUTypeManagerClientRef *client;
@property (nonatomic, assign, nullable) CASUDidAdLoadedCallback didAdLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidAdFailedToLoadCallback didAdFailedToLoadCallback;

- (void)load:(CASType)type;
- (void)show:(CASType)type;
- (void)hideBanner;
- (void)setBannerSize:(NSInteger)sizeId;
- (void)setBannerPosition:(NSInteger)positionId;
@end
