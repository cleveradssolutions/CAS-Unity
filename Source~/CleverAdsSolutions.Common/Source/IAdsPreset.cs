//  Copyright © 2024 CAS.AI. All rights reserved.

namespace CAS
{
    public interface IAdsPreset
    {
        int defaultBannerRefresh { get; }
        int defaultInterstitialInterval { get; }
        bool defaultDebugModeEnabled { get; }
        bool defaultLocationCollectionEnabled { get; }
        [System.Obsolete("Use defaultLocationCollectionEnabled instead.")]
        bool defaultIOSTrackLocationEnabled { get; }
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