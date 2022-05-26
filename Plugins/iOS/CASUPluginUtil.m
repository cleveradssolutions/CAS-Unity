//
//  CASUPluginUtil.m
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import "CASUPluginUtil.h"

#if __has_include("UnityAppController.h")
#import "UnityAppController.h"
#else
#ifdef __cplusplus
extern "C" {
#endif
UIViewController * UnityGetGLViewController(void)
{
    return nil;
}

UIWindow * UnityGetMainWindow(void)
{
    return nil;
}

int UnityIsPaused(void)
{
    return 0;
}

void UnityPause(int pause)
{
}

#ifdef __cplusplus
}
#endif
#endif

#if __has_include(<FirebaseAnalytics/FIRAnalytics.h>)
#import <FirebaseAnalytics/FIRAnalytics.h>
#endif

@interface CASUPluginUtil ()
/// References to objects Google Mobile ads objects created from Unity.
@property (nonatomic, strong) NSMutableDictionary *internalReferences;

@end

@implementation CASUPluginUtil
{
    dispatch_queue_t _lockQueue;
}

+ (instancetype)sharedInstance {
    static CASUPluginUtil *sharedInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (id)init {
    self = [super init];
    if (self) {
        _internalReferences = [[NSMutableDictionary alloc] init];
        _lockQueue = dispatch_queue_create("CASUObjectCache lock queue", DISPATCH_QUEUE_SERIAL);
    }
    return self;
}

- (void)saveObject:(id)obj withKey:(NSString *)key {
    dispatch_async(_lockQueue, ^{
        self->_internalReferences[key] = obj;
    });
}

- (void)removeObjectWithKey:(NSString *)key {
    dispatch_async(_lockQueue, ^{
        [self->_internalReferences removeObjectForKey:key];
    });
}

- (void)analyticsEvent:(NSString *)eventName map:(NSDictionary<NSString *, id> *)map {
#if __has_include(<FirebaseAnalytics/FIRAnalytics.h>)
    [FIRAnalytics logEventWithName:eventName parameters:map];
#else
    NSLog(@"[CAS] Framework bridge cant find Firebase Analytics");
#endif
}

static BOOL _pauseOnBackground = YES;

+ (BOOL)pauseOnBackground {
    return _pauseOnBackground;
}

+ (void)setPauseOnBackground:(BOOL)pause {
    _pauseOnBackground = pause;
}

+ (NSString *)stringFromUnity:(const char *)bytes {
    return bytes ? @(bytes) : nil;
}

+ (const char *)stringToUnity:(NSString *)str {
    if (!str) {
        return NULL;
    }
    const char *string = str.UTF8String;
    char *res = (char *)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

+ (const char *)adMetaDataToStringPointer:(id<CASStatusHandler>)ad {
    NSMutableString *result = [[NSMutableString alloc] initWithCapacity:64];
    [result appendString:@"cpm="];
    [result appendString:[@(ad.cpm) stringValue]];
    [result appendString:@";accuracy="];
    [result appendString:[@(ad.priceAccuracy) stringValue]];
    [result appendString:@";"];

    NSString *network = ad.network;
    if (![network isEqualToString:CASNetwork.lastPageAd]) {
        NSUInteger netIndex = [[CASNetwork values] indexOfObject:network];
        if (netIndex != NSNotFound) {
            [result appendString:@"network="];
            [result appendString:[@(netIndex) stringValue]];
            [result appendString:@";"];
        }
    }

    NSString *creativeId = ad.creativeIdentifier;
    if (creativeId.length != 0) {
        [result appendString:@"creative="];
        [result appendString:creativeId];
        [result appendString:@";"];
    }

    NSString *identifier = ad.identifier;
    if (creativeId.length != 0) {
        [result appendString:@"id="];
        [result appendString:identifier];
        [result appendString:@";"];
    }
    return [CASUPluginUtil stringToUnity:result];
}

+ (UIViewController *)unityGLViewController {
    return UnityGetGLViewController()
            ? : UnityGetMainWindow().rootViewController
            ? : [[UIApplication sharedApplication].keyWindow rootViewController];
}

+ (void)onAdsWillPressent {
    if ([CASUPluginUtil pauseOnBackground]) {
        UnityPause(YES);
    }
}

+ (void)onAdsDidClosed {
    if (UnityIsPaused()) {
        UnityPause(NO);
    }
}

@end
