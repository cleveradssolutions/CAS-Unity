//
//  CASUPluginUtil.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import <AVFoundation/AVFoundation.h>
#import "CASUPluginUtil.h"

extern void UnityPause(int pause);
extern void UnityUpdateMuteState(int mute);
extern int UnityIsPaused(void);

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

+ (UIWindow *)unityWindow {
    id<UIApplicationDelegate> appDelegate = [UIApplication sharedApplication].delegate;
    return appDelegate.window;
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
    
    // Unity not support change active of [AVAudioSession sharedInstance]
    // After that audio session can be restored by
    // UnitySetAudioSessionActive(YES);
    // but any AudioSources that are already playing will be stoped.
    
    // need to do this with delay because FMOD restarts audio in AVAudioSessionInterruptionNotification handler
    [CASUPluginUtil.sharedInstance performSelector:@selector(updateUnityAudioOutput) withObject:nil afterDelay:0.1];
}

- (void)updateUnityAudioOutput {
    if ([[AVAudioSession sharedInstance] outputVolume] > 0.0f) {
        UnityUpdateMuteState(0);
    }
}

@end
