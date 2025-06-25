//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;

namespace CAS
{
    public enum Gender
    {
        Unknown, Male, Female
    }

    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-CAS#targeting-options")]
    public interface ITargetingOptions
    {
        /// <summary>
        /// The user’s gender
        /// </summary>
        Gender gender { get; set; }
        /// <summary>
        /// The user’s age
        /// <para>Limitation: 1-99 and 0 is 'unknown'</para>
        /// </summary>
        int age { get; set; }
        /// <summary>
        /// Collect from the device the latitude and longitude coordinated truncated to the
        /// hundredths decimal place.
        /// <para>Collect only if your application already has the relevant end-user permissions.</para>
        /// <para>Does not collect if the target audience is children.</para>
        /// <para>By default selected in `Assets/CleverAdsSolutions/Settings` menu</para>
        /// </summary>
        bool locationCollectionEnabled { get; set; }
        /// <summary>
        /// Sets the content URL for a web site whose content matches the app's primary content.
        /// This web site content is used for targeting and brand safety purposes.
        /// <para>Limitation: max URL length 512</para>
        /// </summary>
        string contentURL { get; set; }
        /// <summary>
        /// A list of keywords, interests, or intents related to your application.
        /// Words or phrase describing the current activity of the user for targeting purposes.
        /// </summary>
        void SetKeywords(IList<string> keywords);
    }
}
