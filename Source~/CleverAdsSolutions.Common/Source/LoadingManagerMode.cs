//  Copyright © 2025 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/Other-Options#loading-mode")]
    public enum LoadingManagerMode
    {
        [Obsolete("Not longer support, replaced to Auto")]
        FastestRequests = 0,
        [Obsolete("Not longer support, replaced to Auto")]
        FastRequests = 1,

        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides balanced polling rate of mediation networks for advertising content.</para>
        /// <para>Doesn't significantly affect application performance.</para>
        /// </summary>
        Auto = 2,

        /// <summary>
        /// Same as Auto
        /// </summary>
        Optimal = Auto,

        [Obsolete("Not longer support, replaced to Auto")]
        HighePerformance = 3,

        [Obsolete("Not longer support, replaced to Auto")]
        HighestPerformance = 4,

        /// <summary>
        /// Manual control loading mediation ads.
        /// <para>Provides minimal impact on application performance.</para>
        /// <para>But it requires manual preparation of advertising content for display.</para>
        /// <para>Use ad loading methods before trying to show: <see cref="IMediationManager.LoadAd(AdType)"/></para>
        /// <para>Reduces memory reservations for advertising content.</para>
        /// </summary>
        Manual = 5
    }
}
