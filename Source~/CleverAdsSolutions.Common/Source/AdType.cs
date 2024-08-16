//  Copyright © 2024 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    public enum AdType
    {
        Banner = 0,
        Interstitial = 1,
        Rewarded = 2,
        AppOpen = 3,
        [Obsolete("Not supported")]
        Native = 4,
        None,
    }

    [Flags]
    public enum AdFlags
    {
        None = 0,
        Banner = 1,
        Interstitial = 1 << 1,
        Rewarded = 1 << 2,
        AppOpen = 1 << 3,
        Everything = Banner | Interstitial | Rewarded | AppOpen
    }
}