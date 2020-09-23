//
//  CASUPluginUtil.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface CASUPluginUtil : NSObject
+ (nonnull instancetype)sharedInstance;

- (void)saveObject:(nullable id)obj withKey:(nonnull NSString *)key;
- (void)removeObjectWithKey:(nonnull NSString *)key;

@property(class) BOOL pauseOnBackground;
@end

NS_ASSUME_NONNULL_END
