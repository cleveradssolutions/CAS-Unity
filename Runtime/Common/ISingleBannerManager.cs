//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;
namespace CAS
{
    /// <summary>
    /// To use the obsolete single banner manager,
    /// you need to activate it in the `Assets/CleverAdsSolutions/Settings` window.
    /// </summary>
    public interface ISingleBannerManager
    {
        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).OnPresenting" )]
        event Action OnBannerAdShown;
        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).OnPresenting" )]
        event CASEventWithMeta OnBannerAdOpening;
        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).OnFailed" )]
        event CASEventWithError OnBannerAdFailedToShow;
        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).OnClicked" )]
        event Action OnBannerAdClicked;
        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).OnHidden" )]
        event Action OnBannerAdHidden;

        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize)" )]
        AdSize bannerSize { get; set; }

        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).position" )]
        AdPosition bannerPosition { get; set; }

        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).SetActive(false)" )]
        void HideBanner();

        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).rectInPixels.height" )]
        float GetBannerHeightInPixels();

        [Obsolete( "Migrated to new multiple banner interface: manager.GetAdView(AdSize).rectInPixels.width" )]
        float GetBannerWidthInPixels();
    }
}