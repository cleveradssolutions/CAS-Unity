//
//  CASUCallback.h
//  CASUnityPlugin
//
//  Copyright © 2025 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUCallback_h
#define CASUCallback_h

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <Foundation/Foundation.h>
#import "CASUTypes.h"

@class CASUManager;

@interface CASUCallback : NSObject<CASScreenContentDelegate, CASImpressionDelegate>

- (nonnull instancetype)initWithType:(int)type
                              client:(_Nonnull CASManagerClientRef *_Nonnull)client;

@property (nonatomic, assign) int adType;
@property (nonatomic, assign) _Nonnull CASManagerClientRef *_Nonnull client;
@property (nonatomic, assign, nullable) CASUActionCallback actionCallback;
@property (nonatomic, assign, nullable) CASUImpressionCallback impressionCallback;
@property (nonatomic, strong, nullable) CASContentInfo *impression;
- (void)didCompletedAd;
- (void)didAdNotReadyToPresent;

@end

#endif /* ifndef CASUCallback_h */
