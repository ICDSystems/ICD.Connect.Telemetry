using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;
using ICD.Connect.Telemetry.Providers.External;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger : IDisposable
	{
		private const int SIG_OFFSET = 49;

		private static readonly IcdHashSet<IFusionSigMapping> s_AssetMappings =
			IcdDisplayFusionSigs.Sigs
			                    .Concat(IcdSwitcherFusionSigs.AssetMappings)
			                    .Concat(IcdSwitcherFusionSigs.InputOutputAssetMappings)
			                    .Concat(IcdStandardFusionSigs.AssetMappings)
			                    .Concat(IcdDialingDeviceFusionSigs.AssetMappings)
			                    .Concat(IcdOccupancyFusionSigs.AssetMappings)
			                    .Concat(IcdDspFusionSigs.AssetMappings)
			                    .Concat(IcdControlSystemFusionSigs.AssetMappings)
			                    .Concat(IcdVolumeDeviceControlFusionSigs.AssetMappings)
								.ToIcdHashSet();

		private static ITelemetryService s_TelemetryService;

		private readonly Dictionary<string, uint> m_InstanceIdToAsset;
		private readonly Dictionary<uint, IcdHashSet<FusionTelemetryBinding>> m_BindingsByAsset;
		private readonly SafeCriticalSection m_BindingsSection;
		private readonly IFusionRoom m_FusionRoom;

		/// <summary>
		/// Gets the telemetry service.
		/// </summary>
		public static ITelemetryService TelemetryService
		{
			get { return s_TelemetryService ?? (s_TelemetryService = ServiceProvider.GetService<ITelemetryService>()); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionRoom"></param>
		public FusionTelemetryMunger([NotNull] IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			m_InstanceIdToAsset = new Dictionary<string, uint>();
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
				m_InstanceIdToAsset.Clear();
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
		/// <param name="mappings"></param>
		public static void RegisterMappingSet([NotNull] IEnumerable<IFusionSigMapping> mappings)
		{
			if (mappings == null)
				throw new ArgumentNullException("mappings");

			s_AssetMappings.AddRange(mappings);
		}

		/// <summary>
		/// Gets the registered mappings.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<IFusionSigMapping> GetMappings()
		{
			return s_AssetMappings.ToList(s_AssetMappings.Count);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the device as assets to fusion and builds the sigs from the telemetry.
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		private void BuildAssets([NotNull] IDevice device)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			// Ensure the Core telemetry has been built
			TelemetryService.InitializeCoreTelemetry();

			// Get the telemetry for this device
			TelemetryCollection nodes = TelemetryService.GetTelemetryForProvider(device);

			// Walk the telemetry nodes and generate bindings
			RangeMappingUsageTracker mappingUsage = new RangeMappingUsageTracker();
			BuildBindingsRecursive(device, nodes, mappingUsage);
		}

		/// <summary>
		/// Builds the bindings recursively for the given telemetry collection.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="nodes"></param>
		/// <param name="mappingUsage"></param>
		private void BuildBindingsRecursive([NotNull] IDevice device,
		                                    [NotNull] IEnumerable<ITelemetryNode> nodes,
		                                    [NotNull] RangeMappingUsageTracker mappingUsage)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			foreach (ITelemetryNode node in nodes)
			{
				TelemetryCollection collection = node as TelemetryCollection;
				if (collection != null)
					BuildBindingsRecursive(device, collection, mappingUsage);

				TelemetryLeaf leaf = node as TelemetryLeaf;
				if (leaf != null)
					BuildBinding(device, leaf, mappingUsage);
			}
		}

		/// <summary>
		/// Builds the telemetry binding for the given telemetry leaf.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="leaf"></param>
		/// <param name="mappingUsage"></param>
		private void BuildBinding([NotNull] IDevice device, [NotNull] TelemetryLeaf leaf, [NotNull] RangeMappingUsageTracker mappingUsage)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			if (leaf == null)
				throw new ArgumentNullException("leaf");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			IFusionSigMapping mapping = GetMapping(leaf);
			if (mapping == null)
				return;

			foreach (eAssetType assetType in mapping.FusionAssetTypes)
			{
				uint assetId = LazyLoadAsset(device, assetType);
				FusionTelemetryBinding binding = mapping.Bind(m_FusionRoom, leaf, assetId, mappingUsage);
				AddAssetBindingToCollection(assetId, binding);

				binding.Initialize();
			}
		}

		/// <summary>
		/// Gets the existing asset for the device or creates a new one.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="assetType"></param>
		/// <returns></returns>
		private uint LazyLoadAsset([NotNull] IDevice device, eAssetType assetType)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			string instanceId = m_FusionRoom.GetInstanceId(device, assetType);

			return m_InstanceIdToAsset.GetOrAddNew(instanceId, () => AddAsset(device, assetType));
		}

		/// <summary>
		/// Adds an asset for the given device to the fusion room.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="assetType"></param>
		/// <returns>Returns the asset id</returns>
		private uint AddAsset([NotNull] IDevice device, eAssetType assetType)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			uint assetId = m_FusionRoom.GetNextAssetId();
			string instanceId = m_FusionRoom.GetInstanceId(device, assetType);

			AssetInfo occAssetInfo =
				new AssetInfo(assetType,
				              assetId,
				              device.Name,
				              device.GetType().Name,
				              instanceId);

			m_FusionRoom.AddAsset(occAssetInfo);

			return assetId;
		}

		/// <summary>
		/// Adds the bindings to the cache for the given asset id.
		/// </summary>
		/// <param name="assetId"></param>
		/// <param name="binding"></param>
		private void AddAssetBindingToCollection(uint assetId, [NotNull] FusionTelemetryBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			m_BindingsSection.Enter();

			try
			{
				m_BindingsByAsset.GetOrAddNew(assetId, () => new IcdHashSet<FusionTelemetryBinding>())
				                 .Add(binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the mapping for the given telemetry leaf.
		/// </summary>
		/// <param name="leaf"></param>
		/// <returns></returns>
		[CanBeNull]
		private static IFusionSigMapping GetMapping([NotNull] TelemetryLeaf leaf)
		{
			if (leaf == null)
				throw new ArgumentNullException("leaf");

			ITelemetryProvider provider = leaf.Provider;

			// Instead of needing to add external providers to our mapping whitelists, lets just check by the external provider parent
			IExternalTelemetryProvider externalProvider = provider as IExternalTelemetryProvider;
			if (externalProvider != null)
				provider = externalProvider.Parent;

			return s_AssetMappings.FirstOrDefault(m => leaf.Name == m.TelemetryName && m.ValidateProvider(provider));
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

		/// <summary>
		/// Called when a sig changes from the service.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void FusionRoomOnFusionAssetSigUpdated(object sender, FusionAssetSigUpdatedArgs args)
		{
			IcdHashSet<FusionTelemetryBinding> bindingsForAsset;
			if (!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			IEnumerable<FusionTelemetryBinding> matches =
				bindingsForAsset.Where(b => b.Mapping.Sig == (args.Sig + SIG_OFFSET) &&
				                            b.Mapping.SigType == args.SigType);

			foreach (FusionTelemetryBinding bindingMatch in matches)
				bindingMatch.UpdateLocalNodeValueFromService();
		}

		/// <summary>
		/// Called when an assets power state changes from the service.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void FusionRoomOnFusionAssetPowerStateUpdated(object sender, FusionAssetPowerStateUpdatedArgs args)
		{
			IcdHashSet<FusionTelemetryBinding> bindingsForAsset;
			if (!m_BindingsByAsset.TryGetValue(args.AssetId, out bindingsForAsset))
				return;

			string telemetryName = args.Powered ? DeviceTelemetryNames.POWER_ON : DeviceTelemetryNames.POWER_OFF;
			FusionTelemetryBinding binding =
				bindingsForAsset.FirstOrDefault(b => b.Mapping.TelemetryName == telemetryName);

			if (binding != null)
				binding.Telemetry.Invoke();
		}

		#endregion
	}
}
