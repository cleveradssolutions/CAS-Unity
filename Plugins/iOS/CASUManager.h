//
//  CASUManager.h
//  CASUnityPlugin
//
//  Copyright © 2024 CAS.AI. All rights reserved.
//

#import <CoreGraphics/CoreGraphics.h>
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CASUCallback.h"
@import CleverAdsSolutions;

@interface CASUManager : NSObject<CASLoadDelegate>

- (nonnull instancetype)initWithManager:(CASMediationManager *_Nonnull)manager
                              forClient:(CASManagerClientRef _Nullable *_Nullable)client;

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
