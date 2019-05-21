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
			yield return ConsoleNodeGroup.IndexNodeMap("Telemetry",
			                                           ServiceProvider.GetService<ITelemetryService>()
			                                                          .GetTelemetryForProvider(instance)
			                                                          .OfType<IConsoleNodeBase>());
		} 
	}
}