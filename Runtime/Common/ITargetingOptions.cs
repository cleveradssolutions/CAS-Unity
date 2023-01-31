//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

namespace CAS
{
    public enum Gender
    {
        Unknown, Male, Female
    }

    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-CAS#targeting-options" )]
    public interface ITargetingOptions
    {
        /// <summary>
        /// The user’s gender
        /// </summary>
        Gender gender { get; set; }
        /// <summary>
        /// The user’s age
        /// Limitation: 1-99 and 0 is 'unknown'
        /// </summary>
        int age { get; set; }
    }
}
