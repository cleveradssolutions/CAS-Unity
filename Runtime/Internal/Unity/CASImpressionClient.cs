﻿//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_EDITOR || TARGET_OS_SIMULATOR

namespace CAS.Unity
{
    public class CASImpressionClient : AdMetaData
    {
        private static int depth = 0;

        public CASImpressionClient( AdType type ) : base( type )
        {
            depth++;
        }

        public override AdNetwork network
        {
            get { return AdNetwork.CrossPromotion; }
        }

        public override double cpm
        {
            get { return 1.0; }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return PriceAccuracy.Floor; }
        }

        public override string creativeIdentifier
        {
            get { return null; }
        }

        public override string identifier
        {
            get { return "Test"; }
        }

        public override int impressionDepth
        {
            get { return depth; }
        }

        public override double lifetimeRevenue
        {
            get { return depth * 0.001; }
        }
    }
}

#endif