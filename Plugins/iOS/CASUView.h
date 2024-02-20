//
//  CASUView.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CASUCallback.h"
#import "CASUTypes.h"
@import CleverAdsSolutions;

@interface CASUView : NSObject

- (nonnull instancetype)initWithManager:(CASMediationManager *_Nonnull)manager
                              forClient:(CASViewClientRef _Nullable *_Nullable)adViewClient
                                   size:(int)size;

@property (nonatomic, strong) CASBannerView *_Nullable bannerView;
@property (nonatomic, assign) CASViewClientRef _Nullable *_Nullable client;
@property (nonatomic, assign, nullable) CASUViewDidLoadCallback adLoadedCallback;
@property (nonatomic, assign, nullable) CASUViewDidFailedCallback adFailedCallback;
@property (nonatomic, assign, nullable) CASUViewWillPresentCallback adPresentedCallback;
@property (nonatomic, assign, nullable) CASUViewDidClickedCallback adClickedCallback;
@property (nonatomic, assign, nullable) CASUViewDidRectCallback adRectCallback;

- (void)present;
- (void)hide;
- (void)load;
- (BOOL)isReady;
- (void)attach;
- (void)destroy;
- (void)setPositionCode:(int)code withX:(int)x withY:(int)y;
- (void)setRefreshInterval:(int)interval;
- (int)getRefreshInterval;
@end
