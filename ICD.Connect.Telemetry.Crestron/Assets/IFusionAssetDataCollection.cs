﻿using System.Collections.Generic;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionAssetDataCollection : IEnumerable<FusionAssetData>
	{
		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		FusionAssetData this[uint id] { get; }
	}
}
