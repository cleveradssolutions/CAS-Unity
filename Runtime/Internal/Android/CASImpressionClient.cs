//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    public class CASImpressionClient : AdMetaData
    {
        private readonly AndroidJavaObject impression;

        public CASImpressionClient( AdType type, AndroidJavaObject impression ) : base( type )
        {
            this.impression = impression;
        }

        public override AdNetwork network
        {
            get { return (AdNetwork)impression.Call<int>( "getNetwork" ); }
        }

        public override double revenue
        {
            get { return cpm / 1000.0; }
        }

        public override double cpm
        {
            get { return impression.Call<long>( "getCpm" ) / 1000.0; }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return (PriceAccuracy)impression.Call<int>( "getPriceAccuracy" ); }
        }

        public override string creativeIdentifier
        {
            get { return impression.Call<string>( "getCreativeIdentifier" ); }
        }

        public override string identifier
        {
            get { return impression.Call<string>( "getIdentifier" ); }
        }

        public override int impressionDepth
        {
            get { return impression.Call<int>( "getImpressionDepth" ); }
        }

        public override double lifetimeRevenue
        {
            get { return impression.Call<long>( "getLifetimeRevenue" ) / 1000000.0; }
        }
    }
}

#endif