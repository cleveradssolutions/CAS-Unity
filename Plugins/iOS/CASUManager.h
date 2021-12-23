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
             mediationExtras:(NSMutableDictionary<NSString *, NSString *> *_Nullable)extras
                      onInit:(nullable CASUInitializationCompleteCallback)onInit;

@property (nonatomic, assign) CASUTypeManagerClientRef _Nullable *_Nullable client;
@property (nonatomic, strong, nonnull) CASMediationManager *casManager;
@property (nonatomic, strong, nullable) CASUCallback *interCallback;
@property (nonatomic, strong, nullable) CASUCallback *rewardCallback;
@property (nonatomic, strong, nullable) CASUCallback *appReturnDelegate;

- (void)presentInter;
- (void)presentReward;

- (void)setLastPageAdFor:(NSString *_Nullable)content;
- (void)enableReturnAds;
- (void)disableReturnAds;
- (void)skipNextAppReturnAd;
@end
