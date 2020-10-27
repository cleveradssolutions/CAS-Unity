//
//  CASUManager.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreGraphics/CoreGraphics.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUCallback.h"
#import <UIKit/UIKit.h>

@interface CASUManager : NSObject<CASLoadDelegate>

- (id _Nonnull)initWithAppID:(NSString *_Nullable)appID
                      enable:(NSUInteger)types
                      demoAd:(BOOL)demoAd
                   forClient:(CASUTypeManagerClientRef _Nullable *_Nullable)client
                      onInit:(nullable CASUInitializationCompleteCallback)onInit;

@property (nonatomic, strong, nonnull) CASMediationManager *mediationManager;
@property (nonatomic, strong, nullable) CASBannerView *bannerView;
@property (nonatomic, strong, nonnull) CASSize *bannerActiveSize;

@property (nonatomic, strong, nullable) CASUCallback *bannerCallback;
@property (nonatomic, strong, nullable) CASUCallback *interstitialCallback;
@property (nonatomic, strong, nullable) CASUCallback *rewardedCallback;

@property (nonatomic, strong, nullable) NSLayoutConstraint *horizontalConstraint;
@property (nonatomic, strong, nullable) NSLayoutConstraint *verticalConstraint;

@property (nonatomic, assign) CASUTypeManagerClientRef _Nullable *_Nullable client;
@property (nonatomic, assign, nullable) CASUDidAdLoadedCallback didAdLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidAdFailedToLoadCallback didAdFailedToLoadCallback;

@property (nonatomic, readonly) CGFloat bannerHeightInPixels;
@property (nonatomic, readonly) CGFloat bannerWidthInPixels;

- (void)load:(CASType)type;
- (void)show:(CASType)type;
- (void)hideBanner;
- (void)setBannerSize:(NSInteger)sizeId;
- (void)setBannerPosition:(NSInteger)positionId;
- (void)setLastPageAdFor:(NSString *_Nullable)content;
@end
