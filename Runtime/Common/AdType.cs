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
        MediumRectangle = 16,
        Everything = Banner | Interstitial | Rewarded | Native | MediumRectangle
    }
}