using ICD.Common.Properties;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class RoomFusionSigMapping : AbstractFusionSigMapping
	{
		/// <summary>
		/// Creates the telemetry binding for the given provider.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public FusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom,
		                                   [NotNull] TelemetryLeaf leaf,
		                                   [NotNull] MappingUsageTracker mappingUsage)
		{
			throw new System.NotImplementedException();
		}
	}
}
