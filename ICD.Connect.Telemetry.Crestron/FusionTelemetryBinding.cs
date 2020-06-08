using System;
using System.Globalization;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Bindings;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryBinding : AbstractTelemetryBinding
	{
		#region Properties

		[NotNull]
		public FusionSigMapping Mapping { get; private set; }

		[NotNull]
		public IFusionRoom FusionRoom { get; private set; }

		[NotNull]
		public IFusionAsset Asset { get; private set; }

		/// <summary>
		/// Gets the telemetry service.
		/// </summary>
		[NotNull]
		private static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		[CanBeNull]
		private RangeAttribute RangeAttribute { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="telemetry"></param>
		/// <param name="asset"></param>
		/// <param name="mapping"></param>
		private FusionTelemetryBinding([NotNull] IFusionRoom fusionRoom, [NotNull] TelemetryLeaf telemetry,
		                               [NotNull] IFusionAsset asset, [NotNull] FusionSigMapping mapping) 
			: base(telemetry)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (asset == null)
				throw new ArgumentNullException("asset");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			if (telemetry.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			FusionRoom = fusionRoom;
			Asset = asset;
			Mapping = mapping;

			RangeAttribute =
				Telemetry.PropertyInfo == null
					? null
					: Telemetry.PropertyInfo
					           .GetCustomAttributes<RangeAttribute>()
					           .FirstOrDefault();

			if (telemetry.PropertyInfo != null)
				SendValueToService(telemetry.Value);
		}

		/// <summary>
		/// Creates the Fusion telemetry binding for the given mapping.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="provider"></param>
		/// <param name="mapping"></param>
		/// <param name="assetId"></param>
		/// <returns></returns>
		[CanBeNull]
		public static FusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom, [NotNull] ITelemetryProvider provider,
		                                          [NotNull] FusionSigMapping mapping, uint assetId)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (provider == null)
				throw new ArgumentException("provider");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			// Check against the fusion asset whitelist for the mapping
			IFusionAsset asset = fusionRoom.UserConfigurableAssetDetails[assetId].Asset;
			if (mapping.FusionAssetTypes != null && !asset.GetType().GetAllTypes().Any(t => mapping.FusionAssetTypes.Contains(t)))
				return null;

			TelemetryLeaf telemetryLeaf =
				(TelemetryLeaf)TelemetryService.GetTelemetryForProvider(provider)
				                               .GetChildByName(mapping.TelemetryName);

			if (telemetryLeaf.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			// If the sig number is 0, that indicates that the sig is special-handling
			if (mapping.Sig != 0)
				fusionRoom.AddSig(assetId, mapping.SigType, mapping.Sig, mapping.FusionSigName, telemetryLeaf.IoMask);

			return new FusionTelemetryBinding(fusionRoom, telemetryLeaf, asset, mapping);
		}

		#endregion

		#region Program to Service

		/// <summary>
		/// Sends the value to the telemetry service.
		/// </summary>
		/// <param name="value"></param>
		protected override void SendValueToService(object value)
		{
			if (Mapping.Sig == 0)
			{
				SendReservedSigToService(value);
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
		/// Sends the reserved sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendReservedSigToService(object value)
		{
			IFusionStaticAsset staticAsset = Asset as IFusionStaticAsset;
			if (staticAsset != null)
			{
				switch (Mapping.TelemetryName)
				{
					case DeviceTelemetryNames.ONLINE_STATE:
						staticAsset.SetOnlineState((bool)value);
						break;

					case DeviceTelemetryNames.POWER_STATE:
						ePowerState powerState = (ePowerState)value;
						bool powered = powerState == ePowerState.PowerOn || powerState == ePowerState.Warming;
						staticAsset.SetPoweredState(powered);
						break;
				}
			}

			IFusionOccupancySensorAsset sensorAsset = Asset as IFusionOccupancySensorAsset;
			if (sensorAsset != null)
			{
				switch (Mapping.TelemetryName)
				{
					case OccupancyTelemetryNames.OCCUPANCY_STATE:
						bool occupied = (eOccupancyState)value == eOccupancyState.Occupied;
						sensorAsset.SetRoomOccupied(occupied);
						break;
				}
			}
		}

		/// <summary>
		/// Sends the digital sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendDigitalSigToService(object value)
		{
			((IFusionStaticAsset)Asset).UpdateDigitalSig(Mapping.Sig, (bool)value);
		}

		/// <summary>
		/// Sends the analog sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendAnalogSigToService(object value)
		{
			ushort analog;

			try
			{
				analog = GetValueAsAnalog(value);
			}
			catch (Exception e)
			{
				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} - Failed to convert value to analog - {1}", this, e.Message);
				return;
			}

			((IFusionStaticAsset)Asset).UpdateAnalogSig(Mapping.Sig, analog);
		}

		/// <summary>
		/// Sends the serial sig to the service.
		/// </summary>
		/// <param name="value"></param>
		private void SendSerialSigToService(object value)
		{
			string serial = value == null ? string.Empty : value.ToString();
			((IFusionStaticAsset)Asset).UpdateSerialSig(Mapping.Sig, serial);
		}

		/// <summary>
		/// Converts the given numeric value to a ushort for Fusion.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private ushort GetValueAsAnalog(object value)
		{
			if (RangeAttribute != null)
				return Convert.ToUInt16(RangeAttribute.ClampMinMaxThenRemap(value, typeof(ushort)));

			// If decimal map dec min-max to ushort min-max
			if (value.GetType().IsDecimalNumeric())
				return Convert.ToUInt16(RangeAttribute.Remap(value, typeof(ushort)));

			// Integral types are clamped
			return Convert.ToUInt16(RangeAttribute.Clamp(value, typeof(ushort)));
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

			bool digital = ((IFusionStaticAsset)Asset).ReadDigitalSig(Mapping.Sig);

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

			ushort analog = ((IFusionStaticAsset)Asset).ReadAnalogSig(Mapping.Sig);
			object rescaledValue;

			try
			{
				rescaledValue = GetRescaledAnalogValue(analog);
			}
			catch (Exception e)
			{
				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} - Failed to convert analog telemtry value - {1}", this, e.Message);
				return;
			}

			try
			{
				Telemetry.Invoke(rescaledValue);
			}
			catch (Exception e)
			{
				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} - Failed to invoke Telemetry method - {1}", this, e.Message);
			}
		}

		private void UpdateSerialTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			string serial = ((IFusionStaticAsset)Asset).ReadSerialSig(Mapping.Sig);
			Telemetry.Invoke(serial);
		}

		private object GetRescaledAnalogValue(ushort analog)
		{
			if (Telemetry.ParameterCount != 1)
				throw new InvalidOperationException("Telemetry must have exactly 1 parameter");

			Type targetType = Telemetry.Parameters.First().ParameterType;

			if (RangeAttribute != null)
				return Convert.ChangeType(RangeAttribute.RemapMinMax(analog), targetType, CultureInfo.InvariantCulture);

			return RangeAttribute.Clamp(analog, targetType);
		}

		#endregion
	}
}
