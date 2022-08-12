//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace CAS
{
    internal class WikiPageAttribute : Attribute
    {
        internal WikiPageAttribute( string url ) { }
    }

    public class MediationExtras
    {
        /// <summary>
        /// Advertising Tracking Enabled for Audience Network for iOS Only.
        /// <para>Set the `FBAdSettings.setAdvertiserTrackingEnabled` flag.</para>
        /// <para>Value "1" flag to inform Audience Network to use the data to deliver personalized ads
        /// in line with your own legal obligations, platform terms, and commitments you’ve made to your users</para>
        /// <para>Value "0" flag to inform Audience Network to not be able to deliver personalized ads.</para>
        /// </summary>
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
        public static string facebookDataProcessing = "FB_dp";

        /// <summary>
        /// Enables/disables automatic fallback collection of Android ID in case ifa is not available.
        /// <b>Android only.</b>
        ///
        /// Default value is "0".
        /// if "1" disables the collection.
        // endregion
        public const string vungleAndroidIdOptedOut = "V_aIDOpt";

        /// <summary>
        /// Sets the publish IDFV flag.
        /// <b>iOS only.</b>
        ///
        /// This value is persistent and so may be set once.
        /// Default value is "1".
        /// if "0" no publish the IDFV value.
        /// </summary>
        public const string vunglePublishIDFV = "V_aIDOpt";

        public const string AdMobGDPRConsent = "AM_gdpr";
        public const string AdMobCCPAOptedOut = "AM_ccpa";

        public const string AppLovinGDPRConsent = "AL_gdpr";
        public const string AppLovinCCPAOptedOut = "AL_ccpa";
        /// <summary>
        /// Value "1" to enable MAX
        /// </summary>
        public const string AppLovinUseMAX = "AL_max";

        public const string InMobiGDPRConsent = "IM_gdpr";
        /// <summary>
        /// Provide user consent in IAB string format
        /// </summary>
        public const string InMobiGDPRIAB = "IM_iab";

        public const string AdColonyGDPRConsent = "AC_gdpr";
        public const string AdColonyCCPAOptedOut = "AC_ccpa";

        public const string VungleGDPRConsent = "V_gdpr";
        public const string VungleCCPAOptedOut = "V_ccpa";

        public const string ironSourceGDPRConsent = "IS_gdpr";
        public const string ironSourceCCPAOptedOut = "IS_ccpa";

        public const string unityAdsGDPRConsent = "U_gdpr";
        public const string unityAdsCCPAOptedOut = "U_ccpa";

        public const string startAppGDPRConsent = "StA_gdpr";

        public const string myTargetGDPRConsent = "MT_gdpr";
        public const string myTargetCCPAOptedOut = "MT_ccpa";

        public const string yandexAdsGDPRConsent = "Ya_gdpr";

        public const string tapjoyGDPRConsent = "TJ_gdpr";
        public const string tapjoyCCPAOptedOut = "TJ_ccpa";

        public const string fyberGDPRConsent = "Fy_gdpr";
        public const string fyberCCPAOptedOut = "Fy_ccpa";

        [Obsolete( "Migrated to facebookAdvertiserTracking option." )]
        public const string facebookGDPRConsent = "FB_gdpr";
        [Obsolete( "Migrated to facebookDataProcessing option." )]
        public const string facebookCCPAOptedOut = "FB_ccpa";

        public const string mintegralGDPRConsent = "MB_gdpr";
        public const string mintegralCCPAOptedOut = "MB_ccpa";

        public static void SetGlobalEtras( Dictionary<string, string> extras )
        {
            CASFactory.SetGlobalMediationExtras( extras );
        }
    }
}
