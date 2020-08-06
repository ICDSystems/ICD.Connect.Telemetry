using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class AbstractFusionSigMapping : AbstractTelemetryMappingBase, IFusionSigMapping
	{
		public uint Sig { get; set; }

		public ushort Range { get; set; }

		public string FusionSigName { get; set; }

		/// <summary>
		/// Whitelist for the telemetry provider types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> TelemetryProviderTypes { get; set; }

		public eSigType SigType { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractFusionSigMapping()
		{
			Range = 1;
		}

		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this)
				.AppendProperty("TelemetryName", TelemetryName)
				.AppendProperty("FusionSigName", FusionSigName);

			if (Sig != 0)
			{
				builder.AppendProperty("SigType", SigType);
				builder.AppendProperty("Sig", Sig);
				builder.AppendProperty("Range", Range);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Returns true if this mapping is valid for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		public bool ValidateProvider([NotNull] ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (TelemetryProviderTypes != null)
				return provider.GetType().GetAllTypes().Any(t => TelemetryProviderTypes.Contains(t));

			return true;
		}
	}
}
