//  Copyright © 2025 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    public enum AdNetwork
    {
        GoogleAds = 0,
        LiftoffMonetize = 1,
        Kidoz = 2,
        Chartboost = 3,
        UnityAds = 4,
        AppLovin = 5,
        [Obsolete("No longer supported")]
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
        Mintegral = 23,
        Pangle = 24,
        YsoNetwork = 25,
        Prado = 26,
        Maticoo = 27,
        Verve = 28,

        DSPExchange = 30,
        LastPage = 31,

    }

    public static class AdNetworkExtension
    {
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
                case AdNetwork.Madex: return "Mdx";
                case AdNetwork.StartIO: return "SIO";
                case AdNetwork.Ogury: return "Og";
                case AdNetwork.CASExchange: return "Ex";
                case AdNetwork.YsoNetwork: return "YSO";
                case AdNetwork.Prado: return "Pr";
                case AdNetwork.Maticoo: return "Mtc";
                case AdNetwork.Verve: return "Vr";
                default: return string.Empty;
            }
        }
    }
}