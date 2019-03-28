using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.Devices;
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
		public IFusionStaticAsset Asset { get; private set; }

		private FusionTelemetryBinding(IFusionRoom fusionRoom, ITelemetryItem getTelemetry, ITelemetryItem setTelemetry, IFusionStaticAsset asset, FusionSigMapping mapping)
		{
			FusionRoom = fusionRoom;
			GetTelemetry = getTelemetry as IFeedbackTelemetryItem;
			SetTelemetry = setTelemetry as IManagementTelemetryItem;
			Asset = asset;

			IUpdatableTelemetryNodeItem updatable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if(updatable != null)
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
			if(singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			bool digital = Asset.ReadDigitalSig(singleMapping.Sig);
			SetTelemetry.Invoke(digital);
		}

		private void UpdateAnalogTelemetry()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			ushort analog = Asset.ReadAnalogSig(singleMapping.Sig);
			object rescaledValue = GetRescaledAnalogValue(analog);
			SetTelemetry.Invoke(rescaledValue);
		}

		private void UpdateSerialTelemetry()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null)
				return;

			if (Mapping.Sig == 0)
				return;

			string serial = Asset.ReadSerialSig(singleMapping.Sig);
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
			if (Mapping.TelemetryGetName == DeviceTelemetryNames.ONLINE_STATE)
			{
				bool value = (bool)GetTelemetry.Value;
				Asset.SetOnlineState(value);
				return;
			}
			if (Mapping.TelemetryGetName == DeviceTelemetryNames.POWER_STATE)
			{
				bool value = (bool)GetTelemetry.Value;
				Asset.SetPoweredState(value);
				return;
			}
		}

		private void UpdateDigitalSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null || GetTelemetry.Value == null)
				return;

			bool digital = (bool)GetTelemetry.Value;
			Asset.UpdateDigitalSig(singleMapping.Sig, digital);
		}

		private void UpdateAnalogSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null || GetTelemetry.Value == null)
				return;

			ushort analog = GetNewAnalogValue();
			Asset.UpdateAnalogSig(singleMapping.Sig, analog);
		}

		private void UpdateSerialSig()
		{
			FusionSigMapping singleMapping = Mapping;
			if (singleMapping == null || GetTelemetry == null ||GetTelemetry.Value == null)
				return;

			string serial = GetTelemetry.Value.ToString();
			Asset.UpdateSerialSig(singleMapping.Sig, serial);
		}

		private object GetRescaledAnalogValue(ushort analog)
		{
			//TODO:FIX ME
			return 0;
		}

		private ushort GetNewAnalogValue()
		{
			if (!GetTelemetry.ValueType.IsDecimalNumeric())
			{
				return (ushort)Convert.ChangeType(GetTelemetry.Value, typeof(ushort), null);
			}

			RangeAttribute rangeAttribute = GetTelemetry.PropertyInfo
			                                            .GetCustomAttributes<RangeAttribute>()
			                                            .FirstOrDefault(null);
			if (rangeAttribute == null)
			{
				return (ushort)Convert.ChangeType(GetTelemetry.Value, typeof(ushort), null);
			}

			if (rangeAttribute.IsInRange(GetTelemetry.Value))
			{
				double value = (double)Convert.ChangeType(GetTelemetry.Value, typeof(double), null);

				//Cast value to double, since we only care about decimal types here 
				//and all decimal types can be converted to double
				return rangeAttribute.RemapRangeToUshort(value);
			}

			ServiceProvider.GetService<ILoggerService>()
			               .AddEntry(eSeverity.Warning,
			                         "Warning when converting analog telemtry value. Value {0} is outside expected range {1}-{2}. Value will be set to 0.",
			                         GetTelemetry.Value,
			                         rangeAttribute.Min,
			                         rangeAttribute.Max);
			return 0;
		}

		public static FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryProvider provider, FusionSigMapping mapping, uint assetId)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if(provider == null)
				throw new ArgumentException("provider");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			ITelemetryItem getTelemetryItem = TelemetryService.GetTelemetryForProvider(provider, mapping.TelemetryGetName);
			ITelemetryItem setTelemetryItem = TelemetryService.GetTelemetryForProvider(provider, mapping.TelemetrySetName);

			if(getTelemetryItem == null && setTelemetryItem == null)
				throw new InvalidOperationException("Cannot create telemetry binding with neither a getter nor a setter.");

			// If the sig number is 0, that indicates that the sig is special-handling
			if (mapping.Sig != 0)
				fusionRoom.AddSig(assetId, mapping.SigType, mapping.Sig, mapping.FusionSigName, mapping.IoMask);

			IFusionStaticAsset asset = fusionRoom.UserConfigurableAssetDetails[assetId].Asset as IFusionStaticAsset;
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