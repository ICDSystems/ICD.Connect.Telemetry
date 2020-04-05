#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro;

namespace ICD.Connect.Telemetry.CrestronPro
{
	public static class SigExtensions
	{
		public static eSigType FromIcd(this Protocol.Sigs.eSigType extends)
		{
			switch (extends)
			{
				case Protocol.Sigs.eSigType.Na:
					return eSigType.NA;
				case Protocol.Sigs.eSigType.Digital:
					return eSigType.Bool;
				case Protocol.Sigs.eSigType.Analog:
					return eSigType.UShort;
				case Protocol.Sigs.eSigType.Serial:
					return eSigType.String;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		public static Protocol.Sigs.eSigType ToIcd(this eSigType extends)
		{
			switch (extends)
			{
				case eSigType.NA:
					return Protocol.Sigs.eSigType.Na;
				case eSigType.Bool:
					return Protocol.Sigs.eSigType.Digital;
				case eSigType.UShort:
					return Protocol.Sigs.eSigType.Analog;
				case eSigType.String:
					return Protocol.Sigs.eSigType.Serial;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		public static eSigIoMask FromIcd(this eTelemetryIoMask extends)
		{
			switch (extends)
			{
				case eTelemetryIoMask.Na:
					return eSigIoMask.NA;
				case eTelemetryIoMask.ServiceToProgram:
					return eSigIoMask.OutputSigOnly;
				case eTelemetryIoMask.ProgramToService:
					return eSigIoMask.InputSigOnly;
				case eTelemetryIoMask.BiDirectional:
					return eSigIoMask.InputOutputSig;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		public static eTelemetryIoMask ToIcd(this eSigIoMask extends)
		{
			switch (extends)
			{
				case eSigIoMask.NA:
					return eTelemetryIoMask.Na;
				case eSigIoMask.OutputSigOnly:
					return eTelemetryIoMask.ServiceToProgram;
				case eSigIoMask.InputSigOnly:
					return eTelemetryIoMask.ProgramToService;
				case eSigIoMask.InputOutputSig:
					return eTelemetryIoMask.BiDirectional;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}
	}
}
#endif