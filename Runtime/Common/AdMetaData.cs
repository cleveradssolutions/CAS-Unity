using System;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// Wiki page: https://github.com/cleveradssolutions/CAS-Unity/wiki/Impression-Level-Data
    /// </summary>
    public struct AdMetaData
    {
        /// <summary>
        /// The Format Type for the impression.
        /// </summary>
        public readonly AdType type;
        /// <summary>
        /// Demand Source name is the name of the mediation-side entity that purchased the impression.
        /// </summary>
        public readonly AdNetwork network;
        /// <summary>
        /// The Cost Per Mille estimated impressions of the ad in USD.
        /// <para>The value accuracy is returned in the <see cref="priceAccuracy"/> property.</para>
        /// </summary>
        public readonly double cpm;
        /// <summary>
        /// Accuracy of the CPM value.
        /// </summary>
        public readonly PriceAccuracy priceAccuracy;

        internal AdMetaData( AdType type, AdNetwork network, double cpm, PriceAccuracy priceAccuracy )
        {
            this.type = type;
            this.network = network;
            this.cpm = cpm;
            this.priceAccuracy = priceAccuracy;
        }

        public override string ToString()
        {
            if (priceAccuracy == PriceAccuracy.Undisclosed)
                return "Impression of the " + type + " ads with undisclosed cost from " + network;
            string accuracy = priceAccuracy == PriceAccuracy.Floor ? "a floor " : "an average ";
            return "Impression of the " + type + " ads with " + accuracy + cpm + " CPM from " + network;
        }
    }
}
