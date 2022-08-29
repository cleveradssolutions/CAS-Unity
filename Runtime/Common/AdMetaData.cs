//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System.Text;

namespace CAS
{
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Impression-Level-Data" )]
    public abstract class AdMetaData
    {
        /// <summary>
        /// The Format Type of the impression.
        /// </summary>
        public readonly AdType type;

        /// <summary>
        /// The mediated network’s name that purchased the impression.
        /// </summary>
        public abstract AdNetwork network { get; }

        /// <summary>
        /// The Cost Per Mille estimated impressions of the ad in USD.
        /// <para>The value accuracy is returned in the <see cref="priceAccuracy"/> property.</para>
        /// </summary>
        public abstract double cpm { get; }

        /// <summary>
        /// Accuracy of the CPM value.
        /// </summary>
        public abstract PriceAccuracy priceAccuracy { get; }

        /// <summary>
        /// The creative id tied to the ad, if any. May be null.
        /// You can report creative issues to our Ad review team using this id.
        /// </summary>
        public abstract string creativeIdentifier { get; }

        /// <summary>
        /// Internal demand source name in CAS database.
        /// </summary>
        public abstract string identifier { get; }

        public AdMetaData( AdType type )
        {
            this.type = type;
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
    }
}
