//
//  CASUView.m
//  CASUnityPlugin
//
//  Copyright © 2022 Clever Ads Solutions. All rights reserved.
//

#import "CASUPluginUtil.h"
#import "CASUView.h"

static const int AD_POSITION_TOP_CENTER = 0;
static const int AD_POSITION_TOP_LEFT = 1;
static const int AD_POSITION_TOP_RIGHT = 2;
static const int AD_POSITION_BOTTOM_CENTER = 3;
static const int AD_POSITION_BOTTOM_LEFT = 4;
static const int AD_POSITION_BOTTOM_RIGHT = 5;

static const int AD_SIZE_BANNER = 1;
static const int AD_SIZE_ADAPTIVE = 2;
static const int AD_SIZE_SMART = 3;
static const int AD_SIZE_LEADER = 4;
static const int AD_SIZE_MREC = 5;
static const int AD_SIZE_FULL_WIDTH = 6;

@interface CASUView () <CASBannerDelegate>
@end

@implementation CASUView {
    NSObject<CASStatusHandler> *_lastImpression;
    /// Offset for the ad in the x-axis when a custom position is used. Value will be 0 for non-custom positions.
    int _horizontalOffset;
    /// Offset for the ad in the y-axis when a custom position is used. Value will be 0 for non-custom positions.
    int _verticalOffset;
    int _activePos;
    int _activeSizeId;
}

- (instancetype)initWithManager:(CASMediationManager *)manager
                      forClient:(CASViewClientRef *)adViewClient
                           size:(int)size {
    self = [super init];

    if (self) {
        UIViewController *unityVC = [CASUPluginUtil unityGLViewController];
        _client = adViewClient;
        _horizontalOffset = 0;
        _verticalOffset = 0;
        _activePos = AD_POSITION_BOTTOM_CENTER;
        _activeSizeId = size;

        if (size > 0) {
            _bannerView = [[CASBannerView alloc] initWithAdSize:[self getSizeByCode:size with:unityVC] manager:manager];
            _bannerView.hidden = YES;
            _bannerView.adDelegate = self;
            _bannerView.rootViewController = unityVC;
        }
    }

    return self;
}

- (void)dealloc {
    if (self.bannerView) {
        self.bannerView.adDelegate = nil;
    }
}

- (CASSize *)getSizeByCode:(int)sizeId with:(UIViewController *)controller {
    switch (sizeId) {
        case AD_SIZE_BANNER: return CASSize.banner;

        case AD_SIZE_ADAPTIVE: {
            CGRect screenRect = [controller.view bounds];
            CGFloat width = MIN(CGRectGetWidth(screenRect), CASSize.leaderboard.width);
            return [CASSize getAdaptiveBannerForMaxWidth:width];
        }

        case AD_SIZE_SMART: return [CASSize getSmartBanner];

        case AD_SIZE_LEADER: return CASSize.leaderboard;

        case AD_SIZE_MREC: return CASSize.mediumRectangle;

        case AD_SIZE_FULL_WIDTH: return [CASSize getAdaptiveBannerInContainer:controller.view];

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
    if (!self.bannerView) {
        return;
    }

    // Ignore changes in device orientation if unknown, face up, or face down.
    if (UIDeviceOrientationIsValidInterfaceOrientation([[UIDevice currentDevice] orientation])) {
        if (_activeSizeId == AD_SIZE_ADAPTIVE || _activeSizeId == AD_SIZE_FULL_WIDTH) {
            UIViewController *unityController = [CASUPluginUtil unityGLViewController];
            self.bannerView.adSize = [self getSizeByCode:_activeSizeId with:unityController];
        }

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

- (int)getRefreshInterval {
    if (self.bannerView) {
        return self.bannerView.refreshInterval;
    }

    return 30;
}

- (void)setPositionCode:(int)code withX:(int)x withY:(int)y {
    if (code < AD_POSITION_TOP_CENTER || code > AD_POSITION_BOTTOM_RIGHT) {
        _activePos = AD_POSITION_BOTTOM_CENTER;
    } else {
        _activePos = code;
    }

    _horizontalOffset = x;
    _verticalOffset = y;
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
    CGFloat top = MIN(CGRectGetMinY(parentBounds) + _verticalOffset + CGRectGetMidY(view.bounds), bottom);
    CGFloat left = MIN(CGRectGetMinX(parentBounds) + _horizontalOffset + CGRectGetMidX(view.bounds), right);

    CGPoint center;
    switch (_activePos) {
        case AD_POSITION_TOP_CENTER:
            center = CGPointMake(CGRectGetMidX(parentView.bounds), top);
            break;

        case AD_POSITION_TOP_LEFT:
            center = CGPointMake(left, top);
            break;

        case AD_POSITION_TOP_RIGHT:
            center = CGPointMake(right, top);
            break;

        case AD_POSITION_BOTTOM_LEFT:
            center = CGPointMake(left, bottom);
            break;

        case AD_POSITION_BOTTOM_RIGHT:
            center = CGPointMake(right, bottom);
            break;

        default:
            center = CGPointMake(CGRectGetMidX(parentView.bounds), bottom);
            break;
    }
    view.center = center;

    if (_adRectCallback) {
        CGFloat scale = [UIScreen mainScreen].scale;
        CGRect bounds = self.bannerView.bounds;
        CGRect frame = CGRectStandardize(self.bannerView.frame);
        _adRectCallback(self.client,
                        CGRectGetMinX(bounds) * scale,
                        CGRectGetMinY(bounds) * scale,
                        CGRectGetWidth(frame),
                        CGRectGetHeight(frame));
    }
}

    #pragma mark - CASBannerDelegate
- (void)bannerAdView:(CASBannerView *_Nonnull)adView didFailToLoadWith:(enum CASError)error {
    if (self.adFailedCallback) {
        self.adFailedCallback(self.client, (int)error);
    }
}

- (void)bannerAdViewDidLoad:(CASBannerView *_Nonnull)view {
    if (self.adLoadedCallback) {
        self.adLoadedCallback(self.client);
    }
}

- (void)bannerAdView:(CASBannerView *)adView willPresent:(id<CASStatusHandler>)impression {
    //Escape from callback when App on background.
    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

    if (self.adPresentedCallback) {
        _lastImpression = (NSObject<CASStatusHandler> *)impression;
        self.adPresentedCallback(self.client, (__bridge CASImpressionRef)_lastImpression);
    }
}

- (void)bannerAdViewDidRecordClick:(CASBannerView *)adView {
    if (self.adClickedCallback) {
        self.adClickedCallback(self.client);
    }
}

@end
