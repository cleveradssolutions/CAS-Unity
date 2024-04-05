//
//  CASUView.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUView_h
#define CASUView_h

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CASUCallback.h"
#import "CASUTypes.h"

@interface CASUView : NSObject

- (nonnull instancetype)initWithManager:(CASMediationManager *_Nonnull)manager
                              forClient:(CASViewClientRef _Nullable *_Nullable)adViewClient
                                   size:(int)size;

@property (nonatomic, strong) CASBannerView *_Nullable bannerView;
@property (nonatomic, assign) CASViewClientRef _Nullable *_Nullable client;
@property (nonatomic, assign, nullable) CASUViewActionCallback actionCallback;
@property (nonatomic, assign, nullable) CASUViewImpressionCallback impressionCallback;
@property (nonatomic, assign, nullable) CASUViewRectCallback rectCallback;

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

#endif /* ifndef CASUView_h */
