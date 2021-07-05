namespace CAS
{
    public struct AdMetaData
    {
        public readonly AdType type;
        public readonly AdNetwork network;
        public readonly double cpm;
        public readonly PriceAccuracy priceAccuracy;

        internal AdMetaData( AdType type, AdNetwork network, double cpm, PriceAccuracy priceAccuracy )
        {
            this.type = type;
            this.network = network;
            this.cpm = cpm;
            this.priceAccuracy = priceAccuracy;
        }
    }
}
