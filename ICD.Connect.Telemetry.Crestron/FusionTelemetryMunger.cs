using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Audio.Biamp;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Devices;
using ICD.Connect.Displays.Devices;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Routing.Controls;
#if SIMPLSHARP
using ICD.Connect.Routing.CrestronPro.ControlSystem;
#endif
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger
	{
		private const int SIG_OFFSET = 49;

		private static readonly Dictionary<Type, IcdHashSet<IFusionSigMapping>> s_MappingsByType =
			new Dictionary<Type, IcdHashSet<IFusionSigMapping>>
			{
				{typeof(IDisplayWithAudio), IcdDisplayWithAudioFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(IDisplay), IcdDisplayFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(IRouteSwitcherControl), IcdSwitcherFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(InputOutputPortBase), IcdSwitcherFusionSigs.InputOutputSigs.ToIcdHashSet()},
				{typeof(IDeviceBase), IcdStandardFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(IDialingDeviceExternalTelemetryProvider), IcdDialingDeviceFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(IOccupancySensorControl), IcdOccupancyFusionSigs.Sigs.ToIcdHashSet()},
				{typeof(BiampTesiraDevice), IcdDspFusionSigs.Sigs.ToIcdHashSet()},
#if SIMPLSHARP
				{typeof(IControlSystemDevice), IcdControlSystemFusionSigs.Sigs.ToIcdHashSet()}
#endif
			};

		private readonly Dictionary<uint, IcdHashSet<FusionTelemetryBinding>> m_BindingsByAsset;
		private readonly SafeCriticalSection m_BindingsSection;
		private readonly IFusionRoom m_FusionRoom;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionRoom"></param>
		public FusionTelemetryMunger(IFusionRoom fusionRoom)
		{
			m_BindingsByAsset = new Dictionary<uint, IcdHashSet<FusionTelemetryBinding>>();
			m_BindingsSection = new SafeCriticalSection();

			m_FusionRoom = fusionRoom;

			m_FusionRoom.OnFusionAssetSigUpdated += FusionRoomOnFusionAssetSigUpdated;
			m_FusionRoom.OnFusionAssetPowerStateUpdated += FusionRoomOnFusionAssetPowerStateUpdated;
		}

		/// <summary>
		/// Adds the device as an asset to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public void BuildAssets(IDeviceBase device)
		{
			// Create the sig bindings
			ITelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(device);

			// Add the assets to fusion
			if (device.Controls.GetControls<IOccupancySensorControl>().Any())
			{
				GenerateOccupancySensorAsset(device);
			}

			GenerateStaticAsset(device, nodes);
		}

		private void GenerateStaticAsset(IDeviceBase device, ITelemetryCollection nodes)
		{
			uint staticAssetId = GetNextAssetId();
			AssetInfo staticAssetInfo = new AssetInfo(eAssetType.StaticAsset,
			                                          staticAssetId,
			                                          device.Name,
			                                          device.GetType().Name,
			                                          AssembleInstanceId(device, eAssetType.StaticAsset));

			m_FusionRoom.AddAsset(staticAssetInfo);

			RangeMappingUsageTracker mappingUsage = new RangeMappingUsageTracker();
			IEnumerable<FusionTelemetryBinding> bindings = BuildBindingsRecursive(nodes, staticAssetId, mappingUsage);

			AddBindingsToCollection(staticAssetId, bindings);
		}

		private void GenerateOccupancySensorAsset(IDeviceBase device)
		{
			AssetInfo occAssetInfo = new AssetInfo(eAssetType.OccupancySensor,
			                                       GetNextAssetId(),
			                                       device.Name,
			                                       device.GetType().Name,
			                                       AssembleInstanceId(device, eAssetType.OccupancySensor));
			m_FusionRoom.AddAsset(occAssetInfo);

			IOccupancySensorControl control = device.Controls.GetControl<IOccupancySensorControl>();
			if(control == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, control not found.");

			ITelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(control);
			if (nodes == null || !nodes.Any())
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, nodes not found.");

			DynamicTelemetryNodeItem<eOccupancyState> node = nodes.OfType<DynamicTelemetryNodeItem<eOccupancyState>>().First();
			if (node == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, matching node not found.");

			IFusionSigMapping mapping =
				s_MappingsByType[typeof(IOccupancySensorControl)].First(m =>
				                                                        m.TelemetryGetName ==
				                                                        OccupancyTelemetryNames.OCCUPANCY_STATE);

			if (mapping == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, mapping not found.");

			FusionTelemetryBinding binding = Bind(node, mapping, occAssetInfo.Number, null);
			if (binding == null)
				return;

			AddBindingsToCollection(occAssetInfo.Number, new[] {binding});
		}

		private void AddBindingsToCollection(uint staticAssetId, IEnumerable<FusionTelemetryBinding> bindings)
		{
			m_BindingsSection.Enter();

			try
			{
				if (m_BindingsByAsset.ContainsKey(staticAssetId))
					m_BindingsByAsset[staticAssetId].AddRange(bindings);
				else
					m_BindingsByAsset[staticAssetId] = bindings.ToIcdHashSet();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		private uint GetNextAssetId()
		{
			return m_FusionRoom.GetAssetIds().Any() ? m_FusionRoom.GetAssetIds().Max() + 1 : 4;
		}

		private string AssembleInstanceId(IDeviceBase device, eAssetType staticAsset)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			int stableHash;

			unchecked
			{
				stableHash = 17;
				stableHash = stableHash * 23 + device.Id;
				stableHash = stableHash * 23 + device.GetType().Name.GetStableHashCode();
				stableHash = stableHash * 23 + m_FusionRoom.RoomId.GetStableHashCode();
				stableHash = stableHash * 23 + (int)staticAsset;
			}

			Guid seeded = GuidUtils.GenerateSeeded(stableHash);
			return seeded.ToString();
		}

		private IFusionSigMapping GetMapping(ITelemetryItem node)
		{
			return s_MappingsByType/*.Where(kvp => node.Parent.GetType().IsAssignableTo(kvp.Key))*/
										.SelectMany(kvp => kvp.Value)
										.FirstOrDefault(m => node.Name == m.TelemetryGetName || node.Name == m.TelemetrySetName);
		}

		private IEnumerable<FusionTelemetryBinding> BuildBindingsRecursive(ITelemetryCollection nodes,
		                                                                   uint assetId, RangeMappingUsageTracker mappingUsage)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			IEnumerable<ITelemetryCollection> childCollections = nodes.OfType<ITelemetryCollection>();
			foreach (ITelemetryCollection collection in childCollections)
			{
				IEnumerable<FusionTelemetryBinding> children = BuildBindingsRecursive(collection, assetId, mappingUsage);
				foreach (FusionTelemetryBinding child in children)
					yield return child;
			}


			foreach (var nodeMappingPair in nodes.Where(n => !n.GetType().IsAssignableTo(typeof(ITelemetryCollection)))
												 .Select(n => new {Node = n, Mapping = GetMapping(n)})
												 .Distinct(a => a.Mapping))
			{
				FusionTelemetryBinding output = Bind(nodeMappingPair.Node, nodeMappingPair.Mapping, assetId, mappingUsage);
				if (output != null)
					yield return output;
			}
		}

		[CanBeNull]
		private FusionTelemetryBinding Bind(ITelemetryItem node, IFusionSigMapping mapping, uint assetId, RangeMappingUsageTracker mappingUsage)
		{
			FusionSigMapping singleMapping = mapping as FusionSigMapping;
			if (singleMapping != null)
			{
				FusionTelemetryBinding binding = FusionTelemetryBinding.Bind(m_FusionRoom, node.Parent, singleMapping, assetId);
				return binding;
			}

			FusionSigMultiMapping multiMapping = mapping as FusionSigMultiMapping;
			if (multiMapping != null)
			{
				FusionSigMapping tempMapping = new FusionSigMapping
				{
					FusionSigName = string.Format(multiMapping.FusionSigName, mappingUsage.GetCurrentOffset(multiMapping) + 1),
					Sig = mappingUsage.GetNextSig(multiMapping),
					SigType = multiMapping.SigType,
					TelemetryGetName = multiMapping.TelemetryGetName,
					TelemetrySetName = multiMapping.TelemetrySetName
				};
				FusionTelemetryBinding binding = FusionTelemetryBinding.Bind(m_FusionRoom, node.Parent, tempMapping, assetId);
				return binding;
			}

			return null;
		}

		public void AddAssets(IEnumerable<IDeviceBase> devices)
		{
			foreach (IDeviceBase device in devices)
				BuildAssets(device);
		}

		public void Clear()
		{
			m_BindingsSection.Enter();
			try
			{
				foreach (KeyValuePair<uint, IcdHashSet<FusionTelemetryBinding>> kvp in m_BindingsByAsset)
					kvp.Value.Clear();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		public static void RegisterMappingSet(Type type, IEnumerable<IFusionSigMapping> mappings)
		{
			IcdHashSet<IFusionSigMapping> stored;
			if (!s_MappingsByType.TryGetValue(type, out stored))
			{
				stored = new IcdHashSet<IFusionSigMapping>();
				s_MappingsByType.Add(type, stored);
			}

			stored.AddRange(mappings);
		}

		private void FusionRoomOnFusionAssetSigUpdated(object sender, FusionAssetSigUpdatedArgs args)
		{
			IcdHashSet<FusionTelemetryBinding> bindingsForAsset;

			if (!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			foreach (
				FusionTelemetryBinding bindingMatch in
					bindingsForAsset.Where(b => b.Mapping.Sig == (args.Sig + SIG_OFFSET) && b.Mapping.SigType == args.SigType))
				bindingMatch.UpdateLocalNodeValueFromService();
		}

		private void FusionRoomOnFusionAssetPowerStateUpdated(object sender, FusionAssetPowerStateUpdatedArgs args)
		{
			IcdHashSet<FusionTelemetryBinding> bindingsForAsset;

			if (!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			if (args.Powered)
			{
				FusionTelemetryBinding binding =
					bindingsForAsset.FirstOrDefault(b => b.Mapping.TelemetrySetName == DeviceTelemetryNames.POWER_ON);

				if (binding != null)
					binding.SetTelemetry.Invoke();
			}
			else
			{
				FusionTelemetryBinding binding =
					bindingsForAsset.FirstOrDefault(b => b.Mapping.TelemetrySetName == DeviceTelemetryNames.POWER_OFF);

				if (binding != null)
					binding.SetTelemetry.Invoke();
			}
		}
	}
}
