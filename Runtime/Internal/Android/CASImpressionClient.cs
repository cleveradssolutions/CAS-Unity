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
        private readonly IDictionary<string, string> fields;

        public CASImpressionClient( AdType type, string parameters ) : base( type )
        {
            fields = CASFactory.ParseParametersString( parameters );
        }

        public override AdNetwork network
        {
            get { return ( AdNetwork )GetInt( "network", -1 ); }
        }

        public override double cpm
        {
            get { return GetDouble( "cpm", 0.0 ); }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return ( PriceAccuracy )GetInt( "accuracy", ( int )PriceAccuracy.Undisclosed ); }
        }

        public override string creativeIdentifier
        {
            get { return GetValue( "creative", null ); }
        }

        public override string identifier
        {
            get { return GetValue( "id", string.Empty ); }
        }

        public string GetValue( string key, string defVal )
        {
            string str;
            return fields.TryGetValue( key, out str ) ? str : defVal;
        }

        public int GetInt( string key, int defVal )
        {
            try
            {
                string str;
                if (fields.TryGetValue( key, out str ))
                    return int.Parse( str, NumberStyles.Integer, CultureInfo.CurrentCulture );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            return defVal;
        }

        public double GetDouble( string key, double defVal )
        {
            try
            {
                string str;
                if (fields.TryGetValue( key, out str ))
                    return double.Parse( str.Replace( ',', '.' ),
                        NumberStyles.Integer | NumberStyles.AllowDecimalPoint,
                        NumberFormatInfo.InvariantInfo );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            return defVal;
        }
    }
}

#endif