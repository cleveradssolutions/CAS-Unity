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
#import "CASUView.h"

@interface CASUManager : NSObject

- (nonnull instancetype)initWithManager:(CASMediationManager *_Nonnull)manager
                                 client:(_Nonnull CASManagerClientRef *_Nonnull)client;

@property (nonatomic, strong, nonnull) CASMediationManager *casManager;

@property (nonatomic, strong, nullable) CASAppOpen *appOpenAd;
@property (nonatomic, strong, nullable) CASInterstitial *interstitialAd;
@property (nonatomic, strong, nullable) CASRewarded *rewardedAd;

@property (nonatomic, strong, nonnull) CASUCallback *interCallback;
@property (nonatomic, strong, nonnull) CASUCallback *rewardCallback;
@property (nonatomic, strong, nonnull) CASUCallback *appOpenCallback;

- (void)enableAd:(int)type;
- (void)loadAd:(int)type;
- (BOOL)isAdReady:(int)type;
- (void)showAd:(int)type;
- (void)destroyAd:(int)type;
- (CASUView *_Nonnull)createViewWithSize:(int)adSize
                                  client:(CASViewClientRef _Nullable *_Nullable)adViewClient;

- (void)setLastPageAdFor:(NSString *_Nullable)content;
- (void)setAutoShowAdOnAppReturn:(BOOL)enabled;
- (void)skipNextAppReturnAd;
@end

#endif /* ifndef CASUManager_h */
