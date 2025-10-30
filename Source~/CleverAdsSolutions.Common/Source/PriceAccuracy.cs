﻿//  Copyright © 2025 CAS.AI. All rights reserved.

namespace CAS
{
    [WikiPage( "https://docs.page/cleveradssolutions/docs/Unity/Impression-Level-Data" )]
    public enum PriceAccuracy
    {
        /// <summary>
        /// The estimated revenue, can also be a minimum price (floor) for ad impression.
        /// </summary>
        Floor,
        /// <summary>
        /// The revenue provided as part of the real-time auction.
        /// </summary>
        Bid,
        /// <summary>
        /// The revenue is '0', when the demand source does not agree to disclose the payout of impression.
        /// </summary>
        Undisclosed
    }
}
