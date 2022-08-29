//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Globalization;
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
            get { return ( AdNetwork )impression.Call<int>( "getNetwork" ); }
        }

        public override double cpm
        {
            get { return impression.Call<double>( "getCpm" ); }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return ( PriceAccuracy )impression.Call<int>( "getPriceAccuracy" ); }
        }

        public override string creativeIdentifier
        {
            get { return impression.Call<string>( "getCreativeIdentifier" ); }
        }

        public override string identifier
        {
            get { return impression.Call<string>( "getIdentifier" ); }
        }
    }
}

#endif