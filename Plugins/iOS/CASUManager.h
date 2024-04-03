//
//  CASUManager.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUManager_h
#define CASUManager_h

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <CoreGraphics/CoreGraphics.h>
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CASUCallback.h"
#import "CASUTypes.h"

@interface CASUManager : NSObject<CASLoadDelegate>

- (nonnull instancetype)initWithManager:(CASMediationManager *_Nonnull)manager
                                 client:(_Nonnull CASManagerClientRef *_Nonnull)client;

@property (nonatomic, strong, nonnull) CASMediationManager *casManager;
@property (nonatomic, strong, nonnull) CASAppOpen *appOpenAd;
@property (nonatomic, strong, nonnull) CASUCallback *interCallback;
@property (nonatomic, strong, nonnull) CASUCallback *rewardCallback;
@property (nonatomic, strong, nonnull) CASUCallback *appReturnDelegate;
@property (nonatomic, strong, nonnull) CASUCallback *appOpenCallback;

@property (nonatomic, assign) _Nonnull CASManagerClientRef *_Nonnull client;
@property (nonatomic, assign, nullable) CASUDidLoadedAdCallback didLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidFailedAdCallback didFailedCallback;
@property (nonatomic, assign, nullable) CASUWillPresentAdCallback willOpeningCallback;
@property (nonatomic, assign, nullable) CASUWillPresentAdCallback didImpressionCallback;
@property (nonatomic, assign, nullable) CASUDidShowAdFailedWithErrorCallback didShowFailedCallback;
@property (nonatomic, assign, nullable) CASUDidCompletedAdCallback didCompleteCallback;
@property (nonatomic, assign, nullable) CASUDidClickedAdCallback didClickCallback;
@property (nonatomic, assign, nullable) CASUDidClosedAdCallback didClosedCallback;

- (void)loadAd:(int)type;
- (BOOL)isAdReady:(int)type;
- (void)showAd:(int)type;

- (void)setLastPageAdFor:(NSString *_Nullable)content;
- (void)setAutoShowAdOnAppReturn:(BOOL)enabled;
- (void)skipNextAppReturnAd;
@end

#endif /* ifndef CASUManager_h */
