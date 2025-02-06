//  Copyright © 2024 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    public enum AdNetwork
    {
        [Obsolete("Renamed to DTExchange")]
        Fyber = DTExchange,
        [Obsolete("Renamed to DTExchange")]
        DigitalTurbine = DTExchange,
        [Obsolete("Renamed to AudienceNetwork")]
        FacebookAN = AudienceNetwork,
        [Obsolete("Renamed to LiftoffMonetize")]
        Vungle = LiftoffMonetize,

        GoogleAds = 0,
        LiftoffMonetize = 1,
        Kidoz = 2,
        Chartboost = 3,
        UnityAds = 4,
        AppLovin = 5,
        SuperAwesome = 6,
        StartIO = 7,
        CASExchange = 8,
        AudienceNetwork = 9,
        InMobi = 10,
        DTExchange = 11,
        [Obsolete("No longer supported")]
        MyTarget = 12,
        CrossPromotion = 13,
        IronSource = 14,
        YandexAds = 15,
        HyprMX = 16,

        Smaato = 18,
        Bigo = 19,
        Ogury = 20,
        Madex = 21,
        [Obsolete("No longer supported")]
        LoopMe = 22,
        Mintegral = 23,
        Pangle = 24,
        YsoNetwork = 25,

        DSPExchange = 30,
        LastPage = 31,

        [Obsolete("No longer supported")]
        Tapjoy = 32,
        [Obsolete("No longer supported")]
        AdColony = 33,
    }

    public static class AdNetworkExtension
    {
        public static string GetExtraKeyGDPR(this AdNetwork network)
        {
            return network.GetTag() + "_gdpr";
        }

        public static string GetExtraKeyCCPA(this AdNetwork network)
        {
            return network.GetTag() + "_ccpa";
        }

        public static string GetPrivacyPolicy(this AdNetwork network)
        {
            switch (network)
            {
                case AdNetwork.GoogleAds:
                    return "https://policies.google.com/technologies/ads";
                case AdNetwork.LiftoffMonetize:
                    return "https://liftoff.io/ru/privacy-policy/";
                case AdNetwork.Kidoz:
                    return "https://kidoz.net/privacy-policies";
                case AdNetwork.UnityAds:
                    return "https://unity3d.com/legal/privacy-policy";
                case AdNetwork.AppLovin:
                    return "https://www.applovin.com/privacy/";
                case AdNetwork.SuperAwesome:
                    return "https://www.superawesome.com/privacy-hub/privacy-policy/";
                case AdNetwork.AudienceNetwork:
                    return "https://developers.facebook.com/docs/audience-network/policy/";
                case AdNetwork.InMobi:
                    return "https://www.inmobi.com/privacy-policy/";
                case AdNetwork.IronSource:
                    return "https://developers.is.com/ironsource-mobile/air/ironsource-mobile-privacy-policy/";
                case AdNetwork.YandexAds:
                    return "https://yandex.com/legal/mobileads_sdk_agreement/";
                case AdNetwork.Mintegral:
                    return "https://www.mintegral.com/en/privacy/";
                case AdNetwork.Pangle:
                    return "https://www.pangleglobal.com/privacy/enduser-en";
                case AdNetwork.Chartboost:
                    return "https://support.chartboost.com/en/legal/privacy-policy";
                case AdNetwork.DTExchange:
                    return "https://www.digitalturbine.com/privacy-policy/";
                case AdNetwork.Bigo:
                    return "https://www.adsbigo.com/privacy.html";
                case AdNetwork.StartIO:
                    return "https://www.start.io/policy/privacy-policy-site/";
                case AdNetwork.Ogury:
                    return "https://ogury.com/ogury-advertising-privacy-policy/";
                case AdNetwork.YsoNetwork:
                    return "https://www.ysonetwork.com/privacy";
                default: return null;
            }
        }

        public static string GetTag(this AdNetwork network)
        {
            switch (network)
            {
                case AdNetwork.GoogleAds: return "AM";
                case AdNetwork.LiftoffMonetize: return "V";
                case AdNetwork.Kidoz: return "K";
                case AdNetwork.Chartboost: return "CB";
                case AdNetwork.UnityAds: return "U";
                case AdNetwork.AppLovin: return "AL";
                case AdNetwork.SuperAwesome: return "SuA";
                case AdNetwork.AudienceNetwork: return "FB";
                case AdNetwork.InMobi: return "IM";
                case AdNetwork.CrossPromotion: return "P";
                case AdNetwork.IronSource: return "IS";
                case AdNetwork.YandexAds: return "Ya";
                case AdNetwork.DTExchange: return "Fy";
                case AdNetwork.Mintegral: return "MB";
                case AdNetwork.Pangle: return "Pa";
                case AdNetwork.HyprMX: return "HMX";
                case AdNetwork.Smaato: return "Sma";
                case AdNetwork.Bigo: return "Big";
                case AdNetwork.Madex: return "Ma";
                case AdNetwork.StartIO: return "SIO";
                case AdNetwork.Ogury: return "Og";
                case AdNetwork.CASExchange: return "Ex";
                case AdNetwork.YsoNetwork: return "YSO";
                default: return string.Empty;
            }
        }

        [Obsolete("Use .GetTag() instead")]
        public static string[] GetListOfTags()
        {
            return new string[0];
        }
    }
}