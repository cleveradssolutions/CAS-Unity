//
//  CASUATTManager.m
//  CASUnityPlugin
//
//  Copyright © 2021 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <dlfcn.h>
#import <objc/runtime.h>
#import <sys/utsname.h>
#import "CASUATTManager.h"

@implementation CASUATTManager

+ (BOOL)isAvailable {
    if (@available(iOS 14, *)) {
        return [[NSBundle mainBundle] objectForInfoDictionaryKey:@"NSUserTrackingUsageDescription"] != nil;
    }
    return NO;
}

+ (void)trackingAuthorizationRequest:(CASUATTCompletion)completion {
    if ([CASUATTManager isAvailable]) {
        [CASUATTManager loadFramework];
        Class managerClass = NSClassFromString(@"ATTrackingManager");
        if (managerClass) {
            id handler = ^(NSUInteger result) {
                if (completion != nil) {
                    dispatch_async(dispatch_get_main_queue(), ^{
                        completion(result);
                    });
                }
            };

            SEL requestSelector = NSSelectorFromString(@"requestTrackingAuthorizationWithCompletionHandler:");
            if ([managerClass respondsToSelector:requestSelector]) {
                [managerClass performSelector:requestSelector withObject:handler];
            }
            return;
        }
        NSLog(@"AppTrackingTransparency class not loaded. Return 0 status");
    }
    if (completion != nil) {
        completion(0);
    }
}

+ (NSUInteger)getTrackingAuthorizationStatus {
    if (self.isAvailable) {
        [CASUATTManager loadFramework];
        Class managerClass = NSClassFromString(@"ATTrackingManager");
        if (managerClass) {
            NSUInteger value = [[managerClass valueForKey:@"trackingAuthorizationStatus"] unsignedIntegerValue];
            return value;
        }else{
            NSLog(@"AppTrackingTransparency class not loaded. Return 0 status");
        }
    }
    return 0;
}

+ (BOOL)isFrameworkPresent {
    id attClass = objc_getClass("ATTrackingManager");
    if (attClass) return TRUE;
    return FALSE;
}

+ (BOOL)isDeviceSimulator {
    struct utsname systemInfo;
    uname(&systemInfo);

    NSString *model = [NSString stringWithCString:systemInfo.machine encoding:NSUTF8StringEncoding];
    if ([model isEqualToString:@"x86_64"] || [model isEqualToString:@"i386"]) return TRUE;

    return FALSE;
}

+ (void)loadFramework {
    if (![CASUATTManager isFrameworkPresent]) {
        NSLog(@"AppTrackingTransparency Framework is not present, trying to load it.");
        NSString *frameworkLocation;
        if ([CASUATTManager isDeviceSimulator]) {
            NSString *frameworkPath = [[NSProcessInfo processInfo] environment]
                [@"DYLD_FALLBACK_FRAMEWORK_PATH"];
            if (frameworkPath) {
                frameworkLocation = [NSString pathWithComponents:@[ frameworkPath, @"AppTrackingTransparency.framework", @"AppTrackingTransparency" ]];
            }
        } else {
            frameworkLocation = [NSString stringWithFormat:@"/System/Library/Frameworks/AppTrackingTransparency.framework/AppTrackingTransparency"];
        }
        dlopen([frameworkLocation cStringUsingEncoding:NSUTF8StringEncoding], RTLD_LAZY);

        if (![CASUATTManager isFrameworkPresent]) {
            NSLog(@"AppTrackingTransparency still not present!");
            return;
        } else {
            NSLog(@"Successfully loaded AppTrackingTransparency framework");
            return;
        }
    } else {
        NSLog(@"AppTrackingTransparency framework already present");
        return;
    }
}

@end
