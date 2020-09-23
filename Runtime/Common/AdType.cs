using System;

namespace CAS
{
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
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
        Everything = Banner | Interstitial | Rewarded | Native
    }
}