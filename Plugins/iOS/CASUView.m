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
        _client = adViewClient;
        _horizontalOffset = 0;
        _verticalOffset = 0;
        _activePos = kCASUPosition_BOTTOM_CENTER;
        _activeSizeId = size;

        if (size > 0) {
            _bannerView = [[CASBannerView alloc] initWithAdSize:[self getSizeByCode:size] 
                                                        manager:manager];
            _bannerView.hidden = YES;
            _bannerView.adDelegate = self;
            _bannerView.rootViewController = [CASUPluginUtil unityGLViewController];
        }
    }

    return self;
}

- (void)dealloc {
    if (self.bannerView) {
        self.bannerView.adDelegate = nil;
    }
}

- (CASSize *)getSizeByCode:(int)sizeId {
    switch (sizeId) {
        case kCASUSize_BANNER: return CASSize.banner;

        case kCASUSize_ADAPTIVE: {
            CGSize screenSize = [self getSafeAreaSize];
            CGFloat width = MIN(screenSize.width, CASSize.leaderboard.width);
            return [CASSize getAdaptiveBannerForMaxWidth:width];
        }

        case kCASUSize_SMART: return [CASSize getSmartBanner];

        case kCASUSize_LEADER: return CASSize.leaderboard;

        case kCASUSize_MREC: return CASSize.mediumRectangle;

        case kCASUSize_FULL_WIDTH:{
            CGSize screenSize = [self getSafeAreaSize];
            return [CASSize getAdaptiveBannerForMaxWidth:screenSize.width];
        }

        case kCASUSize_LINE:{
            CGSize screenSize = [self getSafeAreaSize];
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
            self.bannerView.adSize = [self getSizeByCode:_activeSizeId];
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

- (CGSize)getSafeAreaSize{
    UIWindow *window = [UIApplication sharedApplication].keyWindow;
    return window.safeAreaLayoutGuide.layoutFrame.size;
}

- (void)refreshPosition {
    if (!self.bannerView || self.bannerView.isHidden) {
        return;
    }

    UIWindow *window = [UIApplication sharedApplication].keyWindow;
    CGRect safeAreaFrame = window.safeAreaLayoutGuide.layoutFrame;
    CGSize adSize = self.bannerView.intrinsicContentSize;

    if (CGSizeEqualToSize(CGSizeZero, adSize)) {
        adSize = [self.bannerView.adSize toCGSize];
    }

    CGFloat verticalPos;
    CGFloat bottom = CGRectGetMaxY(safeAreaFrame) - adSize.height;
    switch (_activePos) {
        case kCASUPosition_TOP_CENTER:
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_TOP_RIGHT:
            verticalPos = MIN(CGRectGetMinY(safeAreaFrame) + _verticalOffset, bottom);
            break;

        default:
            verticalPos = bottom;
            break;
    }

    CGFloat horizontalPos;
    CGFloat right = CGRectGetMaxX(safeAreaFrame) - adSize.width;
    switch (_activePos) {
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_BOTTOM_LEFT:
            horizontalPos = MIN(CGRectGetMinX(safeAreaFrame) + _horizontalOffset, right);
            break;

        case kCASUPosition_TOP_RIGHT:
        case kCASUPosition_BOTTOM_RIGHT:
            horizontalPos = right;
            break;

        default:
            horizontalPos = CGRectGetMidX(window.bounds) - adSize.width * 0.5;
            break;
    }

    self.bannerView.frame = CGRectMake(horizontalPos, verticalPos, adSize.width, adSize.height);

    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

    if (self.rectCallback) {
        CGFloat scale = [UIScreen mainScreen].scale;
        self.rectCallback(self.client,
                          horizontalPos * scale,
                          verticalPos * scale,
                          adSize.width * scale,
                          adSize.height * scale);
    }
}

    #pragma mark - CASBannerDelegate
- (void)bannerAdView:(CASBannerView *_Nonnull)adView didFailToLoadWith:(enum CASError)error {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_FAILED, (int)error);
    }
}

- (void)bannerAdViewDidLoad:(CASBannerView *_Nonnull)view {
    [self refreshPosition];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_LOADED, 0);
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

    if (self.impressionCallback) {
        _lastImpression = (NSObject<CASStatusHandler> *)impression;
        self.impressionCallback(self.client, (__bridge CASImpressionRef)_lastImpression);
    }
}

- (void)bannerAdViewDidRecordClick:(CASBannerView *)adView {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_CLICKED, 0);
    }
}

@end
