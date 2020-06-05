using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Bindings
{
	public abstract class AbstractTelemetryBinding : IDisposable
	{
		[NotNull] private readonly TelemetryLeaf m_Telemetry;

		/// <summary>
		/// Gets the wrapped telemetry leaf.
		/// </summary>
		[NotNull]
		public TelemetryLeaf Telemetry { get { return m_Telemetry; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="telemetry"></param>
		protected AbstractTelemetryBinding([NotNull] TelemetryLeaf telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			m_Telemetry = telemetry;

			m_Telemetry.OnValueRaised += UpdateableOnValueRaised;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
			m_Telemetry.OnValueRaised -= UpdateableOnValueRaised;
		}

		/// <summary>
		/// Called when the wrapped property value changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void UpdateableOnValueRaised(object sender, GenericEventArgs<object> eventArgs)
		{
			SendValueToService(eventArgs.Data);
		}

		/// <summary>
		/// Sends the value to the telemetry service.
		/// </summary>
		/// <param name="value"></param>
		protected abstract void SendValueToService(object value);

		/// <summary>
		/// Handles a method call with parameters from the service.
		/// </summary>
		/// <param name="values"></param>
		protected void HandleValuesFromService(object[] values)
		{
			Telemetry.Invoke(values);
		}
	}
}
