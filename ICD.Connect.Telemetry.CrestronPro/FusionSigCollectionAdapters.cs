#if SIMPLSHARP
using Crestron.SimplSharpPro;
using ICD.Connect.Misc.CrestronPro.Sigs;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.CrestronPro
{
	public sealed class FusionStringInputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IStringInputSig, StringSigData>, IDeviceStringInputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionStringInputCollectionAdapter(CrestronCollection<StringSigData> collection)
			: base(collection, data => new StringInputSigAdapter(data.InputSig))
		{
		}
	}

	public sealed class FusionStringOutputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IStringOutputSig, StringSigData>, IDeviceStringOutputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionStringOutputCollectionAdapter(CrestronCollection<StringSigData> collection)
			: base(collection, data => new StringOutputSigAdapter(data.OutputSig))
		{
		}
	}

	public sealed class FusionUShortInputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IUShortInputSig, UShortSigData>, IDeviceUShortInputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionUShortInputCollectionAdapter(CrestronCollection<UShortSigData> collection)
			: base(collection, data => new UShortInputSigAdapter(data.InputSig))
		{
		}
	}

	public sealed class FusionUShortOutputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IUShortOutputSig, UShortSigData>, IDeviceUShortOutputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionUShortOutputCollectionAdapter(CrestronCollection<UShortSigData> collection)
			: base(collection, data => new UShortOutputSigAdapter(data.OutputSig))
		{
		}
	}

	public sealed class FusionBooleanInputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IBoolInputSig, BooleanSigData>, IDeviceBooleanInputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionBooleanInputCollectionAdapter(CrestronCollection<BooleanSigData> collection)
			: base(collection, data => new BoolInputSigAdapter(data.InputSig))
		{
		}
	}

	public sealed class FusionBooleanOutputCollectionAdapter :
		AbstractFusionSigCollectionAdapter<IBoolOutputSig, BooleanSigData>, IDeviceBooleanOutputCollection
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionBooleanOutputCollectionAdapter(CrestronCollection<BooleanSigData> collection)
			: base(collection, data => new BoolOutputSigAdapter(data.OutputSig))
		{
		}
	}
}
#endif
