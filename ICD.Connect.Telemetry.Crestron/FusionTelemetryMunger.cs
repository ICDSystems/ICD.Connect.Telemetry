using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger
	{
		private const int SIG_OFFSET = 49;

		private static readonly IcdHashSet<IFusionSigMapping> s_Mappings =
			IcdDisplayFusionSigs.Sigs
			                    .Concat(IcdSwitcherFusionSigs.Sigs)
			                    .Concat(IcdSwitcherFusionSigs.InputOutputSigs)
			                    .Concat(IcdStandardFusionSigs.Sigs)
			                    .Concat(IcdDialingDeviceFusionSigs.Sigs)
			                    .Concat(IcdOccupancyFusionSigs.Sigs)
			                    .Concat(IcdDspFusionSigs.Sigs)
			                    .Concat(IcdControlSystemFusionSigs.Sigs)
			                    .Concat(IcdVolumeDeviceControlFusionSigs.Sigs)
			                    .ToIcdHashSet();

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

		#region Methods

		/// <summary>
		/// Adds the devices as assets to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="devices"></param>
		/// <returns></returns>
		public void BuildAssets(IEnumerable<IDevice> devices)
		{
			Clear();

			foreach (IDevice device in devices)
				BuildAssets(device);
		}

		/// <summary>
		/// Adds the device as assets to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public void BuildAssets(IDevice device)
		{
			// Create the sig bindings
			ITelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(device);

			// Add the assets to fusion
			if (device.Controls.GetControls<IOccupancySensorControl>().Any())
				GenerateOccupancySensorAsset(device);

			GenerateStaticAsset(device, nodes);
		}

		public static void RegisterMappingSet(Type type, IEnumerable<IFusionSigMapping> mappings)
		{
			s_Mappings.AddRange(mappings);
		}

		public void Clear()
		{
			m_BindingsSection.Enter();
			try
			{
				foreach (FusionTelemetryBinding binding in m_BindingsByAsset.Values.SelectMany(v => v))
					binding.Dispose();

				m_BindingsByAsset.Clear();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		public static IEnumerable<IFusionSigMapping> GetMappings()
		{
			return s_Mappings.ToList(s_Mappings.Count);
		}

		#endregion

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

		/// <summary>
		/// Adds a new occupancy sensor asset to the fusion room for the given device.
		/// </summary>
		/// <param name="device"></param>
		private void GenerateOccupancySensorAsset(IDevice device)
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

			PropertyTelemetryItem node =
				nodes.OfType<PropertyTelemetryItem>()
				     .FirstOrDefault(n => n.PropertyInfo.PropertyType == typeof(eOccupancyState));
			if (node == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, matching node not found.");

			IFusionSigMapping mapping = IcdOccupancyFusionSigs.OccupancyState;
			FusionTelemetryBinding binding = mapping.Bind(m_FusionRoom, node, occAssetInfo.Number, null);
			if (binding == null)
				return;

			AddBindingsToCollection(occAssetInfo.Number, new[] {binding});
		}

		/// <summary>
		/// Adds the bindings to the cache for the given asset id.
		/// </summary>
		/// <param name="staticAssetId"></param>
		/// <param name="bindings"></param>
		private void AddBindingsToCollection(uint staticAssetId, IEnumerable<FusionTelemetryBinding> bindings)
		{
			m_BindingsSection.Enter();

			try
			{
				m_BindingsByAsset.GetOrAddNew(staticAssetId, () => new IcdHashSet<FusionTelemetryBinding>())
				                 .AddRange(bindings);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Returns the next unused asset id for the fusion room.
		/// </summary>
		/// <returns></returns>
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

		[CanBeNull]
		private static IFusionSigMapping GetMapping(ITelemetryItem node)
		{
			return s_Mappings/*.Where(kvp => node.Parent.GetType().IsAssignableTo(kvp.Key))*/
				.FirstOrDefault(m => (node.Name == m.TelemetryGetName || node.Name == m.TelemetrySetName) &&
				                     (m.TelemetryProviderTypes == null ||
				                      node.Parent.GetType().GetAllTypes().Any(t => m.TelemetryProviderTypes.Contains(t))));
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
												 .Where(a => a.Mapping != null)
												 .Distinct(a => a.Mapping))
			{
				FusionTelemetryBinding output = nodeMappingPair.Mapping.Bind(m_FusionRoom, nodeMappingPair.Node, assetId, mappingUsage);
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
