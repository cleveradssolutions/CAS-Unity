//
//  CASUView.m
//  CASUnityPlugin
//
//  Copyright © 2021 Clever Ads Solutions. All rights reserved.
//

    #import "CASUView.h"
    #import "CASUPluginUtil.h"
    #import <UIKit/UIKit.h>

@interface CASUView () <CASBannerDelegate>
@property (nonatomic, assign) CGPoint adPositionOffset;
@property (nonatomic, assign) int activePos;
@end

@implementation CASUView
- (id)initWithManager:(CASMediationManager *)manager
            forClient:(CASUTypeViewClientRef *)adViewClient
                 size:(int)size {
    self = [super init];
    if (self) {
        UIViewController *unityVC = [CASUPluginUtil unityGLViewController];
        _client = adViewClient;
        _bannerView = [[CASBannerView alloc] initWithAdSize:[self getBannerSizeFromId:size withViewController:unityVC]
                                                    manager:manager];
        _bannerView.hidden = YES;
        _bannerView.adDelegate = self;
        _bannerView.rootViewController = unityVC;
    }
    return self;
}

- (void)dealloc {
    _bannerView.adDelegate = nil;
}

- (CASSize *)getBannerSizeFromId:(int)sizeId withViewController:(UIViewController *)controller {
    switch (sizeId) {
        case 2: return [CASSize getAdaptiveBannerInContainer:controller.view];
        case 3: return [CASSize getSmartBanner];
        case 4:  return CASSize.leaderboard;
        case 5: return CASSize.mediumRectangle;
        default: return CASSize.banner;
    }
}

- (void)present {
    if (self.bannerView) {
        self.bannerView.hidden = NO;
        [self refreshPosition];
    }
}

- (void)hide {
    if (self.bannerView) {
        self.bannerView.hidden = YES;
    }
}

- (void)attach {
    if (self.bannerView) {
        UIViewController *unityController = [CASUPluginUtil unityGLViewController];
        UIView *unityView = unityController.view;
        [unityView addSubview:self.bannerView];

        UIInterfaceOrientationMask orientation = [unityController supportedInterfaceOrientations];
        NSLog(@"Orientation: %ld", (long)orientation);
        if ((orientation & UIInterfaceOrientationMaskPortrait) != 0
            && (orientation & UIInterfaceOrientationMaskLandscape) != 0) {
            [[NSNotificationCenter defaultCenter] addObserver:self
                                                     selector:@selector(orientationChangedNotification:)
                                                         name:UIDeviceOrientationDidChangeNotification
                                                       object:nil];
        }
    }
}

- (void)orientationChangedNotification:(NSNotification *)notification {
    // Ignore changes in device orientation if unknown, face up, or face down.
    if (UIDeviceOrientationIsValidInterfaceOrientation([[UIDevice currentDevice] orientation])) {
        [self refreshPosition];
    }
}

- (void)destroy {
    if (self.bannerView) {
        [self.bannerView removeFromSuperview];
        [self.bannerView destroy];

        [[NSNotificationCenter defaultCenter] removeObserver:self];
    }
}

- (void)load {
    if (self.bannerView) {
        [self.bannerView loadNextAd];
    }
}

- (BOOL)isReady {
    return self.bannerView && self.bannerView.isAdReady;
}

- (void)setRefreshInterval:(int)interval {
    if (self.bannerView) {
        self.bannerView.refreshInterval = interval;
    }
}

- (int)xOffsetInPixels {
    return CGRectGetMinX(self.bannerView.bounds) * [UIScreen mainScreen].scale;
}

- (int)yOffsetInPixels {
    return CGRectGetMinY(self.bannerView.bounds) * [UIScreen mainScreen].scale;
}

- (int)heightInPixels {
    return CGRectGetHeight(CGRectStandardize(self.bannerView.frame)) * [UIScreen mainScreen].scale;
}

- (int)widthInPixels {
    return CGRectGetWidth(CGRectStandardize(self.bannerView.frame)) * [UIScreen mainScreen].scale;
}

- (void)setPositionCode:(int)code withX:(int)x withY:(int)y {
    if (code < 0 || code > 5) {
        self.activePos = 3;
    } else {
        self.activePos = code;
    }
    self.adPositionOffset = CGPointMake(x, y);
    [self refreshPosition];
}

- (void)refreshPosition {
    if (self.bannerView && !self.bannerView.isHidden) {
        /// Align the bannerView in the Unity view bounds.
        UIView *unityView = [CASUPluginUtil unityGLViewController].view;
        if (unityView) {
            [self positionView:self.bannerView inParentView:unityView];
        }
    }
}

- (void)positionView:(UIView *)view
        inParentView:(UIView *)parentView {
    CGRect parentBounds = parentView.bounds;
    if (@available(iOS 11, *)) {
        CGRect safeAreaFrame = parentView.safeAreaLayoutGuide.layoutFrame;
        if (!CGSizeEqualToSize(CGSizeZero, safeAreaFrame.size)) {
            parentBounds = safeAreaFrame;
        }
    }

    CGFloat bottom = CGRectGetMaxY(parentBounds) - CGRectGetMidY(view.bounds);
    CGFloat right = CGRectGetMaxX(parentBounds) - CGRectGetMidX(view.bounds);

    // Clamp with Maximum Bottom Right position
    CGFloat top = MIN(CGRectGetMinY(parentBounds) + self.adPositionOffset.y + CGRectGetMidY(view.bounds), bottom);
    CGFloat left = MIN(CGRectGetMinX(parentBounds) + self.adPositionOffset.x + CGRectGetMidX(view.bounds), right);

    CGPoint center;
    switch (self.activePos) {
        case 0:
            center = CGPointMake(CGRectGetMidX(parentBounds), top);
            break;
        case 1:
            center = CGPointMake(left, top);
            break;
        case 2:
            center = CGPointMake(right, top);
            break;
        case 4:
            center = CGPointMake(left, bottom);
            break;
        case 5:
            center = CGPointMake(right, bottom);
            break;
        default:
            center = CGPointMake(CGRectGetMidX(parentBounds), bottom);
            break;
    }
    view.center = center;
}

    #pragma mark - CASBannerDelegate
- (void)bannerAdView:(CASBannerView *_Nonnull)adView didFailToLoadWith:(enum CASError)error {
    if (self.adFailedCallback) {
        self.adFailedCallback(self.client, error);
    }
}

- (void)bannerAdViewDidLoad:(CASBannerView *_Nonnull)view {
    if (self.adLoadedCallback) {
        self.adLoadedCallback(self.client);
    }
}

- (void)bannerAdView:(CASBannerView *)adView willPresent:(id<CASStatusHandler>)impression {
    if (self.adPresentedCallback) {
        self.adPresentedCallback(self.client,
                                 [[CASNetwork values] indexOfObject:impression.network],
                                 impression.cpm,
                                 impression.priceAccuracy);
    }
}

- (void)bannerAdViewDidRecordClick:(CASBannerView *)adView {
    if (self.adClickedCallback) {
        self.adClickedCallback(self.client);
    }
}

@end
