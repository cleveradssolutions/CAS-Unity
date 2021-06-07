//
//  CASUCallback.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUTypes.h"

@interface CASUCallback : NSObject<CASAppReturnDelegate, CASAnalyticsHandler>

- (id _Nonnull )initForFullScreen:(BOOL)isFullScreen;

@property (nonatomic, assign) CASUTypeManagerClientRef _Nullable * _Nullable client;
@property (nonatomic, assign, nullable) CASUWillShownWithAdCallback willShownCallback;
@property (nonatomic, assign, nullable) CASUDidShowAdFailedWithErrorCallback didShowFailedCallback;
@property (nonatomic, assign, nullable) CASUDidCompletedAdCallback didCompleteCallback;
@property (nonatomic, assign, nullable) CASUDidClickedAdCallback didClickCallback;
@property (nonatomic, assign, nullable) CASUDidClosedAdCallback didClosedCallback;

@end
