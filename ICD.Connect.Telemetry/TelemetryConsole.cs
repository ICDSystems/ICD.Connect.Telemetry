using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry
{
	public static class TelemetryConsole
	{
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(ITelemetryProvider instance)
		{
			ITelemetryService service = ServiceProvider.TryGetService<ITelemetryService>();
			if (service == null)
				yield break;

			yield return ConsoleNodeGroup.IndexNodeMap("Telemetry",
			                                           service.GetTelemetryForProvider(instance)
			                                                  .OfType<IConsoleNodeBase>());
		} 
	}
}