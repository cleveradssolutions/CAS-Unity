//
//  CASUCallback.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUCallback_h
#define CASUCallback_h

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <Foundation/Foundation.h>
#import "CASUTypes.h"

@class CASUManager;

@interface CASUCallback : NSObject<CASAppReturnDelegate, CASPaidCallback>

- (nonnull instancetype)initWithType:(int)type;

@property (nonatomic, weak) CASUManager *_Nullable manager;
@property (nonatomic, strong, nullable) NSObject<CASStatusHandler> *impression;

- (void)didAdLoaded;
- (void)didAdFailedToLoadWithError:(NSString *_Nonnull)error;
- (void)didAdFailedToLoadWithErrorCode:(int)error;

@end

#endif /* ifndef CASUCallback_h */
