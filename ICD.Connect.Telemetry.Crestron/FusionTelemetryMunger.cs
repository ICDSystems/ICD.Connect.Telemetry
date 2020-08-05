using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Telemetry;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Crestron.SigMappings.Assets;
using ICD.Connect.Telemetry.Crestron.SigMappings.Rooms;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;
using ICD.Connect.Telemetry.Providers.External;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryMunger : IDisposable
	{
		private const int SIG_OFFSET = 49;

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			DisplayFusionSigs.AssetMappings
			                    .Concat(SwitcherFusionSigs.AssetMappings)
			                    .Concat(SwitcherFusionSigs.InputOutputAssetMappings)
			                    .Concat(StandardFusionSigs.AssetMappings)
			                    .Concat(DialingDeviceFusionSigs.AssetMappings)
			                    .Concat(OccupancyFusionSigs.AssetMappings)
			                    .Concat(DspFusionSigs.AssetMappings)
			                    .Concat(ControlSystemFusionSigs.AssetMappings)
			                    .Concat(VolumeDeviceControlFusionSigs.AssetMappings)
								.ToIcdHashSet();

		private static readonly IcdHashSet<RoomFusionSigMapping> s_RoomMappings =
			RoomFusionSigs.RoomMappings
			              .Concat(CommercialRoomFusionSigs.RoomMappings)
			              .ToIcdHashSet();

		private static ITelemetryService s_TelemetryService;

		private readonly IcdHashSet<FusionTelemetryBinding> m_RoomBindings; 
		private readonly Dictionary<string, uint> m_InstanceIdToAsset;
		private readonly Dictionary<uint, IcdHashSet<FusionTelemetryBinding>> m_BindingsByAsset;
		private readonly SafeCriticalSection m_BindingsSection;
		private readonly IFusionRoom m_FusionRoom;

		#region Properties

		/// <summary>
		/// Gets the telemetry service.
		/// </summary>
		private static ITelemetryService TelemetryService
		{
			get { return s_TelemetryService ?? (s_TelemetryService = ServiceProvider.GetService<ITelemetryService>()); }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionRoom"></param>
		public FusionTelemetryMunger([NotNull] IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			m_RoomBindings = new IcdHashSet<FusionTelemetryBinding>();
			m_InstanceIdToAsset = new Dictionary<string, uint>();
			m_BindingsByAsset = new Dictionary<uint, IcdHashSet<FusionTelemetryBinding>>();
			m_BindingsSection = new SafeCriticalSection();

			m_FusionRoom = fusionRoom;
			Subscribe(m_FusionRoom);
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			Unsubscribe(m_FusionRoom);

			Clear();
		}

		/// <summary>
		/// Disposes and clears all of the generated bindings.
		/// </summary>
		public void Clear()
		{
			m_BindingsSection.Enter();

			try
			{
				foreach (FusionTelemetryBinding binding in m_RoomBindings)
					binding.Dispose();

				foreach (FusionTelemetryBinding binding in m_BindingsByAsset.Values.SelectMany(v => v))
					binding.Dispose();

				m_RoomBindings.Clear();
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
			if (room == null)
				throw new ArgumentNullException("room");

			// Ensure the Core telemetry has been built
			TelemetryService.InitializeCoreTelemetry();

			// Get the telemetry for this room
			TelemetryCollection nodes = TelemetryService.GetTelemetryForProvider(room);

			// Walk the telemetry nodes and generate bindings
			MappingUsageTracker mappingUsage = new MappingUsageTracker();
			BuildRoomBindingsRecursive(nodes, mappingUsage);
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
		/// Adds the given mappings to the asset mappings.
		/// </summary>
		/// <param name="mappings"></param>
		[PublicAPI]
		public static void RegisterAssetMappings([NotNull] IEnumerable<AssetFusionSigMapping> mappings)
		{
			if (mappings == null)
				throw new ArgumentNullException("mappings");

			s_AssetMappings.AddRange(mappings);
		}

		/// <summary>
		/// Adds the given mappings to the room mappings.
		/// </summary>
		/// <param name="mappings"></param>
		[PublicAPI]
		public static void RegisterRoomMappings([NotNull] IEnumerable<RoomFusionSigMapping> mappings)
		{
			if (mappings == null)
				throw new ArgumentNullException("mappings");

			s_RoomMappings.AddRange(mappings);
		}

		/// <summary>
		/// Gets the registered asset mappings.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<AssetFusionSigMapping> GetAssetMappings()
		{
			return s_AssetMappings.ToList(s_AssetMappings.Count);
		}

		/// <summary>
		/// Gets the registered room mappings.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<RoomFusionSigMapping> GetRoomMappings()
		{
			return s_RoomMappings.ToList(s_RoomMappings.Count);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the asset mapping for the given telemetry leaf.
		/// </summary>
		/// <param name="leaf"></param>
		/// <param name="mappings"></param>
		/// <returns></returns>
		[CanBeNull]
		private static T GetMapping<T>([NotNull] TelemetryLeaf leaf, [NotNull] IEnumerable<T> mappings)
			where T : IFusionSigMapping
		{
			if (leaf == null)
				throw new ArgumentNullException("leaf");

			if (mappings == null)
				throw new ArgumentNullException("mappings");

			ITelemetryProvider provider = leaf.Provider;

			// Instead of needing to add external providers to our mapping whitelists, lets just check by the external provider parent
			IExternalTelemetryProvider externalProvider = provider as IExternalTelemetryProvider;
			if (externalProvider != null)
			{
				provider = externalProvider.Parent;
				if (provider == null)
					throw new ArgumentException("External telemetry provider has not been initialized", "leaf");
			}

			return mappings.FirstOrDefault(m => leaf.Name == m.TelemetryName && m.ValidateProvider(provider));
		}

		#endregion

		#region Assets

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
			MappingUsageTracker mappingUsage = new MappingUsageTracker();
			BuildAssetBindingsRecursive(device, nodes, mappingUsage);
		}

		/// <summary>
		/// Builds the bindings recursively for the given telemetry collection.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="nodes"></param>
		/// <param name="mappingUsage"></param>
		private void BuildAssetBindingsRecursive([NotNull] IDevice device,
		                                         [NotNull] IEnumerable<ITelemetryNode> nodes,
		                                         [NotNull] MappingUsageTracker mappingUsage)
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
					BuildAssetBindingsRecursive(device, collection, mappingUsage);

				TelemetryLeaf leaf = node as TelemetryLeaf;
				if (leaf != null)
					BuildAssetBinding(device, leaf, mappingUsage);
			}
		}

		/// <summary>
		/// Builds the telemetry binding for the given telemetry leaf.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="leaf"></param>
		/// <param name="mappingUsage"></param>
		private void BuildAssetBinding([NotNull] IDevice device, [NotNull] TelemetryLeaf leaf,
		                               [NotNull] MappingUsageTracker mappingUsage)
		{
			if (device == null)
				throw new ArgumentNullException("device");

			if (leaf == null)
				throw new ArgumentNullException("leaf");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			AssetFusionSigMapping mapping = GetAssetMapping(leaf);
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
		/// Gets the asset mapping for the given telemetry leaf.
		/// </summary>
		/// <param name="leaf"></param>
		/// <returns></returns>
		[CanBeNull]
		private static AssetFusionSigMapping GetAssetMapping([NotNull] TelemetryLeaf leaf)
		{
			if (leaf == null)
				throw new ArgumentNullException("leaf");

			return GetMapping(leaf, s_AssetMappings);
		}

		#endregion

		#region Rooms

		/// <summary>
		/// Builds the bindings recursively for the given telemetry collection.
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="mappingUsage"></param>
		private void BuildRoomBindingsRecursive([NotNull] IEnumerable<ITelemetryNode> nodes,
		                                        [NotNull] MappingUsageTracker mappingUsage)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			foreach (ITelemetryNode node in nodes)
			{
				TelemetryCollection collection = node as TelemetryCollection;
				if (collection != null)
					BuildRoomBindingsRecursive(collection, mappingUsage);

				TelemetryLeaf leaf = node as TelemetryLeaf;
				if (leaf != null)
					BuildRoomBinding(leaf, mappingUsage);
			}
		}

		/// <summary>
		/// Builds the telemetry binding for the given telemetry leaf.
		/// </summary>
		/// <param name="leaf"></param>
		/// <param name="mappingUsage"></param>
		private void BuildRoomBinding([NotNull] TelemetryLeaf leaf,
		                              [NotNull] MappingUsageTracker mappingUsage)
		{
			if (leaf == null)
				throw new ArgumentNullException("leaf");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			RoomFusionSigMapping mapping = GetRoomMapping(leaf);
			if (mapping == null)
				return;

			FusionTelemetryBinding binding = mapping.Bind(m_FusionRoom, leaf, mappingUsage);
			AddRoomBindingToCollection(binding);

			binding.Initialize();
		}

		/// <summary>
		/// Adds the bindings to the cache.
		/// </summary>
		/// <param name="binding"></param>
		private void AddRoomBindingToCollection([NotNull] FusionTelemetryBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			m_BindingsSection.Execute(() => m_RoomBindings.Add(binding));
		}

		/// <summary>
		/// Gets the room mapping for the given telemetry leaf.
		/// </summary>
		/// <param name="leaf"></param>
		/// <returns></returns>
		[CanBeNull]
		private static RoomFusionSigMapping GetRoomMapping(TelemetryLeaf leaf)
		{
			if (leaf == null)
				throw new ArgumentNullException("leaf");

			return GetMapping(leaf, s_RoomMappings);
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
