//
//  CASUPluginUtil.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <AVFoundation/AVFoundation.h>
#import "CASUPluginUtil.h"
#if __has_include("UnityInterface.h")
#import "UnityInterface.h"
#endif

@interface CASUPluginUtil ()
@property (nonatomic, strong) NSMutableDictionary *internalReferences;

@end

@implementation CASUPluginUtil{
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
    }

    return self;
}

- (void)saveObject:(id)obj withKey:(NSString *)key {
    self->_internalReferences[key] = obj;
}

- (void)removeObjectWithKey:(NSString *)key {
    [self->_internalReferences removeObjectForKey:key];
}

static BOOL _pauseOnBackground = YES;

+ (BOOL)pauseOnBackground {
    return _pauseOnBackground;
}

+ (void)setPauseOnBackground:(BOOL)pause {
    _pauseOnBackground = pause;
}

+ (UIViewController *)unityGLViewController {
#if __has_include("UnityInterface.h")
    UIViewController *controller = UnityGetGLViewController() ? : UnityGetMainWindow().rootViewController;

    if (controller) {
        return controller;
    }

#endif
    id<UIApplicationDelegate> appDelegate = [UIApplication sharedApplication].delegate;

    UIWindow *window;

    if ([appDelegate respondsToSelector:@selector(window)]) {
        window = appDelegate.window;
    }

    if (!window) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
        window = [UIApplication sharedApplication].keyWindow;
#pragma clang diagnostic pop
    }

    if (window) {
        return window.rootViewController;
    }

    return nil;
}

+ (void)onAdsWillPressent {
#if __has_include("UnityInterface.h")

    if ([CASUPluginUtil pauseOnBackground]) {
        UnityPause(YES);
    }

#endif
}

+ (void)onAdsDidClosed {
#if __has_include("UnityInterface.h")

    if (UnityIsPaused()) {
        UnityPause(NO);
        // need to do this with delay because FMOD restarts audio in AVAudioSessionInterruptionNotification handler
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, 100 * NSEC_PER_MSEC), dispatch_get_main_queue(), ^{
            UnityUpdateMuteState([[AVAudioSession sharedInstance] outputVolume] < 0.01f ? 1 : 0);
        });
    }

#endif
}

@end
