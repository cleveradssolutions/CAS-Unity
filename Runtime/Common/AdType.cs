//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

using System;

namespace CAS
{
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
        [Obsolete( "Comming soon" )]
        Native,
        None,
    }

    [Flags]
    public enum AdFlags
    {
        None = 0,
        Banner = 1,
        Interstitial = 2,
        Rewarded = 4,
        Native = 8,
        Everything = Banner | Interstitial | Rewarded | Native
    }
}