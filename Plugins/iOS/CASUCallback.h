//
//  CASUCallback.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import "CASUTypes.h"

@interface CASUCallback : NSObject<CASAppReturnDelegate>

- (id _Nonnull )initWithComplete:(BOOL)complete;

@property (nonatomic, assign) CASUTypeManagerClientRef _Nullable * _Nullable client;
@property (nonatomic, assign, nullable) CASUDidLoadedAdCallback didLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidFailedAdCallback didFailedCallback;
@property (nonatomic, assign, nullable) CASUWillOpeningWithMetaCallback willOpeningCallback;
@property (nonatomic, assign, nullable) CASUDidShowAdFailedWithErrorCallback didShowFailedCallback;
@property (nonatomic, assign, nullable) CASUDidCompletedAdCallback didCompleteCallback;
@property (nonatomic, assign, nullable) CASUDidClickedAdCallback didClickCallback;
@property (nonatomic, assign, nullable) CASUDidClosedAdCallback didClosedCallback;

@end
