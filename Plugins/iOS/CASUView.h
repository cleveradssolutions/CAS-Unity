//
//  CASUView.h
//  CASUnityPlugin
//
//  Copyright © 2021 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUCallback.h"
#import "CASUTypes.h"

@interface CASUView : NSObject

- (id _Nonnull)initWithManager:(CASMediationManager *_Nonnull)manager
                     forClient:(CASUTypeViewClientRef _Nullable *_Nullable)adViewClient
                          size:(int)size;

@property (nonatomic, strong) CASBannerView *_Nullable bannerView;
@property (nonatomic, assign) CASUTypeViewClientRef _Nullable *_Nullable client;
@property (nonatomic, assign, nullable) CASUDidLoadedAdCallback adLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidFailedAdCallback adFailedCallback;
@property (nonatomic, assign, nullable) CASUWillOpeningWithMetaCallback adPresentedCallback;
@property (nonatomic, assign, nullable) CASUDidClickedAdCallback adClickedCallback;

@property (nonatomic, readonly) int xOffsetInPixels;
@property (nonatomic, readonly) int yOffsetInPixels;
@property (nonatomic, readonly) int heightInPixels;
@property (nonatomic, readonly) int widthInPixels;

- (void)present;
- (void)hide;
- (void)load;
- (BOOL)isReady;
- (void)attach;
- (void)destroy;
- (void)setPositionCode:(int)code withX:(int)x withY:(int)y;
- (void)setRefreshInterval:(int)interval;
@end
