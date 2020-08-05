using System;
using System.Globalization;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Bindings;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Bindings
{
	public abstract class AbstractFusionTelemetryBinding<TMapping> : AbstractTelemetryBinding
		where TMapping : class, IFusionSigMapping
	{
		[CanBeNull] private readonly RangeAttribute m_RangeAttribute;

		#region Properties

		[NotNull]
		public TMapping Mapping { get; private set; }

		[NotNull]
		protected static ILoggerService Logger { get { return ServiceProvider.GetService<ILoggerService>(); } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="mapping"></param>
		protected AbstractFusionTelemetryBinding([NotNull] TelemetryLeaf telemetry,
		                               [NotNull] TMapping mapping)
			: base(telemetry)
		{
			if (mapping == null)
				throw new ArgumentNullException("mapping");

			if (telemetry.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			Mapping = mapping;

			m_RangeAttribute =
				Telemetry.PropertyInfo == null
					? null
					: Telemetry.PropertyInfo
					           .GetCustomAttributes<RangeAttribute>()
					           .FirstOrDefault();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Converts the given object to a digital for fusion.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static bool GetValueAsDigital([CanBeNull] object value)
		{
			// Nullables
			if (value == null)
				return false;

			return (bool)value;
		}

		/// <summary>
		/// Converts the given numeric value to a ushort for Fusion.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected ushort GetValueAsAnalog([CanBeNull] object value)
		{
			// Nullables
			if (value == null)
				return 0;

			if (m_RangeAttribute != null)
				return Convert.ToUInt16(m_RangeAttribute.ClampMinMaxThenRemap(value, typeof(ushort)));

			// If decimal map dec min-max to ushort min-max
			if (value.GetType().IsDecimalNumeric())
				return Convert.ToUInt16(RangeAttribute.Remap(value, typeof(ushort)));

			// Integral types are clamped
			return Convert.ToUInt16(RangeAttribute.Clamp(value, typeof(ushort)));
		}

		/// <summary>
		/// Converts the given object to a serial for Fusion.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static string GetValueAsSerial([CanBeNull] object value)
		{
			if (value == null)
				return string.Empty;

			if (value is TimeSpan)
			{
				TimeSpan timeSpan = (TimeSpan)value;
				return string.Format("{0} days {1:D2}:{2:D2}:{3:D2}.{4:D3}",
				                     timeSpan.Days,
				                     timeSpan.Hours,
				                     timeSpan.Minutes,
				                     timeSpan.Seconds,
				                     timeSpan.Milliseconds);
			}

			if (value is DateTime)
			{
				DateTime dateTime = (DateTime)value;
				return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
			}

			return value.ToString();
		}

		/// <summary>
		/// Converts the given Fusion analog to the telemetry parameter type.
		/// </summary>
		/// <param name="analog"></param>
		/// <returns></returns>
		protected object GetAnalogAsValue(ushort analog)
		{
			if (Telemetry.ParameterCount != 1)
				throw new InvalidOperationException("Telemetry must have exactly 1 parameter");

			Type targetType = Telemetry.Parameters.Single().ParameterType;

			if (m_RangeAttribute != null)
				return Convert.ChangeType(m_RangeAttribute.RemapMinMax(analog), targetType, CultureInfo.InvariantCulture);

			return RangeAttribute.Clamp(analog, targetType);
		}

		#endregion

		#region Program to Service

		/// <summary>
		/// Sends the value to the telemetry service.
		/// </summary>
		/// <param name="value"></param>
		protected override void SendValueToService(object value)
		{
			switch (Mapping.SigType)
			{
				case eSigType.Digital:
					SendDigitalSigToService(value);
					break;
				case eSigType.Analog:
					SendAnalogSigToService(value);
					break;
				case eSigType.Serial:
					SendSerialSigToService(value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Sends the digital sig to the service.
		/// </summary>
		/// <param name="value"></param>
		protected abstract void SendDigitalSigToService(object value);

		/// <summary>
		/// Sends the analog sig to the service.
		/// </summary>
		/// <param name="value"></param>
		protected abstract void SendAnalogSigToService([CanBeNull] object value);

		/// <summary>
		/// Sends the serial sig to the service.
		/// </summary>
		/// <param name="value"></param>
		protected abstract void SendSerialSigToService(object value);

		#endregion

		#region Service to Program

		public void UpdateLocalNodeValueFromService()
		{
			switch (Mapping.SigType)
			{
				case eSigType.Digital:
					UpdateDigitalTelemetryFromService();
					break;
				case eSigType.Analog:
					UpdateAnalogTelemetryFromService();
					break;
				case eSigType.Serial:
					UpdateSerialTelemetryFromService();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected abstract void UpdateDigitalTelemetryFromService();

		protected abstract void UpdateAnalogTelemetryFromService();

		protected abstract void UpdateSerialTelemetryFromService();

		#endregion
	}
}
