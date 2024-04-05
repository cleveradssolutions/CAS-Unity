//
//  CASUPluginUtil.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <AVFoundation/AVFoundation.h>
#import "CASUPluginUtil.h"
#import "UnityInterface.h"

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
    return UnityGetGLViewController()
            ? : [UIApplication sharedApplication].delegate.window.rootViewController
            ? : UnityGetMainWindow().rootViewController;
}

+ (void)onAdsWillPressent {
    if ([CASUPluginUtil pauseOnBackground]) {
        UnityPause(YES);
    }
}

+ (void)onAdsDidClosed {
    if (UnityIsPaused()) {
        UnityPause(NO);
        // need to do this with delay because FMOD restarts audio in AVAudioSessionInterruptionNotification handler
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, 100 * NSEC_PER_MSEC), dispatch_get_main_queue(), ^{
            UnityUpdateMuteState([[AVAudioSession sharedInstance] outputVolume] < 0.01f ? 1 : 0);
        });
    }
}

@end
