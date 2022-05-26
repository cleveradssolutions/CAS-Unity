//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// Wiki page: https://github.com/cleveradssolutions/CAS-Unity/wiki/Impression-Level-Data
    /// </summary>
    public sealed class AdMetaData
    {
        /// <summary>
        /// The Format Type of the impression.
        /// </summary>
        public readonly AdType type;

        /// <summary>
        /// The mediated network’s name that purchased the impression.
        /// </summary>
        public AdNetwork network
        {
            get { return ( AdNetwork )GetInt( "network", -1 ); }
        }

        /// <summary>
        /// The Cost Per Mille estimated impressions of the ad in USD.
        /// <para>The value accuracy is returned in the <see cref="priceAccuracy"/> property.</para>
        /// </summary>
        public double cpm
        {
            get { return GetDouble( "cpm", 0.0 ); }
        }

        /// <summary>
        /// Accuracy of the CPM value.
        /// </summary>
        public PriceAccuracy priceAccuracy
        {
            get { return ( PriceAccuracy )GetInt( "accuracy", ( int )PriceAccuracy.Undisclosed ); }
        }

        /// <summary>
        /// The creative id tied to the ad, if any. May be null.
        /// You can report creative issues to our Ad review team using this id.
        /// </summary>
        public string creativeIdentifier
        {
            get { return GetValue( "creative", null ); }
        }

        /// <summary>
        /// Internal demand source name in CAS database.
        /// </summary>
        public string identifier
        {
            get { return GetValue( "id", string.Empty ); }
        }

        private readonly IDictionary<string, string> field = null;

        public AdMetaData( AdType type, IDictionary<string, string> field )
        {
            this.type = type;
            this.field = field;
        }

        public override string ToString()
        {
            var result = new StringBuilder( "Impression of the " )
                .Append( type )
                .Append( " ads with " );

            var precision = priceAccuracy;
            if (precision == PriceAccuracy.Undisclosed)
            {
                result.Append( "undisclosed cost " );
            }
            else
            {
                if (precision == PriceAccuracy.Floor)
                    result.Append( "a floor " );
                else
                    result.Append( "an average " );
                result.Append( cpm )
                    .Append( " CPM " );
            }
            result.Append( "from " )
                .Append( network );

            var creative = creativeIdentifier;
            if (!string.IsNullOrEmpty( creative ))
                result.Append( " creative id: " )
                    .Append( creative );

            return result.ToString();
        }

        public string GetValue( string key, string defVal )
        {
            string str;
            return field.TryGetValue( key, out str ) ? str : defVal;
        }

        public int GetInt( string key, int defVal )
        {
            try
            {
                string str;
                if (field.TryGetValue( key, out str ))
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
                if (field.TryGetValue( key, out str ))
                    return double.Parse( str,
                        NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            return defVal;
        }
    }
}
