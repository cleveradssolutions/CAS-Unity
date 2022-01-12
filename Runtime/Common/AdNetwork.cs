using System;

namespace CAS
{
    public enum AdNetwork
    {
        LastPage = -1,
        GoogleAds = 0,
        Vungle,
        Kidoz,
        [Obsolete( "No longer supported" )] Chartboost,
        UnityAds,
        AppLovin,
        SuperAwesome,
        [Obsolete( "No longer supported" )] StartApp,
        AdColony,
        FacebookAN,
        InMobi,
        [Obsolete( "No longer supported" )] MobFox,
        MyTarget,
        CrossPromotion,
        IronSource,
        YandexAds,
        [Obsolete( "No longer supported" )] OwnVAST,
        MAX,
        [Obsolete( "No longer supported" )] Smaato,
        [Obsolete( "No longer supported" )] MoPub,
        Tapjoy,
        [Obsolete( "No longer supported" )] Fyber,
        FairBid,
        Mintegral,
        Pangle,
        [Obsolete( "No longer supported" )] Verizon,
        [Obsolete( "No longer supported" )] AmazonAds,
        [Obsolete( "No longer supported" )] HyperMX
    }

    public static class AdNetworkExtension
    {
        public static string GetPrivacyPolicy( this AdNetwork network )
        {
            switch (network)
            {
                case AdNetwork.GoogleAds:
                    return "https://policies.google.com/technologies/ads";
                case AdNetwork.Vungle:
                    return "https://vungle.com/privacy/";
                case AdNetwork.Kidoz:
                    return "https://kidoz.net/privacy-policy/";
                case AdNetwork.UnityAds:
                    return "https://unity3d.com/legal/privacy-policy";
                case AdNetwork.AppLovin:
                    return "https://www.applovin.com/privacy/";
                case AdNetwork.SuperAwesome:
                    return "https://www.superawesome.com/privacy-hub/privacy-policy/";
                case AdNetwork.AdColony:
                    return "https://www.adcolony.com/privacy-policy/";
                case AdNetwork.FacebookAN:
                    return "https://developers.facebook.com/docs/audience-network/policy/";
                case AdNetwork.InMobi:
                    return "https://www.inmobi.com/privacy-policy/";
                case AdNetwork.MyTarget:
                    return "https://legal.my.com/us/mytarget/privacy/";
                case AdNetwork.IronSource:
                    return "https://developers.ironsrc.com/ironsource-mobile/air/ironsource-mobile-privacy-policy/";
                case AdNetwork.YandexAds:
                    return "https://yandex.com/legal/mobileads_sdk_agreement/";
                case AdNetwork.Tapjoy:
                    return "https://www.tapjoy.com/legal/players/privacy-policy/";
                case AdNetwork.Mintegral:
                    return "https://www.mintegral.com/en/privacy/";
                case AdNetwork.Pangle:
                    return "https://www.pangleglobal.com/privacy/enduser-en";
                default: return null;
            }
        }

        public static string[] GetListOfTags()
        {
            return new string[]
            {
                "AM",
                "V",
                "K",
                "CB",
                "U",
                "AL",
                "SuA",
                "StA",
                "AC",
                "FB",
                "IM",
                "MF",
                "MT",
                "P",
                "IS",
                "Ya",
                string.Empty, //VAST
                string.Empty, //MAX
                "Sm",
                "MP",
                "TJ",
                string.Empty, //Fyber
                "Fy",
                "MB",
                "Pa"
            };
        }
    }
}