using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Devices;
using ICD.Connect.Displays.Devices;
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
		private static readonly Dictionary<Type, IEnumerable<IFusionSigMapping>> s_MappingsByType =
			new Dictionary<Type, IEnumerable<IFusionSigMapping>>
			{
				{typeof(IDisplayWithAudio), IcdDisplayWithAudioFusionSigs.Sigs},
				{typeof(IDisplay), IcdDisplayFusionSigs.Sigs},
				{typeof(IRouteSwitcherControl), IcdSwitcherFusionSigs.Sigs},
				{typeof(InputOutputPortBase), IcdSwitcherFusionSigs.InputOutputSigs},
				{typeof(IDevice), IcdStandardFusionSigs.Sigs},
				{typeof(IDialingDeviceExternalTelemetryProvider), IcdDialingDeviceFusionSigs.Sigs},
#if SIMPLSHARP
				{typeof(IControlSystemDevice), IcdControlSystemFusionSigs.Sigs}
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
		public void BuildAsset(IDevice device)
		{
			// Add the asset to fusion
			uint assetId = m_FusionRoom.GetAssetIds().Any() ? m_FusionRoom.GetAssetIds().Max() + 1 : 4;
			string instanceId = AssembleInstanceId(device);
			AssetInfo asset = new AssetInfo(eAssetType.StaticAsset, assetId, device.Name, device.GetType().Name, instanceId);
			m_FusionRoom.AddAsset(asset);

			// Create the sig bindings
			ITelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(device);

			RangeMappingUsageTracker mappingUsage = new RangeMappingUsageTracker();
			IEnumerable<FusionTelemetryBinding> bindings = BuildBindingsRecursive(nodes, assetId, mappingUsage);

			m_BindingsSection.Enter();

			try
			{
				if (m_BindingsByAsset.ContainsKey(assetId))
					m_BindingsByAsset[assetId].AddRange(bindings);
				else
					m_BindingsByAsset[assetId] = bindings.ToIcdHashSet();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		private string AssembleInstanceId(IDevice device)
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

		public void AddAssets(IEnumerable<IDevice> devices)
		{
			foreach (IDevice device in devices)
				BuildAsset(device);

			m_FusionRoom.RebuildRvi();
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

		public void RegisterMappingSet(Type type, IEnumerable<IFusionSigMapping> mappings)
		{
			if (s_MappingsByType.ContainsKey(type))
			{
				throw new ArgumentException("type",
				                            string.Format("Cannot Register Mapping Set for type {0}, type already registered.", type));
			}

			s_MappingsByType.Add(type, mappings);
		}

		private void FusionRoomOnFusionAssetSigUpdated(object sender, FusionAssetSigUpdatedArgs args)
		{
			IcdHashSet<FusionTelemetryBinding> bindingsForAsset;

			if (!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			foreach (
				FusionTelemetryBinding bindingMatch in
					bindingsForAsset.Where(b => b.Mapping.Sig == args.Sig && b.Mapping.SigType == args.SigType))
				bindingMatch.UpdateTelemetryNode();
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
