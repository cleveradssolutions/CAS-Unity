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
}