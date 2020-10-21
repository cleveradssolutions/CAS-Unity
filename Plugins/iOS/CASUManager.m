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
    NSInteger bannerPositionId;
}

- (id)initWithAppID:(NSString *)appID
             enable:(NSUInteger)types
             demoAd:(BOOL)demoAd
          forClient:(CASUTypeManagerClientRef *)client
             onInit:(CASUInitializationCompleteCallback)onInit {
    self = [super init];
    if (self) {
        self.client = client;
        self.mediationManager =
            [CAS createWithManagerID:appID
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

        self.bannerCallback = [[CASUCallback alloc] initForFullScreen:NO];
        self.bannerCallback.client = client;
        self.interstitialCallback = [[CASUCallback alloc] initForFullScreen:YES];
        self.interstitialCallback.client = client;
        self.rewardedCallback = [[CASUCallback alloc] initForFullScreen:YES];
        self.rewardedCallback.client = client;
        self.bannerActiveSize = [self.mediationManager getBannerSize];

        [CASAnalytics setHandler:self.bannerCallback];
    }
    return self;
}

- (void)load:(CASType)type {
    switch (type) {
        case CASTypeBanner:
            if (!self.bannerView) {
                [self createBannerView];
                [self.bannerView setHidden:YES];
            }
            [self.bannerView loadNextAd];
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
            if (self.bannerView) {
                [self.bannerView setHidden:NO];
            } else {
                [self createBannerView];
            }
            [self setBannerPosition:bannerPositionId];
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

- (void)createBannerView {
    /// Align the bannerView in the Unity view bounds.
    UIViewController *unityController = [self unityGLViewController];
    UIView *unityView = unityController.view;
    self.bannerView = [[CASBannerView alloc] initWithManager:_mediationManager];
    self.bannerView.translatesAutoresizingMaskIntoConstraints = NO;
    self.bannerView.rootViewController = unityController;
    [unityView addSubview:self.bannerView];
}

- (void)hideBanner {
    if (self.bannerView) {
        [self.bannerView setHidden:YES];
    }
}

- (void)setBannerSize:(NSInteger)sizeId {
    CASSize *targetSize;
    switch (sizeId) {
        case 1:
            targetSize = CASSize.banner;
            break;
        case 2:
            targetSize = [CASSize getAdaptiveBannerInContainer:[self unityGLViewController].view];
            break;
        case 3:
            targetSize = [CASSize getSmartBanner];
            break;
        case 4:
            targetSize = CASSize.leaderboard;
            break;
        case 5:
            targetSize = CASSize.mediumRectangle;
            break;
        default:
            NSLog(@"[CAS] Framework bridge call change banner size with unknown id: %zd", sizeId);
            return;
    }
    if (self.bannerActiveSize.height != targetSize.height || self.bannerActiveSize.width != targetSize.width) {
        self.bannerActiveSize = targetSize;
        [_mediationManager setBannerSize:targetSize];
    }
}

- (CGFloat)bannerHeightInPixels {
    return CGRectGetHeight(CGRectStandardize(CGRectMake(0, 0, self.bannerActiveSize.width, self.bannerActiveSize.height))) * [UIScreen mainScreen].nativeScale;
}

- (CGFloat)bannerWidthInPixels {
    return CGRectGetWidth(CGRectStandardize(CGRectMake(0, 0, self.bannerActiveSize.width, self.bannerActiveSize.height))) * [UIScreen mainScreen].nativeScale;
}

- (void)setBannerPosition:(NSInteger)positionId {
    bannerPositionId = positionId;
    if (!self.bannerView) {
        return;
    }
    UIView *unityView = self.bannerView.superview;
    if (unityView) {
        for (NSLayoutConstraint *constraint in unityView.constraints) {
            if ([ constraint.identifier isEqualToString:@"casUHorizontalPos"]
                || [ constraint.identifier isEqualToString:@"casUVerticalPos"]) {
                [unityView removeConstraint:constraint];
            }
        }

        switch (positionId) {
            case 0:
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
            case 1:
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
            case 2:
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
            case 4:
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
            case 5:
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

- (void)setLastPageAdFor:(NSString *)content {
    self.mediationManager.lastPageAdContent = [CASLastPageAdContent createFrom:content];
}

- (void)onAdLoaded:(enum CASType)adType {
    if (self.didAdLoadedCallback) {
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
