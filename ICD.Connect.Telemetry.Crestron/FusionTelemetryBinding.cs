using System;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
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
			bool digital = Asset.ReadDigitalSig(Mapping.Sig);
			SetTelemetry.Invoke(digital);
		}

		private void UpdateAnalogTelemetry()
		{
			ushort analog = Asset.ReadAnalogSig(Mapping.Sig);
			object rescaledValue = GetRescaledAnalogValue(analog);
			SetTelemetry.Invoke(rescaledValue);
		}

		private void UpdateSerialTelemetry()
		{
			string serial = Asset.ReadSerialSig(Mapping.Sig);
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

		private void UpdateDigitalSig()
		{
			bool digital = (bool)GetTelemetry.Value;
			Asset.UpdateDigitalSig(Mapping.Sig, digital);
		}

		private void UpdateAnalogSig()
		{
			ushort analog = GetNewAnalogValue();
			Asset.UpdateAnalogSig(Mapping.Sig, analog);
		}

		private void UpdateSerialSig()
		{
			string serial = GetTelemetry.Value.ToString();
			Asset.UpdateSerialSig(Mapping.Sig, serial);
		}

		private object GetRescaledAnalogValue(ushort analog)
		{
			//TODO:FIX ME
			return 0;
		}

		private ushort GetNewAnalogValue()
		{
			if (!GetTelemetry.ValueType.IsDecimalNumeric())
				return (ushort)GetTelemetry.Value;

			RangeAttribute rangeAttribute = GetTelemetry.PropertyInfo
			                                            .GetCustomAttributes<RangeAttribute>()
			                                            .FirstOrDefault(null);
			if (rangeAttribute == null)
				return (ushort)GetTelemetry.Value;

			if (rangeAttribute.IsInRange(GetTelemetry.Value))
			{
				//Cast value to double, since we only care about decimal types here 
				//and all decimal types can be converted to double
				return rangeAttribute.RemapRangeToUShort((double)GetTelemetry.Value);
			}

			ServiceProvider.GetService<ILoggerService>()
			               .AddEntry(eSeverity.Warning,
			                         "Warning when converting analog telemtry value. Value {0} is outside expected range {1}-{2}. Value will be set to 0.",
			                         GetTelemetry.Value,
			                         rangeAttribute.Min,
			                         rangeAttribute.Max);
			return 0;
		}

		public static FusionTelemetryBinding Bind(IFusionRoom fusionRoom, FusionSigMapping mapping, uint assetId)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			ITelemetryItem getTelemetryItem = TelemetryService.GetTelemetryForProvider(fusionRoom, mapping.TelemetryGetName);
			ITelemetryItem setTelemetryItem = TelemetryService.GetTelemetryForProvider(fusionRoom, mapping.TelemetrySetName);

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