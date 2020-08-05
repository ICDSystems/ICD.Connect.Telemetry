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
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryBinding : AbstractTelemetryBinding
	{
		[NotNull] private readonly IFusionAsset m_Asset;
		[CanBeNull] private readonly RangeAttribute m_RangeAttribute;

		#region Properties

		[NotNull]
		public AssetFusionSigMapping Mapping { get; private set; }

		[NotNull]
		private static ILoggerService Logger { get { return ServiceProvider.GetService<ILoggerService>(); } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="asset"></param>
		/// <param name="mapping"></param>
		private FusionTelemetryBinding([NotNull] TelemetryLeaf telemetry,
		                               [NotNull] IFusionAsset asset,
		                               [NotNull] AssetFusionSigMapping mapping)
			: base(telemetry)
		{
			if (asset == null)
				throw new ArgumentNullException("asset");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			if (telemetry.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			m_Asset = asset;

			Mapping = mapping;

			m_RangeAttribute =
				Telemetry.PropertyInfo == null
					? null
					: Telemetry.PropertyInfo
					           .GetCustomAttributes<RangeAttribute>()
					           .FirstOrDefault();
		}

		/// <summary>
		/// Creates the Fusion telemetry binding for the given leaf.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="mapping"></param>
		/// <param name="assetId"></param>
		/// <returns></returns>
		[NotNull]
		public static FusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom, [NotNull] TelemetryLeaf leaf,
		                                          [NotNull] AssetFusionSigMapping mapping, uint assetId)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (leaf == null)
				throw new ArgumentException("provider");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			// Check against the fusion asset whitelist for the mapping
			IFusionAsset asset = fusionRoom.UserConfigurableAssetDetails[assetId].Asset;
			if (!mapping.ValidateAsset(asset.AssetType))
				throw new ArgumentException(string.Format("Mapping {0} does not support {1}", mapping.FusionSigName, asset.GetType()));

			if (leaf.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			// If the sig number is 0, that indicates that the sig is special-handling
			if (mapping.Sig != 0)
				fusionRoom.AddSig(assetId, mapping.SigType, mapping.Sig, mapping.FusionSigName, leaf.IoMask);

			return new FusionTelemetryBinding(leaf, asset, mapping);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Converts the given object to a digital for fusion.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static bool GetValueAsDigital([CanBeNull] object value)
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
		/// <param name="range"></param>
		/// <returns></returns>
		private static ushort GetValueAsAnalog([CanBeNull] object value, [CanBeNull] RangeAttribute range)
		{
			// Nullables
			if (value == null)
				return 0;

			if (range != null)
				return Convert.ToUInt16(range.ClampMinMaxThenRemap(value, typeof(ushort)));

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
		private static string GetValueAsSerial([CanBeNull] object value)
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
		private object GetAnalogAsValue(ushort analog)
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
			// Special handling
			if (Mapping.SendReservedSig != null)
			{
				Mapping.SendReservedSig(m_Asset, value);
				return;
			}

			// Static asset sigs
			IFusionStaticAsset staticAsset = m_Asset as IFusionStaticAsset;
			if (staticAsset == null)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Unable to send value to non-static asset", this);
				return;
			}

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
		private void SendDigitalSigToService(object value)
		{
			try
			{
				bool digital = GetValueAsDigital(value);
				((IFusionStaticAsset)m_Asset).UpdateDigitalSig(Mapping.Sig, digital);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to send value to Fusion as a Digital - {1}", this, e.Message);
			}
		}

		/// <summary>
		/// Sends the analog sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendAnalogSigToService([CanBeNull] object value)
		{
			try
			{
				ushort analog = GetValueAsAnalog(value, m_RangeAttribute);
				((IFusionStaticAsset)m_Asset).UpdateAnalogSig(Mapping.Sig, analog);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to send value to Fusion as an Analog - {1}", this, e.Message);
			}
		}

		/// <summary>
		/// Sends the serial sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendSerialSigToService(object value)
		{
			try
			{
				string serial = GetValueAsSerial(value);
				((IFusionStaticAsset)m_Asset).UpdateSerialSig(Mapping.Sig, serial);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to send value to Fusion as a Serial - {1}", this, e.Message);
			}
		}

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

		private void UpdateDigitalTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			bool digital = ((IFusionStaticAsset)m_Asset).ReadDigitalSig(Mapping.Sig);

			// Handle case where two separate digitals are used to set true and false
			if (Telemetry.ParameterCount == 0 && digital)
				Telemetry.Invoke();
			else
				Telemetry.Invoke(digital);
		}

		private void UpdateAnalogTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			ushort analog = ((IFusionStaticAsset)m_Asset).ReadAnalogSig(Mapping.Sig);
			object rescaledValue;

			try
			{
				rescaledValue = GetAnalogAsValue(analog);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to convert analog telemetry value - {1}", this, e.Message);
				return;
			}

			try
			{
				Telemetry.Invoke(rescaledValue);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to invoke Telemetry method - {1}", this, e.Message);
			}
		}

		private void UpdateSerialTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			string serial = ((IFusionStaticAsset)m_Asset).ReadSerialSig(Mapping.Sig);
			Telemetry.Invoke(serial);
		}

		#endregion
	}
}
