//
//  CASUManager.m
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CASUManager.h"
#import <UIKit/UIKit.h>
#if __has_include("UnityAppController.h")
#import "UnityAppController.h"
#endif

@implementation CASUManager
{
    BOOL bannerAttached;
}

- (id)initWithAppID:(NSString *)appID
             enable:(NSUInteger)types
             demoAd:(BOOL)demoAd
          forClient:(CASUTypeManagerClientRef *)client
             onInit:(CASUInitializationCompleteCallback)onInit {
    self = [super init];
    if (self) {
        bannerAttached = NO;
        self.client = client;
        self.mediationManager = [CAS createWithManagerID:appID
                                             enableTypes:types
                                              demoAdMode:demoAd
                                                  onInit:^(BOOL succses, NSString *_Nullable error) {
            if (onInit) {
                if (error) {
                    onInit(client, succses, [error cStringUsingEncoding:NSUTF8StringEncoding]);
                } else {
                    onInit(client, succses, NULL);
                }
            }
        }];

        self.bannerView = [[CASBannerView alloc] initWithManager:_mediationManager];
        self.bannerView.translatesAutoresizingMaskIntoConstraints = NO;
        self.bannerView.rootViewController = [self unityGLViewController];
        self.bannerCallback = [[CASUCallback alloc] initForFullScreen:NO];
        self.bannerCallback.client = client;
        self.interstitialCallback = [[CASUCallback alloc] initForFullScreen:YES];
        self.interstitialCallback.client = client;
        self.rewardedCallback = [[CASUCallback alloc] initForFullScreen:YES];
        self.rewardedCallback.client = client;

        [CASAnalytics setHandler:self.bannerCallback];
    }
    return self;
}

- (void)load:(CASType)type {
    switch (type) {
        case CASTypeBanner:
            [_bannerView loadNextAd];
            break;
        case CASTypeInterstitial:
            [_mediationManager loadInterstitial];
            break;
        case CASTypeRewarded:
            [_mediationManager loadRewardedVideo];
            break;
        default:
            break;
    }
}

- (void)show:(CASType)type {
    switch (type) {
        case CASTypeBanner:
            if (bannerAttached) {
                _bannerView.hidden = NO;
            } else {
                [_bannerView removeFromSuperview];

                /// Align the bannerView in the Unity view bounds.
                UIView *unityView = [self unityGLViewController].view;

                [unityView addSubview:self.bannerView];
                _bannerView.hidden = NO;
                bannerAttached = YES;
            }
            if (self.bannerView.adPostion == CASPositionUndefined) {
                [self setBannerPosition:CASPositionBottomCenter];
            } else {
                [self setBannerPosition:self.bannerView.adPostion];
            }
            break;
        case CASTypeInterstitial:
            [_mediationManager showFromRootViewController:[self unityGLViewController]
                                                     type:type callback:_interstitialCallback];
            break;
        case CASTypeRewarded:
            [_mediationManager showFromRootViewController:[self unityGLViewController]
                                                     type:type callback:_rewardedCallback];
            break;
        default:
            break;
    }
}

- (void)hideBanner {
    if (bannerAttached) {
        _bannerView.hidden = YES;
    }
}

- (void)setBannerSize:(NSInteger)sizeId {
    switch (sizeId) {
        case 1:
            [_mediationManager setBannerWithSize:CASSize.banner];
            break;
        case 2:
            [_mediationManager setBannerWithSize:[CASSize getAdaptiveBannerInContainer:[self unityGLViewController].view]];
            break;
        case 3:
            [_mediationManager setBannerWithSize:[CASSize getSmartBanner]];
            break;
        case 4:
            [_mediationManager setBannerWithSize:CASSize.leaderboard];
            break;
        case 5:
            [_mediationManager setBannerWithSize:CASSize.mediumRectangle];
            break;
        default:
            NSLog(@"[CAS] Framework bridge call change banner size with unknown id: %zd", sizeId);
            break;
    }
}

- (void)setBannerPosition:(NSInteger)positionId {
    CASPosition newPos = (CASPosition)positionId;
    self.bannerView.adPostion = newPos;

    UIView *unityView = self.bannerView.superview;
    if (unityView) {
        for (NSLayoutConstraint *constraint in unityView.constraints) {
            if ([ constraint.identifier isEqualToString:@"casUHorizontalPos"]
                || [ constraint.identifier isEqualToString:@"casUVerticalPos"]) {
                [unityView removeConstraint:constraint];
            }
        }

        switch (newPos) {
            case CASPositionTopCenter:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.safeAreaLayoutGuide.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.centerXAnchor
                                                   to:unityView.safeAreaLayoutGuide.centerXAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.centerXAnchor
                                                   to:unityView.centerXAnchor];
                }
                break;
            case CASPositionTopLeft:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.safeAreaLayoutGuide.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.leftAnchor
                                                   to:unityView.safeAreaLayoutGuide.leftAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.leftAnchor
                                                   to:unityView.leftAnchor];
                }
                break;
            case CASPositionTopRight:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.safeAreaLayoutGuide.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.rightAnchor
                                                   to:unityView.safeAreaLayoutGuide.rightAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.topAnchor
                                                 to:unityView.topAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.rightAnchor
                                                   to:unityView.rightAnchor];
                }
                break;
            case CASPositionBottomLeft:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor to:unityView.safeAreaLayoutGuide.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.leftAnchor
                                                   to:unityView.safeAreaLayoutGuide.leftAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor
                                                 to:unityView.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.leftAnchor
                                                   to:unityView.leftAnchor];
                }
                break;
            case CASPositionBottomRight:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor
                                                 to:unityView.safeAreaLayoutGuide.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.rightAnchor
                                                   to:unityView.safeAreaLayoutGuide.rightAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor
                                                 to:unityView.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.rightAnchor
                                                   to:unityView.rightAnchor];
                }
                break;
            default:
                if (@available(iOS 11, *)) {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor
                                                 to:unityView.safeAreaLayoutGuide.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.centerXAnchor
                                                   to:unityView.safeAreaLayoutGuide.centerXAnchor];
                } else {
                    [self addVerticalConstraintsFor:_bannerView.bottomAnchor
                                                 to:unityView.bottomAnchor];
                    [self addHorizontalConstraintsFor:_bannerView.centerXAnchor
                                                   to:unityView.centerXAnchor];
                }
                break;
        }
        [unityView layoutIfNeeded ];
    }
}

- (void)addHorizontalConstraintsFor:(NSLayoutXAxisAnchor *)subview to:(NSLayoutXAxisAnchor *)container {
    NSLayoutConstraint *result = [subview constraintEqualToAnchor:container];
    result.identifier = @"casUHorizontalPos";
    result.active = YES;
}

- (void)addVerticalConstraintsFor:(NSLayoutYAxisAnchor *)subview to:(NSLayoutYAxisAnchor *)container {
    NSLayoutConstraint *result = [subview constraintEqualToAnchor:container];
    result.identifier = @"casUVerticalPos";
    result.active = YES;
}

- (void)onAdLoaded:(enum CASType)adType {
    if (self.didAdLoadedCallback && self.client) {
        if (self.client) {
            self.didAdLoadedCallback(self.client, (NSInteger)adType);
        }
    }
}

- (void)onAdFailedToLoad:(enum CASType) adType withError:(NSString *)error {
    if (self.didAdFailedToLoadCallback) {
        if (self.client) {
            self.didAdFailedToLoadCallback(self.client, (NSInteger)adType, [error cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }
}

- (UIViewController *)unityGLViewController {
#if __has_include("UnityAppController.h")
    return ((UnityAppController *)[UIApplication sharedApplication].delegate).rootViewController;
#else
    NSLog(@"[CAS] Framework bridge cant find UnityAppController.h");
    return nil;
#endif
}

@end
