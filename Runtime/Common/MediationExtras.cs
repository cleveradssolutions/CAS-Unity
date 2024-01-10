//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;

namespace CAS
{
    public class MediationExtras
    {
        /// <summary>
        /// Advertising Tracking Enabled for Audience Network for iOS Only.
        /// <para>Set the `FBAdSettings.setAdvertiserTrackingEnabled` flag.</para>
        /// <para>Value "1" flag to inform Audience Network to use the data to deliver personalized ads
        /// in line with your own legal obligations, platform terms, and commitments you’ve made to your users</para>
        /// <para>Value "0" flag to inform Audience Network to not be able to deliver personalized ads.</para>
        /// </summary>
        [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Additional-Meta-AudienceNetwork-steps" )]
        public static string facebookAdvertiserTracking = "FB_track";

        /// <summary>
        /// Facebook Data Processing Options for Users in California
        /// <para>Set the `FBAdSettings.setDataProcessingOptions` flag.</para>
        /// <para>Values:</para>
        /// <para>- ""  - To explicitly not enable Limited Data Use (LDU) mode</para>
        /// <para>- "LDU"  - To enable LDU mode using geolocation</para>
        /// <para>- "LDU_1_1000"  - To enable LDU for users and specify user geography</para>
        ///
        /// <para>For information about how to implement Facebook’s Limited Data Use flag in California,
        /// visit <a href="https://developers.facebook.com/docs/marketing-apis/data-processing-options">Facebook’s developer documentation</a>.</para>
        /// </summary>
        [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Additional-Meta-AudienceNetwork-steps" )]
        public static string facebookDataProcessing = "FB_dp";

        public static void SetGlobalEtras( Dictionary<string, string> extras )
        {
            CASFactory.SetGlobalMediationExtras( extras );
        }
    }
}
