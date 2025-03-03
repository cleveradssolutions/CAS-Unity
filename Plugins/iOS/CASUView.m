//
//  CASUView.m
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#import "CASUPluginUtil.h"
#import "CASUView.h"


@interface CASUView () <CASBannerDelegate>
@property (nonatomic, strong) NSLayoutConstraint *constraintX;
@property (nonatomic, strong) NSLayoutConstraint *constraintY;
@end

@implementation CASUView {
    NSObject<CASStatusHandler> *_lastImpression;
    int _horizontalOffset;
    int _verticalOffset;
    int _activePos;
    int _activeSizeId;
    BOOL _requiredRefreshSize;
    
    /// Bug in Apple's SKProductViewController which always tries to present in portrait mode.
    /// In landscape apps, it tries to force the orientation to be portrait, causing the safe areas insets to switch to portrait mode insets even though the SKProductViewController looks like it's presenting in landscape.
    /// As woraround we ignore Top safearea offset if Portrait Orientation is not Supported.
    BOOL _isPortraitOrientationSupported;

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
        _requiredRefreshSize = NO;
        _isPortraitOrientationSupported = NO;

        if (size > 0) {
            _bannerView = [[CASBannerView alloc] initWithAdSize:[self getSizeByCode:size]
                                                        manager:manager];
            _bannerView.hidden = YES;
            _bannerView.adDelegate = self;
        }
    }

    return self;
}

- (void)present {
    if (self.bannerView) {
        self.bannerView.hidden = NO;
    }
}

- (void)hide {
    if (self.bannerView) {
        self.bannerView.hidden = YES;
    }
}

- (void)attach {
    if (!self.bannerView) {
        return;
    }

    [self.bannerView addObserver:self forKeyPath:@"center" options:NSKeyValueObservingOptionNew context:nil];

    UIWindow *unityWindow = [CASUPluginUtil unityWindow];
    UIViewController *unityController = unityWindow.rootViewController;
    UIView *unityView = unityController.view;
    self.bannerView.rootViewController = unityController;
    self.bannerView.translatesAutoresizingMaskIntoConstraints = NO;
    [unityView addSubview:self.bannerView];

    UIInterfaceOrientationMask orientation = [unityController supportedInterfaceOrientations];

    _isPortraitOrientationSupported = (orientation & UIInterfaceOrientationMaskPortrait) != 0;

    if (_isPortraitOrientationSupported && (orientation & UIInterfaceOrientationMaskLandscape) != 0) {
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(orientationChangedNotification:)
                                                     name:UIDeviceOrientationDidChangeNotification
                                                   object:nil];
    }
    
    UILayoutGuide *safeArea = unityView.safeAreaLayoutGuide;

    [NSLayoutConstraint activateConstraints:@[
         [NSLayoutConstraint constraintWithItem:self.bannerView
                                      attribute:NSLayoutAttributeTop
                                      relatedBy:NSLayoutRelationGreaterThanOrEqual
                                         toItem:_isPortraitOrientationSupported ?
          safeArea : unityWindow
                                      attribute:NSLayoutAttributeTop
                                     multiplier:1.0
                                       constant:0.0],
         [NSLayoutConstraint constraintWithItem:self.bannerView
                                      attribute:NSLayoutAttributeBottom
                                      relatedBy:NSLayoutRelationLessThanOrEqual
                                         toItem:safeArea
                                      attribute:NSLayoutAttributeBottom
                                     multiplier:1.0
                                       constant:0.0],
         [NSLayoutConstraint constraintWithItem:self.bannerView
                                      attribute:NSLayoutAttributeLeft
                                      relatedBy:NSLayoutRelationGreaterThanOrEqual
                                         toItem:safeArea
                                      attribute:NSLayoutAttributeLeft
                                     multiplier:1.0
                                       constant:0.0],
         [NSLayoutConstraint constraintWithItem:self.bannerView
                                      attribute:NSLayoutAttributeRight
                                      relatedBy:NSLayoutRelationLessThanOrEqual
                                         toItem:safeArea
                                      attribute:NSLayoutAttributeRight
                                     multiplier:1.0
                                       constant:0.0]
    ]];

    [self refreshPositionInSafeArea:safeArea];
}

- (void)destroy {
    self.constraintX = nil;
    self.constraintY = nil;

    if (self.bannerView) {
        [[NSNotificationCenter defaultCenter] removeObserver:self name:UIDeviceOrientationDidChangeNotification object:nil];
        [self.bannerView removeObserver:self forKeyPath:@"center"];
        [self.bannerView removeFromSuperview];
        [self.bannerView destroy];
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
    _activePos = code;
    _horizontalOffset = x;
    _verticalOffset = y;

    if (self.bannerView) {
        UILayoutGuide *safeArea = self.bannerView.superview.safeAreaLayoutGuide;
        [self refreshPositionInSafeArea:safeArea];
    }
}

- (void)orientationChangedNotification:(NSNotification *)notification {
    if (!self.bannerView) {
        return;
    }

    // Ignore changes in device orientation if unknown, face up, or face down.
    if (UIDeviceOrientationIsValidInterfaceOrientation([[UIDevice currentDevice] orientation])) {
        if (_activeSizeId == kCASUSize_ADAPTIVE || _activeSizeId == kCASUSize_FULL_WIDTH || _activeSizeId == kCASUSize_LINE) {
            _requiredRefreshSize = YES;
        }
    }
}

- (void)observeValueForKeyPath:(NSString *)keyPath ofObject:(id)object change:(NSDictionary *)change context:(void *)context
{
    // Refresh View.frame with set View.center for position or View.bounds for size.
    if (object == self.bannerView) {
        [self refreshPixelsRect:self.bannerView.frame];
    }
}

- (void)refreshPixelsRect:(CGRect)rect {
    if (!self.bannerView) {
        return;
    }

#if __has_include("UnityInterface.h")
    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

#endif

    if (self.rectCallback) {
        CGFloat scale = [UIScreen mainScreen].scale;
        self.rectCallback(self.client,
                          CGRectGetMinX(rect) * scale,
                          CGRectGetMinY(rect) * scale,
                          CGRectGetWidth(rect) * scale,
                          CGRectGetHeight(rect) * scale);
    }

    if (_requiredRefreshSize) {
        _requiredRefreshSize = NO;
        self.bannerView.adSize = [self getSizeByCode:_activeSizeId];
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

    #pragma mark - CASBannerDelegate
- (void)bannerAdView:(CASBannerView *_Nonnull)adView didFailToLoadWith:(enum CASError)error {
    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_FAILED, (int)error);
    }
}

- (void)bannerAdViewDidLoad:(CASBannerView *_Nonnull)view {
    // Banner View.frame is currently invalid.
    // NSLayoutConstraint updates View.frame the next time the visible view is rendered.
    // [self refreshPixelsRect:self.bannerView.frame];
    [self refreshPixelsRect:[self calculateRect]];

    if (self.actionCallback) {
        self.actionCallback(self.client, kCASUAction_LOADED, 0);
    }
}

- (void)bannerAdView:(CASBannerView *)adView willPresent:(id<CASStatusHandler>)impression {
#if __has_include("UnityInterface.h")
    extern bool _didResignActive;

    if (_didResignActive) {
        // We are in the middle of the shutdown sequence, and at this point unity runtime is already destroyed.
        // We shall not call unity API, and definitely not script callbacks, so nothing to do here
        return;
    }

#endif

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

#pragma mark - Ad Rect

- (CGSize)getSafeAreaSize {
    CGRect screenBounds;
    CGRect safeFrame = [CASUPluginUtil unityWindow].safeAreaLayoutGuide.layoutFrame;

    if (CGSizeEqualToSize(safeFrame.size, CGSizeZero)) {
        screenBounds = [UIScreen mainScreen].bounds;
    } else {
        screenBounds = safeFrame;
    }

    return CGSizeMake(CGRectGetWidth(screenBounds), CGRectGetHeight(screenBounds));
}

/// Attention. NSLayoutConstraint with `[self getWindow].safeAreaLayoutGuide` can be lost (removed) after Window refreshed (Interstitial ad), use `superview.safeAreaLayoutGuide` instead.
- (void)refreshPositionInSafeArea:(UILayoutGuide *)safeArea {
    if (!safeArea) {
        return;
    }

    if (self.constraintX) {
        [NSLayoutConstraint deactivateConstraints:@[self.constraintX, self.constraintY]];
    }

    switch (_activePos) {
        case kCASUPosition_TOP_CENTER:
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_TOP_RIGHT:
            self.constraintY = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeTop relatedBy:NSLayoutRelationEqual toItem: _isPortraitOrientationSupported ? safeArea : [CASUPluginUtil unityWindow] attribute:NSLayoutAttributeTop multiplier:1.0 constant:_verticalOffset];
            break;

        case kCASUPosition_BOTTOM_CENTER:
        case kCASUPosition_BOTTOM_LEFT:
        case kCASUPosition_BOTTOM_RIGHT:
            self.constraintY = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeBottom relatedBy:NSLayoutRelationEqual toItem:safeArea attribute:NSLayoutAttributeBottom multiplier:1.0 constant:-_verticalOffset];
            break;

        default:
            self.constraintY = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeCenterY relatedBy:NSLayoutRelationEqual toItem:safeArea attribute:NSLayoutAttributeCenterY multiplier:1.0 constant:0.0];
            break;
    }

    switch (_activePos) {
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_BOTTOM_LEFT:
        case kCASUPosition_MIDDLE_LEFT:
            self.constraintX = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeLeft relatedBy:NSLayoutRelationEqual toItem:safeArea attribute:NSLayoutAttributeLeft multiplier:1.0 constant:_horizontalOffset];
            break;

        case kCASUPosition_TOP_RIGHT:
        case kCASUPosition_BOTTOM_RIGHT:
        case kCASUPosition_MIDDLE_RIGHT:
            self.constraintX = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeRight relatedBy:NSLayoutRelationEqual toItem:safeArea attribute:NSLayoutAttributeRight multiplier:1.0 constant:-_horizontalOffset];
            break;

        default:
            self.constraintX = [NSLayoutConstraint constraintWithItem:self.bannerView attribute:NSLayoutAttributeCenterX relatedBy:NSLayoutRelationEqual toItem:safeArea attribute:NSLayoutAttributeCenterX multiplier:1.0 constant:0.0];
            break;
    }

    self.constraintY.priority = UILayoutPriorityDefaultHigh;
    self.constraintX.priority = UILayoutPriorityDefaultHigh;
    [NSLayoutConstraint activateConstraints:@[self.constraintX, self.constraintY]];
}

- (CGRect)calculateRect {
    if (!self.bannerView) {
        return CGRectZero;
    }

    CGRect screenRect = [CASUPluginUtil unityWindow].bounds;
    CGRect safeAreaRect = self.bannerView.superview.safeAreaLayoutGuide.layoutFrame;

    if (CGSizeEqualToSize(safeAreaRect.size, CGSizeZero)) {
        safeAreaRect = screenRect;
    }

    CGSize adSize = self.bannerView.intrinsicContentSize;

    if (CGSizeEqualToSize(CGSizeZero, adSize)) {
        adSize = [self.bannerView.adSize toCGSize];
    }

    CGFloat verticalPos;
    CGFloat horizontalPos;
    switch (_activePos) {
        case kCASUPosition_TOP_CENTER:
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_TOP_RIGHT:
            verticalPos = _verticalOffset;
            break;

        case kCASUPosition_BOTTOM_CENTER:
        case kCASUPosition_BOTTOM_LEFT:
        case kCASUPosition_BOTTOM_RIGHT:
            verticalPos = CGRectGetMaxY(screenRect) - adSize.height - _verticalOffset;
            break;

        default:
            verticalPos = CGRectGetMidY(screenRect) - adSize.height * 0.5;
            break;
    }

    switch (_activePos) {
        case kCASUPosition_TOP_LEFT:
        case kCASUPosition_BOTTOM_LEFT:
        case kCASUPosition_MIDDLE_LEFT:
            horizontalPos = _horizontalOffset;
            break;

        case kCASUPosition_TOP_RIGHT:
        case kCASUPosition_BOTTOM_RIGHT:
        case kCASUPosition_MIDDLE_RIGHT:
            horizontalPos = CGRectGetMaxX(screenRect) - adSize.width - _horizontalOffset;
            break;

        default:
            horizontalPos = CGRectGetMidX(screenRect) - adSize.width * 0.5;
            break;
    }

    verticalPos = [self clampFloat:verticalPos
                               min:CGRectGetMinY(safeAreaRect)
                               max:CGRectGetMaxY(safeAreaRect) - adSize.height];

    horizontalPos = [self clampFloat:horizontalPos
                                 min:CGRectGetMinX(safeAreaRect)
                                 max:CGRectGetMaxX(safeAreaRect) - adSize.width];

    return CGRectMake(horizontalPos, verticalPos, adSize.width, adSize.height);
}

- (CGFloat)clampFloat:(CGFloat)value min:(CGFloat)minValue max:(CGFloat)maxValue {
    return MIN(MAX(value, minValue), maxValue);
}

@end
