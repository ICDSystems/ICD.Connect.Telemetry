using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger : IDisposable
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
		public FusionTelemetryMunger([NotNull] IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			m_BindingsByAsset = new Dictionary<uint, IcdHashSet<FusionTelemetryBinding>>();
			m_BindingsSection = new SafeCriticalSection();

			m_FusionRoom = fusionRoom;
			Subscribe(m_FusionRoom);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			Unsubscribe(m_FusionRoom);

			Clear();
		}

		#region Methods

		/// <summary>
		/// Disposes and clears all of the generated bindings.
		/// </summary>
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

		/// <summary>
		/// Builds the bindings for the given room.
		/// </summary>
		/// <param name="room"></param>
		public void BuildRoom([NotNull] IRoom room)
		{
			throw new NotImplementedException();
		}

		/*
		private static void AddSigsToFusionRoom(IFusionRoom fusionRoom)
		{
			fusionRoom.AddSig(eSigType.Serial, FUSION_SIG_ROOM_PREFIX, "Room Prefix", eTelemetryIoMask.ProgramToService);
			fusionRoom.AddSig(eSigType.Serial, FUSION_SIG_ROOM_NUMBER, "Room Number", eTelemetryIoMask.ProgramToService);
			fusionRoom.AddSig(eSigType.Serial, FUSION_SIG_ROOMOS_VERSION, "RoomOS Version", eTelemetryIoMask.ProgramToService);
			fusionRoom.AddSig(eSigType.Serial, FUSION_SIG_ROOM_ATC_NUMBER, "Room ATC Number", eTelemetryIoMask.ProgramToService);
		}

		private void SendCurrentRoomValuesToFusion(IFusionRoom fusionRoom)
		{
			fusionRoom.SetSystemPower(m_Room != null && m_Room.SystemPower);
			fusionRoom.SetDisplayPower(m_Room != null && m_Room.DisplayPower);
			fusionRoom.UpdateSerialSig(FUSION_SIG_ROOM_PREFIX, m_Room != null ? m_Room.Prefix : string.Empty);
			fusionRoom.UpdateSerialSig(FUSION_SIG_ROOM_NUMBER, m_Room != null ? m_Room.Number : string.Empty);
			fusionRoom.UpdateSerialSig(FUSION_SIG_ROOMOS_VERSION, m_Room != null ? m_Room.InformationalVersion : string.Empty);
			fusionRoom.UpdateSerialSig(FUSION_SIG_ROOM_ATC_NUMBER, m_Room != null ? m_Room.PhoneNumber : string.Empty);
		}*/

				/*
		#region Room Callbacks

		private void Subscribe(IMetlifeRoom room)
		{
			if (room == null)
				return;

			room.OnDeviceUsageChanged += RoomOnDeviceUsageChanged;
			room.OnSystemPowerStateChanged += RoomOnSystemPowerStateChanged;
			room.OnDisplaysPowerStateChanged += RoomOnDisplaysPowerStateChanged;
			room.OnRoomPrefixChanged += RoomOnRoomPrefixChanged;
		}

		private void Unsubscribe(IMetlifeRoom room)
		{
			if (room == null)
				return;

			room.OnDeviceUsageChanged -= RoomOnDeviceUsageChanged;
			room.OnSystemPowerStateChanged -= RoomOnSystemPowerStateChanged;
			room.OnDisplaysPowerStateChanged -= RoomOnDisplaysPowerStateChanged;
			room.OnRoomPrefixChanged -= RoomOnRoomPrefixChanged;
		}

		private void RoomOnDisplaysPowerStateChanged(object sender, BoolEventArgs args)
		{
			m_FusionRoom.SetDisplayPower(args.Data);
		}

		private void RoomOnSystemPowerStateChanged(object sender, BoolEventArgs args)
		{
			m_FusionRoom.SetSystemPower(args.Data);
		}

		private void RoomOnDeviceUsageChanged(object sender, StringEventArgs args)
		{
			m_FusionRoom.SendDeviceUsage(args.Data);
		}

		private void RoomOnRoomPrefixChanged(object sender, StringEventArgs args)
		{
			m_FusionRoom.UpdateSerialSig(FUSION_SIG_ROOM_PREFIX, args.Data);
		}

		#endregion

		#region Fusion Room Callbacks

		private void Subscribe(IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.OnFusionSystemPowerChangeEvent += FusionRoomOnFusionSystemPowerChangeEvent;
			fusionRoom.OnFusionDisplayPowerChangeEvent += FusionRoomOnFusionDisplayPowerChangeEvent;
		}

		private void Unsubscribe(IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.OnFusionSystemPowerChangeEvent -= FusionRoomOnFusionSystemPowerChangeEvent;
			fusionRoom.OnFusionDisplayPowerChangeEvent -= FusionRoomOnFusionDisplayPowerChangeEvent;
		}

		private void FusionRoomOnFusionSystemPowerChangeEvent(object sender, BoolEventArgs args)
		{
			if (m_Room == null)
				return;

			// No System Power On Action
			if (!args.Data)
				m_Room.Shutdown();
		}

		private void FusionRoomOnFusionDisplayPowerChangeEvent(object sender, BoolEventArgs args)
		{
			if (m_Room == null)
				return;

			//No Display Power On Action
			if (!args.Data)
				m_Room.PowerOffDisplays();
		}

		#endregion
		 */

		/// <summary>
		/// Adds the devices as assets to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="devices"></param>
		/// <returns></returns>
		public void BuildAssets([NotNull] IEnumerable<IDevice> devices)
		{
			foreach (IDevice device in devices)
				BuildAssets(device);
		}

		/// <summary>
		/// Adds the given mappings to the mappings set.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="mappings"></param>
		public static void RegisterMappingSet([NotNull] Type type, [NotNull] IEnumerable<IFusionSigMapping> mappings)
		{
			s_Mappings.AddRange(mappings);
		}

		/// <summary>
		/// Gets the registered mappings.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<IFusionSigMapping> GetMappings()
		{
			return s_Mappings.ToList(s_Mappings.Count);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the device as assets to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		private void BuildAssets(IDevice device)
		{
			// Ensure the Core telemetry has been built
			ServiceProvider.GetService<ITelemetryService>().InitializeCoreTelemetry();

			// Create the sig bindings
			TelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(device);

			// Add the assets to fusion
			if (device.Controls.GetControls<IOccupancySensorControl>().Any())
				GenerateOccupancySensorAsset(device);

			GenerateStaticAsset(device, nodes);
		}

		private void GenerateStaticAsset(IDeviceBase device, TelemetryCollection nodes)
		{
			uint staticAssetId = m_FusionRoom.GetNextAssetId();
			AssetInfo staticAssetInfo = new AssetInfo(eAssetType.StaticAsset,
			                                          staticAssetId,
			                                          device.Name,
			                                          device.GetType().Name,
			                                          m_FusionRoom.GetInstanceId(device, eAssetType.StaticAsset));

			m_FusionRoom.AddAsset(staticAssetInfo);

			RangeMappingUsageTracker mappingUsage = new RangeMappingUsageTracker();
			IEnumerable<FusionTelemetryBinding> bindings = BuildBindingsRecursive(nodes, staticAssetId, mappingUsage);

			AddAssetBindingsToCollection(staticAssetId, bindings);
		}

		/// <summary>
		/// Adds a new occupancy sensor asset to the fusion room for the given device.
		/// </summary>
		/// <param name="device"></param>
		private void GenerateOccupancySensorAsset(IDevice device)
		{
			AssetInfo occAssetInfo = new AssetInfo(eAssetType.OccupancySensor,
			                                       m_FusionRoom.GetNextAssetId(),
			                                       device.Name,
			                                       device.GetType().Name,
			                                       m_FusionRoom.GetInstanceId(device, eAssetType.OccupancySensor));
			m_FusionRoom.AddAsset(occAssetInfo);

			IOccupancySensorControl control = device.Controls.GetControl<IOccupancySensorControl>();
			if(control == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, control not found.");

			TelemetryCollection nodes = ServiceProvider.GetService<ITelemetryService>().GetTelemetryForProvider(control);
			if (nodes == null || !nodes.Any())
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, nodes not found.");

			TelemetryLeaf node =
				nodes.OfType<TelemetryLeaf>()
				     .FirstOrDefault(n => n.PropertyInfo.PropertyType == typeof(eOccupancyState));
			if (node == null)
				throw new InvalidOperationException("Cannot Generate Occupancy Sensor Asset, matching node not found.");

			IFusionSigMapping mapping = IcdOccupancyFusionSigs.OccupancyState;
			FusionTelemetryBinding binding = mapping.Bind(m_FusionRoom, node, occAssetInfo.Number, null);
			if (binding == null)
				return;

			AddAssetBindingsToCollection(occAssetInfo.Number, binding.Yield());
		}

		/// <summary>
		/// Adds the bindings to the cache for the given asset id.
		/// </summary>
		/// <param name="staticAssetId"></param>
		/// <param name="bindings"></param>
		private void AddAssetBindingsToCollection(uint staticAssetId, IEnumerable<FusionTelemetryBinding> bindings)
		{
			m_BindingsSection.Enter();

			try
			{
				IcdHashSet<FusionTelemetryBinding> collection =
					m_BindingsByAsset.GetOrAddNew(staticAssetId, () => new IcdHashSet<FusionTelemetryBinding>());

				foreach (FusionTelemetryBinding binding in bindings.Where(collection.Add))
					binding.Initialize();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		[CanBeNull]
		private static IFusionSigMapping GetMapping(ITelemetryNode node)
		{
			return s_Mappings/*.Where(kvp => node.Provider.GetType().IsAssignableTo(kvp.Key))*/
				.FirstOrDefault(m => node.Name == m.TelemetryName &&
				                     (m.TelemetryProviderTypes == null ||
				                      node.Provider.GetType().GetAllTypes().Any(t => m.TelemetryProviderTypes.Contains(t))));
		}

		private IEnumerable<FusionTelemetryBinding> BuildBindingsRecursive(TelemetryCollection nodes,
		                                                                   uint assetId, RangeMappingUsageTracker mappingUsage)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			IEnumerable<TelemetryCollection> childCollections = nodes.OfType<TelemetryCollection>();
			foreach (TelemetryCollection collection in childCollections)
			{
				IEnumerable<FusionTelemetryBinding> children = BuildBindingsRecursive(collection, assetId, mappingUsage);
				foreach (FusionTelemetryBinding child in children)
					yield return child;
			}

			foreach (var nodeMappingPair in nodes.Where(n => !n.GetType().IsAssignableTo(typeof(TelemetryCollection)))
												 .Select(n => new {Node = n, Mapping = GetMapping(n)})
												 .Where(a => a.Mapping != null)
												 .Distinct(a => a.Mapping))
			{
				FusionTelemetryBinding output = nodeMappingPair.Mapping.Bind(m_FusionRoom, nodeMappingPair.Node, assetId, mappingUsage);
				if (output != null)
					yield return output;
			}
		}

		#endregion

		#region FusionRoom Callbacks

		/// <summary>
		/// Subscribe to the fusion room events.
		/// </summary>
		/// <param name="fusionRoom"></param>
		private void Subscribe(IFusionRoom fusionRoom)
		{
			fusionRoom.OnFusionAssetSigUpdated += FusionRoomOnFusionAssetSigUpdated;
			fusionRoom.OnFusionAssetPowerStateUpdated += FusionRoomOnFusionAssetPowerStateUpdated;
		}

		/// <summary>
		/// Unsubscribe from the fusion room events.
		/// </summary>
		/// <param name="fusionRoom"></param>
		private void Unsubscribe(IFusionRoom fusionRoom)
		{
			fusionRoom.OnFusionAssetSigUpdated -= FusionRoomOnFusionAssetSigUpdated;
			fusionRoom.OnFusionAssetPowerStateUpdated -= FusionRoomOnFusionAssetPowerStateUpdated;
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
					bindingsForAsset.FirstOrDefault(b => b.Mapping.TelemetryName == DeviceTelemetryNames.POWER_ON);

				if (binding != null)
					binding.Telemetry.Invoke();
			}
			else
			{
				FusionTelemetryBinding binding =
					bindingsForAsset.FirstOrDefault(b => b.Mapping.TelemetryName == DeviceTelemetryNames.POWER_OFF);

				if (binding != null)
					binding.Telemetry.Invoke();
			}
		}

		#endregion
	}
}
