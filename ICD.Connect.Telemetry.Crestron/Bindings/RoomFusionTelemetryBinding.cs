using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Bindings
{
	public sealed class RoomFusionTelemetryBinding : AbstractFusionTelemetryBinding<RoomFusionSigMapping>
	{
		[NotNull] private readonly IFusionRoom m_Room;

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="room"></param>
		/// <param name="mapping"></param>
		private RoomFusionTelemetryBinding([NotNull] TelemetryLeaf telemetry,
		                                   [NotNull] IFusionRoom room,
		                                   [NotNull] RoomFusionSigMapping mapping)
			: base(telemetry, mapping)
		{
			if (room == null)
				throw new ArgumentNullException("asset");

			m_Room = room;
		}

		/// <summary>
		/// Creates the Fusion telemetry binding for the given leaf.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="mapping"></param>
		/// <returns></returns>
		[NotNull]
		public static RoomFusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom,
		                                              [NotNull] TelemetryLeaf leaf,
		                                              [NotNull] RoomFusionSigMapping mapping)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			if (leaf == null)
				throw new ArgumentException("provider");

			if (mapping == null)
				throw new ArgumentNullException("mapping");

			if (leaf.ParameterCount > 1)
				throw new NotSupportedException("Method telemetry with more than 1 parameter is not supported by Fusion");

			// If the sig number is 0, that indicates that the sig is special-handling
			if (mapping.Sig != 0)
				fusionRoom.AddSig(mapping.SigType, mapping.Sig, mapping.FusionSigName, leaf.IoMask);

			return new RoomFusionTelemetryBinding(leaf, fusionRoom, mapping);
		}

		#endregion

		#region Program to Service

		/// <summary>
		/// Sends the value to the telemetry service.
		/// </summary>
		/// <param name="value"></param>
		protected override void SendValueToService(object value)
		{
			if (Mapping.SendReservedSig == null)
				base.SendValueToService(value);
			else
				Mapping.SendReservedSig(m_Room, value);
		}

		/// <summary>
		/// Sends the digital sig to the service.
		/// </summary>
		/// <param name="value"></param>
		protected override void SendDigitalSigToService(object value)
		{
			try
			{
				bool digital = GetValueAsDigital(value);
				m_Room.UpdateDigitalSig(Mapping.Sig, digital);
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
		protected override void SendAnalogSigToService([CanBeNull] object value)
		{
			try
			{
				ushort analog = GetValueAsAnalog(value);
				m_Room.UpdateAnalogSig(Mapping.Sig, analog);
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
		protected override void SendSerialSigToService(object value)
		{
			try
			{
				string serial = GetValueAsSerial(value);
				m_Room.UpdateSerialSig(Mapping.Sig, serial);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Failed to send value to Fusion as a Serial - {1}", this, e.Message);
			}
		}

		#endregion

		#region Service to Program

		protected override void UpdateDigitalTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			bool digital = m_Room.ReadDigitalSig(Mapping.Sig);

			// Handle case where two separate digitals are used to set true and false
			if (Telemetry.ParameterCount == 0 && digital)
				Telemetry.Invoke();
			else
				Telemetry.Invoke(digital);
		}

		protected override void UpdateAnalogTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			ushort analog = m_Room.ReadAnalogSig(Mapping.Sig);
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

		protected override void UpdateSerialTelemetryFromService()
		{
			if (Mapping.Sig == 0)
				return;

			string serial = m_Room.ReadSerialSig(Mapping.Sig);
			Telemetry.Invoke(serial);
		}

		#endregion
	}
}
