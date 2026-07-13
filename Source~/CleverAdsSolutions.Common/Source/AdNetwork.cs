//  Copyright © 2025 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    public enum AdNetwork
    {
        Unknown = 0,
        GoogleAds = 2,
        LiftoffMonetize = 9,
        Kidoz = 11,
        Chartboost = 7,
        UnityAds = 8,
        AppLovin = 13,
        SuperAwesome = 16,
        StartIO = 45,
        CASExchange = 49,
        AudienceNetwork = 10,
        InMobi = 4,
        DTExchange = 32,
        CrossPromotion = 17,
        IronSource = 18,
        YangoAds = 19,
        HyprMX = 47,
        Smaato = 6,
        Bigo = 38,
        Ogury = 50,
        Madex = 44,
        MonetriX = 71,
        Mintegral = 28,
        Pangle = 30,
        YsoNetwork = 52,
        Prado = 57,
        Maticoo = 62,
        PubMatic = 70,
        Verve = 65,
        DSPExchange = 33,
        LastPage = 23,
        DisplayIO = 73,
        Bidease = 74,
        Moloco = 76,
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
                case AdNetwork.YangoAds: return "Ya";
                case AdNetwork.SuperAwesome: return "SA";
                case AdNetwork.DTExchange: return "Fy";
                case AdNetwork.MonetriX: return "MtX";
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
                case AdNetwork.PubMatic: return "PM";
                case AdNetwork.Verve: return "Vr";
                case AdNetwork.DisplayIO: return "DIO";
                case AdNetwork.Bidease: return "Bse";
                case AdNetwork.Moloco: return "Mol";
                default: return string.Empty;
            }
        }
    }
}