using System;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Crestron.Bindings;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class RoomFusionSigMapping : AbstractFusionSigMapping
	{
		/// <summary>
		/// Action for sending a new value to a reserved sig on the asset.
		/// </summary>
		public Action<IFusionRoom, object> SendReservedSig { get; set; }

		/// <summary>
		/// Creates the telemetry binding for the given provider.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public RoomFusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom,
		                                       [NotNull] TelemetryLeaf leaf,
		                                       [NotNull] MappingUsageTracker mappingUsage)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (leaf == null)
				throw new ArgumentNullException("leaf");

			if (mappingUsage == null)
				throw new ArgumentNullException("mappingUsage");

			string name = string.Format(FusionSigName, mappingUsage.GetCurrentOffset(this) + 1);
			ushort sig = mappingUsage.GetNextSig(this);

			RoomFusionSigMapping offsetMapping = new RoomFusionSigMapping
			{
				FusionSigName = name,
				Sig = sig,
				SigType = SigType,
				TelemetryName = TelemetryName,
				TelemetryProviderTypes = TelemetryProviderTypes,
				SendReservedSig = SendReservedSig
			};

			return RoomFusionTelemetryBinding.Bind(fusionRoom, leaf, offsetMapping);
		}
	}
}
