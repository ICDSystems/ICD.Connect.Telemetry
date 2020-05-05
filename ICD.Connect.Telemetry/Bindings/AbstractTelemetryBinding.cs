using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.Bindings
{
	public abstract class AbstractTelemetryBinding : IDisposable
	{
		[CanBeNull] private readonly PropertyTelemetryItem m_GetTelemetry;
		[CanBeNull] private readonly MethodTelemetryItem m_SetTelemetry;

		protected static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		[CanBeNull]
		public PropertyTelemetryItem GetTelemetry { get { return m_GetTelemetry; } }

		[CanBeNull]
		public MethodTelemetryItem SetTelemetry { get { return m_SetTelemetry; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		protected AbstractTelemetryBinding([CanBeNull] PropertyTelemetryItem getTelemetry, [CanBeNull] MethodTelemetryItem setTelemetry)
		{
			m_GetTelemetry = getTelemetry;
			m_SetTelemetry = setTelemetry;

			if (m_GetTelemetry != null)
				m_GetTelemetry.OnValueChanged += UpdateableOnValueChanged;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
			if (m_GetTelemetry != null)
				m_GetTelemetry.OnValueChanged -= UpdateableOnValueChanged;
		}

		/// <summary>
		/// Called when the wrapped property value changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void UpdateableOnValueChanged(object sender, EventArgs eventArgs)
		{
			SendValueToService();
		}

		/// <summary>
		/// Sends the wrapped property value to the telemetry service.
		/// </summary>
		protected abstract void SendValueToService();

		/// <summary>
		/// Handles a simple method call from the service.
		/// </summary>
		protected void HandleValueFromService()
		{
			if (SetTelemetry == null || SetTelemetry.ParameterInfo != null)
				throw new NotSupportedException();

			SetTelemetry.Invoke();
		}

		/// <summary>
		/// Handles a method call with parameter from the service.
		/// </summary>
		/// <param name="value"></param>
		protected void HandleValueFromService(object value)
		{
			if (SetTelemetry == null || SetTelemetry.ParameterInfo == null)
				throw new NotSupportedException();

			SetTelemetry.Invoke(value);
		}
	}
}