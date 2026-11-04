//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    public class CASImpressionClient : AdMetaData
    {
        private readonly AndroidJavaObject impression;

        public CASImpressionClient(AndroidJavaObject impression) : base(GetAdFormat(impression))
        {
            this.impression = impression;
        }

        public override AdNetwork network
        {
            get { return (AdNetwork)impression.Call<int>( "getNetwork" ); }
        }

        public override string SourceName
        {
            get { return impression.Call<string>("getSourceName"); }
        }

        public override double revenue
        {
            get
            {
                return impression.Call<long>("getCpm") / 1000000.0;
            }
        }

        public override double cpm
        {
            get
            {
                return impression.Call<long>("getCpm") / 1000.0;
            }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return (PriceAccuracy)impression.Call<int>( "getPriceAccuracy" ); }
        }

        public override string creativeIdentifier
        {
            get { return impression.Call<string>( "getCreativeIdentifier" ); }
        }

        public override string sourceUnitId
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

        private static AdFormat GetAdFormat(AndroidJavaObject impression)
        {
            int format = impression.Call<int>("getFormatValue");

            if (format == 6)
                return AdFormat.Banner;

            return (AdFormat)format;
        }

    }
}

#endif