//
//  CASUPluginUtil.h
//  CASUnityPlugin
//
//  Copyright © 2023 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import CleverAdsSolutions;

NS_ASSUME_NONNULL_BEGIN

@interface CASUPluginUtil : NSObject
+ (nonnull instancetype)sharedInstance;

- (void)saveObject:(nullable id)obj withKey:(nonnull NSString *)key;
- (void)removeObjectWithKey:(nonnull NSString *)key;

+ (const char *)adMetaDataToStringPointer:(id<CASStatusHandler>)ad;
+ (UIViewController *)unityGLViewController;
+ (void)onAdsWillPressent;
+ (void)onAdsDidClosed;

@property (class) BOOL pauseOnBackground;
@end

NS_ASSUME_NONNULL_END
