using System;
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
{
	[PublicAPI]
	[Flags]
	public enum eTelemetryIoMask
	{
		[PublicAPI]
		Na = 0,
		[PublicAPI]
		ServiceToProgram = 1,
		[PublicAPI]
		ProgramToService = 2,
		[PublicAPI]
		BiDirectional = ServiceToProgram | ProgramToService
	}
}