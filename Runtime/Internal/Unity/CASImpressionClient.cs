//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_EDITOR

namespace CAS.Unity
{
    public class CASImpressionClient : AdMetaData
    {
        private static int depth = 0;

        public CASImpressionClient(AdType type) : base(type)
        {
            depth++;
        }

        public override AdNetwork network
        {
            get { return AdNetwork.CrossPromotion; }
        }

        public override double revenue
        {
            get { return 0.001; }
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