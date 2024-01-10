//
//  CASUCallback.h
//  CASUnityPlugin
//
//  Copyright © 2024 CAS.AI. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUTypes.h"
@import CleverAdsSolutions;

@interface CASUCallback : NSObject<CASAppReturnDelegate, CASPaidCallback>

- (nonnull instancetype)initWithComplete:(BOOL)complete;

@property (nonatomic, assign) CASManagerClientRef _Nullable *_Nullable client;
@property (nonatomic, assign, nullable) CASUDidLoadedAdCallback didLoadedCallback;
@property (nonatomic, assign, nullable) CASUDidFailedAdCallback didFailedCallback;
@property (nonatomic, assign, nullable) CASUWillPresentAdCallback willOpeningCallback;
@property (nonatomic, assign, nullable) CASUWillPresentAdCallback didImpressionCallback;
@property (nonatomic, assign, nullable) CASUDidShowAdFailedWithErrorCallback didShowFailedCallback;
@property (nonatomic, assign, nullable) CASUDidCompletedAdCallback didCompleteCallback;
@property (nonatomic, assign, nullable) CASUDidClickedAdCallback didClickCallback;
@property (nonatomic, assign, nullable) CASUDidClosedAdCallback didClosedCallback;

- (void)callInUITheradLoadedCallback;
- (void)callInUITheradFailedToLoadCallbackWithError:(NSString *_Nonnull)error;

@end
