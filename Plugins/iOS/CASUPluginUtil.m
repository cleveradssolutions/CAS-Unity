//
//  CASUPluginUtil.m
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import "CASUPluginUtil.h"
#import <CleverAdsSolutions/CleverAdsSolutions.h>

#if __has_include("UnityAppController.h")
#import "UnityAppController.h"
#else
#ifdef __cplusplus
extern "C" {
#endif
UIViewController * UnityGetGLViewController(void);
UIWindow * UnityGetMainWindow(void);
int UnityIsPaused();
void UnityPause(int pause);
#ifdef __cplusplus
}
#endif
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

static BOOL _pauseOnBackground = YES;

+ (BOOL)pauseOnBackground {
    return _pauseOnBackground;
}

+ (void)setPauseOnBackground:(BOOL)pause {
    _pauseOnBackground = pause;
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
