using System;
using ICD.Common.Properties;

namespace ICD.Connect.Protocol.Sigs
{
	[PublicAPI][Flags]
	public enum eSigIoMask
	{
		[PublicAPI] Na = 0,
		[PublicAPI] FusionToProgram = 1,
		[PublicAPI] ProgramToFusion = 2,
		[PublicAPI] BiDirectional = FusionToProgram | ProgramToFusion
	}
}