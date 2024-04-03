//
//  CASUView.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import "CASUPluginUtil.h"
#import "CASUView.h"


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
        _activePos = kCASUPosition_BOTTOM_CENTER;
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
        case kCASUSize_BANNER: return CASSize.banner;

        case kCASUSize_ADAPTIVE: {
            CGSize screenSize = [self getSafeBoundsView:controller.view].size;
            CGFloat width = MIN(screenSize.width, CASSize.leaderboard.width);
            return [CASSize getAdaptiveBannerForMaxWidth:width];
        }

        case kCASUSize_SMART: return [CASSize getSmartBanner];

        case kCASUSize_LEADER: return CASSize.leaderboard;

        case kCASUSize_MREC: return CASSize.mediumRectangle;

        case kCASUSize_FULL_WIDTH:{
            CGSize screenSize = [self getSafeBoundsView:controller.view].size;
            return [CASSize getAdaptiveBannerForMaxWidth:screenSize.width];
        }

        case kCASUSize_LINE:{
            CGSize screenSize = [self getSafeBoundsView:controller.view].size;
            BOOL inLandscape = screenSize.height < screenSize.width;
            CGFloat bannerHeight;

            if (screenSize.height > 720 && screenSize.width >= 728) {
                bannerHeight = inLandscape ? 50 : 90;
            } else {
                bannerHeight = inLandscape ? 32 : 50;
            }

            return [CASSize getInlineBannerWithWidth:screenSize.width maxHeight:bannerHeight];
        }

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
        if (_activeSizeId == kCASUSize_ADAPTIVE || _activeSizeId == kCASUSize_FULL_WIDTH || _activeSizeId == kCASUSize_LINE) {
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
        return (int)self.bannerView.refreshInterval;
    }

    return 30;
}

- (void)setPositionCode:(int)code withX:(int)x withY:(int)y {
    if (code < kCASUPosition_TOP_CENTER || code > kCASUPosition_BOTTOM_RIGHT) {
        _activePos = kCASUPosition_BOTTOM_CENTER;
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

- (CGRect)getSafeBoundsView:(UIView *)view {
    if (@available(iOS 11, *)) {
        CGRect safeAreaFrame = view.safeAreaLayoutGuide.layoutFrame;

        if (!CGSizeEqualToSize(CGSizeZero, safeAreaFrame.size)) {
            return safeAreaFrame;
        }
    }

    return view.bounds;
}

- (void)positionView:(UIView *)view
        inParentView:(UIView *)parentView {
    CGRect parentBounds = [self getSafeBoundsView:parentView];
    CGSize adSize = view.intrinsicContentSize;

    if (CGSizeEqualToSize(CGSizeZero, adSize)) {
        adSize = [self.bannerView.adSize toCGSize];
    }

    CGFloat verticalPos;
    CGFloat bottom = CGRectGetMaxY(parentBounds) - adSize.height;
    switch (_activePos) {
        case kCASUPosition_TOP_CENTER:
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_TOP_RIGHT:
            verticalPos = MIN(CGRectGetMinY(parentBounds) + _verticalOffset, bottom);
            break;

        default:
            verticalPos = bottom;
            break;
    }

    CGFloat horizontalPos;
    CGFloat right = CGRectGetMaxX(parentBounds) - adSize.width;
    switch (_activePos) {
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_BOTTOM_LEFT:
            horizontalPos = MIN(CGRectGetMinX(parentBounds) + _horizontalOffset, right);
            break;

        case kCASUPosition_TOP_RIGHT:
        case kCASUPosition_BOTTOM_RIGHT:
            horizontalPos = right;
            break;

        default:
            horizontalPos = CGRectGetMidX(parentView.bounds) - adSize.width * 0.5;
            break;
    }

    view.frame = CGRectMake(horizontalPos, verticalPos, adSize.width, adSize.height);

    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

    if (_adRectCallback) {
        CGFloat scale = [UIScreen mainScreen].scale;
        _adRectCallback(self.client,
                        horizontalPos * scale,
                        verticalPos * scale,
                        adSize.width * scale,
                        adSize.height * scale);
    }
}

    #pragma mark - CASBannerDelegate
- (void)bannerAdView:(CASBannerView *_Nonnull)adView didFailToLoadWith:(enum CASError)error {
    if (self.adFailedCallback) {
        self.adFailedCallback(self.client, (int)error);
    }
}

- (void)bannerAdViewDidLoad:(CASBannerView *_Nonnull)view {
    [self refreshPosition];

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
