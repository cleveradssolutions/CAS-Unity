//
//  CASUPluginUtil.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUPluginUtils_h
#define CASUPluginUtils_h

#import <CleverAdsSolutions/CleverAdsSolutions-Swift.h>
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface CASUPluginUtil : NSObject
+ (nonnull instancetype)sharedInstance;

- (void)saveObject:(nullable id)obj withKey:(nonnull NSString *)key;
- (void)removeObjectWithKey:(nonnull NSString *)key;

+ (UIViewController *)unityGLViewController;
+ (void)onAdsWillPressent;
+ (void)onAdsDidClosed;

@property (class) BOOL pauseOnBackground;
@end

NS_ASSUME_NONNULL_END

#endif /* ifndef CASUPluginUtils_h */
