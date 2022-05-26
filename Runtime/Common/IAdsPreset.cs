//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

namespace CAS
{
    public interface IAdsPreset
    {
        int defaultBannerRefresh { get; }
        int defaultInterstitialInterval { get; }
        bool defaultDebugModeEnabled { get; }
        bool defaultIOSTrackLocationEnabled { get; }
        bool defaultAnalyticsCollectionEnabled { get; }
        bool defaultInterstitialWhenNoRewardedAd { get; }
        AdFlags defaultAllowedFormats { get; }
        Audience defaultAudienceTagged { get; }
        LoadingManagerMode defaultLoadingMode { get; }

        int managersCount { get; }
        string GetManagerId( int index );
        int IndexOfManagerId( string id );
        bool IsTestAdMode();
    }
}