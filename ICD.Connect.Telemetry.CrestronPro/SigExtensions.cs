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

		public static eSigIoMask FromIcd(this Crestron.eSigIoMask extends)
		{
			switch (extends)
			{
				case Crestron.eSigIoMask.Na:
					return eSigIoMask.NA;
				case Crestron.eSigIoMask.FusionToProgram:
					return eSigIoMask.OutputSigOnly;
				case Crestron.eSigIoMask.ProgramToFusion:
					return eSigIoMask.InputSigOnly;
				case Crestron.eSigIoMask.BiDirectional:
					return eSigIoMask.InputOutputSig;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		public static Crestron.eSigIoMask ToIcd(this eSigIoMask extends)
		{
			switch (extends)
			{
				case eSigIoMask.NA:
					return Crestron.eSigIoMask.Na;
				case eSigIoMask.OutputSigOnly:
					return Crestron.eSigIoMask.FusionToProgram;
				case eSigIoMask.InputSigOnly:
					return Crestron.eSigIoMask.ProgramToFusion;
				case eSigIoMask.InputOutputSig:
					return Crestron.eSigIoMask.BiDirectional;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}
	}
}
#endif