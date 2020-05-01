using System;
using System.Globalization;
using System.Linq;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.Crestron
{
	public sealed class FusionTelemetryBinding : IDisposable
	{
		private static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		public IFeedbackTelemetryItem GetTelemetry { get; private set; }
		public IManagementTelemetryItem SetTelemetry { get; private set; }
		public FusionSigMapping Mapping { get; private set; }
		public IFusionRoom FusionRoom { get; private set; }
		public IFusionAsset Asset { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		/// <param name="asset"></param>
		/// <param name="mapping"></param>
		private FusionTelemetryBinding(IFusionRoom fusionRoom, ITelemetryItem getTelemetry, ITelemetryItem setTelemetry,
		                               IFusionAsset asset, FusionSigMapping mapping)
		{
			FusionRoom = fusionRoom;
			GetTelemetry = getTelemetry as IFeedbackTelemetryItem;
			SetTelemetry = setTelemetry as IManagementTelemetryItem;
			Asset = asset;

			IUpdatableTelemetryNodeItem updatable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updatable != null)
				updatable.OnValueChanged += UpdatableOnValueChanged;

			Mapping = mapping;

			UpdateSig();
		}

		public void UpdateTelemetryNode()
		{
			switch (Mapping.SigType)
			{
				case eSigType.Digital:
					UpdateDigitalTelemetry();
					break;
				case eSigType.Analog:
					UpdateAnalogTelemetry();
					break;
				case eSigType.Serial:
					UpdateSerialTelemetry();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateDigitalTelemetry()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			bool digital = ((IFusionStaticAsset)Asset).ReadDigitalSig(singleMapping.Sig);

			// Handle case where two separate digitals are used to set true and false
			if (SetTelemetry.ParameterCount == 0 && digital)
				SetTelemetry.Invoke();
			else
				SetTelemetry.Invoke(digital);
		}

		private void UpdateAnalogTelemetry()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			ushort analog = ((IFusionStaticAsset)Asset).ReadAnalogSig(singleMapping.Sig);
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
				SetTelemetry.Invoke(rescaledValue);
			}
			catch (Exception e)
			{
				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} - Failed to invoke SetTelemetry method - {1}", this, e.Message);
			}
		}

		private void UpdateSerialTelemetry()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			string serial = ((IFusionStaticAsset)Asset).ReadSerialSig(singleMapping.Sig);
			SetTelemetry.Invoke(serial);
		}

		private void UpdatableOnValueChanged(object sender, EventArgs eventArgs)
		{
			UpdateSig();
		}

		private void UpdateSig()
		{
			if (GetTelemetry == null)
				return;

			if (Mapping.Sig == 0)
			{
				UpdateReservedSig();
				return;
			}

			switch (Mapping.SigType)
			{
				case eSigType.Digital:
					UpdateDigitalSig();
					break;
				case eSigType.Analog:
					UpdateAnalogSig();
					break;
				case eSigType.Serial:
					UpdateSerialSig();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateReservedSig()
		{
			IFusionStaticAsset asset = Asset as IFusionStaticAsset;
			if (asset != null)
			{
				if (Mapping.TelemetryGetName == DeviceTelemetryNames.ONLINE_STATE)
				{
					bool value = (bool)GetTelemetry.Value;
					asset.SetOnlineState(value);
					return;
				}
				if (Mapping.TelemetryGetName == DeviceTelemetryNames.POWER_STATE)
				{
					ePowerState value = (ePowerState)GetTelemetry.Value;
					bool powered = value == ePowerState.PowerOn || value == ePowerState.Warming;
					asset.SetPoweredState(powered);
					return;
				}
			}

			IFusionOccupancySensorAsset sensorAsset = Asset as IFusionOccupancySensorAsset;
			if (sensorAsset != null)
			{
				if (Mapping.TelemetryGetName == OccupancyTelemetryNames.OCCUPANCY_STATE)
				{
					bool value = (eOccupancyState)GetTelemetry.Value == eOccupancyState.Occupied;
					sensorAsset.SetRoomOccupied(value);
				}
			}
		}

		private void UpdateDigitalSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null || GetTelemetry.Value == null)
				return;

			bool digital = (bool)GetTelemetry.Value;
			((IFusionStaticAsset)Asset).UpdateDigitalSig(singleMapping.Sig, digital);
		}

		private void UpdateAnalogSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null || GetTelemetry.Value == null)
				return;

			ushort analog;

			try
			{
				analog = GetValueAsAnalog();
			}
			catch (Exception e)
			{
				ServiceProvider.GetService<ILoggerService>()
				               .AddEntry(eSeverity.Error, "{0} - Failed to convert value to analog - {1}", this, e.Message);
				return;
			}

			((IFusionStaticAsset)Asset).UpdateAnalogSig(singleMapping.Sig, analog);
		}

		private void UpdateSerialSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null)
				return;

			string serial;
			if (GetTelemetry.Value == null)
				serial = String.Empty;
			else
				serial = GetTelemetry.Value.ToString();

			((IFusionStaticAsset)Asset).UpdateSerialSig(singleMapping.Sig, serial);
		}

		private object GetRescaledAnalogValue(ushort analog)
		{
			if (SetTelemetry == null || SetTelemetry.ParameterCount == 0)
				throw new InvalidOperationException("Set telemetry is null or has no parameters");

			Type targetType = SetTelemetry.ParameterTypes[0];

			RangeAttribute rangeAttribute =
				GetTelemetry.PropertyInfo
				            .GetCustomAttributes<RangeAttribute>()
				            .FirstOrDefault();

			if (rangeAttribute != null)
				return Convert.ChangeType(rangeAttribute.RemapMinMax(analog), targetType, CultureInfo.InvariantCulture);

			return RangeAttribute.Clamp(analog, targetType);
		}

		private ushort GetValueAsAnalog()
		{
			RangeAttribute rangeAttribute =
				GetTelemetry.PropertyInfo
				            .GetCustomAttributes<RangeAttribute>()
				            .FirstOrDefault();

			if (rangeAttribute != null)
				return Convert.ToUInt16(rangeAttribute.ClampMinMaxThenRemap(GetTelemetry.Value, typeof(ushort)));

			// If decimal map dec min-max to ushort min-max
			Type propertyType = GetTelemetry.PropertyInfo.PropertyType;
			if (propertyType.IsDecimalNumeric())
				return Convert.ToUInt16(RangeAttribute.Remap(GetTelemetry.Value, typeof(ushort)));

			// Integral types are clamped
			return Convert.ToUInt16(RangeAttribute.Clamp(GetTelemetry.Value, typeof(ushort)));
		}

		public static FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryProvider provider,
		                                          FusionSigMapping mapping, uint assetId)
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

			ITelemetryItem getTelemetryItem = TelemetryService.GetTelemetryForProvider(provider, mapping.TelemetryGetName);
			ITelemetryItem setTelemetryItem = TelemetryService.GetTelemetryForProvider(provider, mapping.TelemetrySetName);

			if (getTelemetryItem == null && setTelemetryItem == null)
				throw new InvalidOperationException("Cannot create telemetry binding with neither a getter nor a setter.");

			// If the sig number is 0, that indicates that the sig is special-handling
			if (mapping.Sig != 0)
				fusionRoom.AddSig(assetId, mapping.SigType, mapping.Sig, mapping.FusionSigName, mapping.IoMask);

			return new FusionTelemetryBinding(fusionRoom, getTelemetryItem, setTelemetryItem, asset, mapping);
		}

		public void Dispose()
		{
			IUpdatableTelemetryNodeItem updatable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updatable != null)
				updatable.OnValueChanged -= UpdatableOnValueChanged;
		}
	}
}
