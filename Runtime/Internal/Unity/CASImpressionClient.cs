//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_EDITOR || TARGET_OS_SIMULATOR

namespace CAS.Unity
{
    public class CASImpressionClient : AdMetaData
    {
        public CASImpressionClient( AdType type ) : base( type )
        {
        }

        public override AdNetwork network
        {
            get
            {
                return AdNetwork.CrossPromotion;
            }
        }

        public override double cpm
        {
            get
            {
                return 0.0;
            }
        }

        public override PriceAccuracy priceAccuracy
        {
            get
            {
                return PriceAccuracy.Floor;
            }
        }

        public override string creativeIdentifier
        {
            get
            {
                return null;
            }
        }

        public override string identifier
        {
            get
            {
                return "Test";
            }
        }
    }
}

#endif