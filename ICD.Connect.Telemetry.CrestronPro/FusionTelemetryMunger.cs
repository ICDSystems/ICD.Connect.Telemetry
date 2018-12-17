using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Net;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Displays.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.CrestronPro.TelemetryAssets;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.CrestronPro
{
	public sealed class FusionTelemetryMunger
	{
		private ITelemetryService TelemetryService{get { return ServiceProvider.GetService<ITelemetryService>(); }}

		private static readonly Dictionary<Type, IFusionAssetFactory> s_Factories
			= new Dictionary<Type, IFusionAssetFactory>
			{
				{typeof(IDisplayWithAudio), new DisplayWithAudioFusionAssetFactory()},
				{typeof(IDisplay), new DisplayFusionAssetFactory()}
			};


		/// <summary>
		/// Adds the device as an asset to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="device"></param>
		/// <returns></returns>
		public uint AddAsset(IFusionRoom fusionRoom, IDevice device)
		{
			// TODO - Validation
			IFusionAssetFactory factory = s_Factories.First(kvp => device.GetType().IsAssignableTo(kvp.Key)).Value;

			// First add the asset
			uint assetId = fusionRoom.GetAssetIds().Max() + 1;
			fusionRoom.AddAsset(eAssetType.StaticAsset, assetId, device.Name, device.GetType().Name, Guid.NewGuid().ToString());
		
			// Now add the sigs

			List<FusionTelemetryBinding> bindings = new List<FusionTelemetryBinding>();
			foreach (var mapping in factory.Mappings)
			{
				TelemetryService.GetTelemetryForProvider(device, mapping.TelemetryGetName);
			}


			fusionRoom.AddSig(assetId, eSigType.Digital, 1, "name", eSigIoMask.ProgramToFusion);

			return assetId;
		}

		public void AddAssets(IFusionRoom room, IEnumerable<IDevice> devices)
		{
			foreach (var device in devices)
				AddAsset(room, device);
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}
	}

	public class FusionTelemetryBinding
	{
		public ITelemetryItem Telemetry { get; private set; }
		public FusionSigMapping Mapping { get; private set; }

		public FusionTelemetryBinding(ITelemetryItem telemetry, FusionSigMapping mapping)
		{
			Telemetry = telemetry;
			Mapping = mapping;
		}
	}
}