using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Displays.Devices;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.SigMappings;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger
	{
		private static readonly Dictionary<Type, IEnumerable<FusionSigMapping>> s_MappingsByType
			= new Dictionary<Type, IEnumerable<FusionSigMapping>>
			{
				{typeof(IDisplayWithAudio), IcdDisplayWithAudioFusionSigs.Sigs},
				{typeof(IDisplay), IcdDisplayFusionSigs.Sigs},
				{typeof(IDevice), IcdStandardFusionSigs.Sigs}
			};

		private readonly IFusionRoom m_FusionRoom;
		private readonly Dictionary<uint, List<FusionTelemetryBinding>> m_BindingsByAsset;
		private readonly SafeCriticalSection m_BindingsSection;

		public FusionTelemetryMunger(IFusionRoom fusionRoom)
		{
			m_FusionRoom = fusionRoom;
			m_BindingsByAsset = new Dictionary<uint, List<FusionTelemetryBinding>>();
			m_FusionRoom.OnFusionAssetSigUpdated += FusionRoomOnFusionAssetSigUpdated;
			m_BindingsSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Adds the device as an asset to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public void AddAsset(IDevice device)
		{
			IEnumerable<FusionSigMapping> mappings =
				s_MappingsByType.Where(kvp => device.GetType().IsAssignableTo(kvp.Key))
						   .SelectMany(kvp => kvp.Value);

			// First add the asset
			uint assetId = m_FusionRoom.GetAssetIds().Any() ? m_FusionRoom.GetAssetIds().Max() + 1 : 4;
			m_FusionRoom.AddAsset(eAssetType.StaticAsset, assetId, device.Name, device.GetType().Name, Guid.NewGuid().ToString());
		
			// Now add the sigs
			List<FusionTelemetryBinding> bindings = new List<FusionTelemetryBinding>();
			foreach (FusionSigMapping mapping in mappings)
			{
				FusionTelemetryBinding binding = FusionTelemetryBinding.Bind(m_FusionRoom, mapping, assetId);
				bindings.Add(binding);
			}
			
			m_BindingsSection.Enter();
			try
			{
				if (m_BindingsByAsset.ContainsKey(assetId))
					m_BindingsByAsset[assetId].AddRange(bindings);
				else
					m_BindingsByAsset[assetId] = bindings;
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		public void AddAssets(IEnumerable<IDevice> devices)
		{
			foreach (IDevice device in devices)
				AddAsset(device);
		}

		public void Clear()
		{
			m_BindingsSection.Enter();
			try
			{
				foreach (var kvp in m_BindingsByAsset)
				{
					kvp.Value.Clear();
				}
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

        private void FusionRoomOnFusionAssetSigUpdated(object sender, FusionAssetSigUpdatedArgs args)
        {
	        List<FusionTelemetryBinding> bindingsForAsset;
			
			if(!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			foreach(var bindingMatch in bindingsForAsset.Where(b=>b.Mapping.Sig == args.Sig && b.Mapping.SigType == args.SigType))
			{
				bindingMatch.UpdateTelemetryNode();
			}
        }
	}
}