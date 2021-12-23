//
//  CASUATTManager.h
//  CASUnityPlugin
//
//  Copyright © 2021 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUTypes.h"

@interface CASUATTManager : NSObject

+ (BOOL)isAvailable;
+ (void)trackingAuthorizationRequest:(CASUATTCompletion)completion;
+ (NSUInteger)getTrackingAuthorizationStatus;

@end
