//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
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

        [Obsolete( "Renamed to Banner" )]
        Small = Banner,
    }

    [Flags]
    public enum AdFlags
    {
        None = 0,
        Banner = 1,
        Interstitial = 2,
        Rewarded = 4,
        Native = 8,
        [Obsolete( "Not longer used. Use Banner instead." )]
        MediumRectangle = 16,
#pragma warning disable CS0618 // Type or member is obsolete
        Everything = Banner | Interstitial | Rewarded | Native | MediumRectangle
#pragma warning restore CS0618 // Type or member is obsolete
    }
}