using System.Collections.Generic;

namespace CAS
{
    public class MediationExtras
    {
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

        public const string ironSourceGDPRConsent = "IS_gdpr";
        public const string ironSourceCCPAOptedOut = "IS_ccpa";

        public const string unityAdsGDPRConsent = "U_gdpr";
        public const string unityAdsCCPAOptedOut = "U_ccpa";

        public const string startAppGDPRConsent = "StA_gdpr";

        [System.Obsolete( "Endless flag does not affect on cross-promo.")]
        public const string crossPromoEndless = "P_endless";

        public const string myTargetGDPRConsent = "MT_gdpr";
        public const string myTargetCCPAOptedOut = "MT_ccpa";

        public const string yandexAdsGDPRConsent = "Ya_gdpr";

        public const string tapjoyGDPRConsent = "TJ_gdpr";
        public const string tapjoyCCPAOptedOut = "TJ_ccpa";

        public const string fyberGDPRConsent = "Fy_gdpr";
        public const string fyberCCPAOptedOut = "Fy_ccpa";

        public const string facebookGDPRConsent = "FB_gdpr";
        public const string facebookCCPAOptedOut = "FB_ccpa";

        public const string mintegralGDPRConsent = "MB_gdpr";
        public const string mintegralCCPAOptedOut = "MB_ccpa";

        public static void SetGlobalEtras( Dictionary<string, string> extras )
        {
            CASFactory.SetGlobalMediationExtras( extras );
        }
    }
}
