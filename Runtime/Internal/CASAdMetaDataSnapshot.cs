using System;
using System.Collections.Generic;
using CAS;
using UnityEngine;

namespace CAS
{
	internal sealed class CASAdMetaDataSnapshot : AdMetaData
	{
		private readonly AdNetwork _network;
		private readonly double _revenue;
		private readonly double _cpm;
		private readonly PriceAccuracy _priceAccuracy;
		private readonly string _creativeIdentifier;
		private readonly string _sourceUnitId;
		private readonly int _impressionDepth;
		private readonly double _lifetimeRevenue;

		internal CASAdMetaDataSnapshot(AdMetaData source)
			: base(source.type)
		{
			_network = source.network;
			_revenue = source.revenue;
			_cpm = source.cpm;
			_priceAccuracy = source.priceAccuracy;
			_creativeIdentifier = source.creativeIdentifier;
			_sourceUnitId = source.sourceUnitId;
			_impressionDepth = source.impressionDepth;
			_lifetimeRevenue = source.lifetimeRevenue;
		}

		public override AdNetwork network
		{
			get { return _network; }
		}

		public override double revenue
		{
			get { return _revenue; }
		}

		public override double cpm
		{
			get { return _cpm; }
		}

		public override PriceAccuracy priceAccuracy
		{
			get { return _priceAccuracy; }
		}

		public override string creativeIdentifier
		{
			get { return _creativeIdentifier; }
		}

		public override string sourceUnitId
		{
			get { return _sourceUnitId; }
		}

		public override int impressionDepth
		{
			get { return _impressionDepth; }
		}

		public override double lifetimeRevenue
		{
			get { return _lifetimeRevenue; }
		}
	}
}
