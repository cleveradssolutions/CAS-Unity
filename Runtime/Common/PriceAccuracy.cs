//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

namespace CAS
{
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Impression-Level-Data" )]
    public enum PriceAccuracy
    {
        /// <summary>
        /// eCPM floor, also known as minimum eCPMs
        /// </summary>
        Floor,
        /// <summary>
        /// eCPM is the exact and committed value per 1000 impressions.
        /// </summary>
        Bid,
        /// <summary>
        /// When the demand source does not agree to disclose the payout of every impression - in such cases the cpm is ‘0’
        /// </summary>
        Undisclosed
    }
}
