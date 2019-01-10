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

		public static eSigIoMask FromIcd(this Protocol.Sigs.eSigIoMask extends)
		{
			switch (extends)
			{
				case Protocol.Sigs.eSigIoMask.Na:
					return eSigIoMask.NA;
				case Protocol.Sigs.eSigIoMask.FusionToProgram:
					return eSigIoMask.InputSigOnly;
				case Protocol.Sigs.eSigIoMask.ProgramToFusion:
					return eSigIoMask.OutputSigOnly;
				case Protocol.Sigs.eSigIoMask.BiDirectional:
					return eSigIoMask.InputOutputSig;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		public static Protocol.Sigs.eSigIoMask ToIcd(this eSigIoMask extends)
		{
			switch (extends)
			{
				case eSigIoMask.NA:
					return Protocol.Sigs.eSigIoMask.Na;
				case eSigIoMask.OutputSigOnly:
					return Protocol.Sigs.eSigIoMask.ProgramToFusion;
				case eSigIoMask.InputSigOnly:
					return Protocol.Sigs.eSigIoMask.FusionToProgram;
				case eSigIoMask.InputOutputSig:
					return Protocol.Sigs.eSigIoMask.BiDirectional;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}
	}
}