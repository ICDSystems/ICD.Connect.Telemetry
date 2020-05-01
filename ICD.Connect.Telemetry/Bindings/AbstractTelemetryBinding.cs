using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.Bindings
{
	public abstract class AbstractTelemetryBinding : IDisposable
	{
		[CanBeNull] private readonly IFeedbackTelemetryItem m_GetTelemetry;
		[CanBeNull] private readonly IManagementTelemetryItem m_SetTelemetry;

		protected static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		[CanBeNull]
		public IFeedbackTelemetryItem GetTelemetry { get { return m_GetTelemetry; } }

		[CanBeNull]
		public IManagementTelemetryItem SetTelemetry { get { return m_SetTelemetry; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		protected AbstractTelemetryBinding([CanBeNull] IFeedbackTelemetryItem getTelemetry, [CanBeNull] IManagementTelemetryItem setTelemetry)
		{
			m_GetTelemetry = getTelemetry;
			m_SetTelemetry = setTelemetry;

			IUpdatableTelemetryNodeItem updateable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updateable != null)
				updateable.OnValueChanged += UpdateableOnValueChanged;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
			IUpdatableTelemetryNodeItem updateable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updateable != null)
				updateable.OnValueChanged -= UpdateableOnValueChanged;
		}

		public abstract void UpdateLocalNodeValueFromService();

		protected abstract void UpdateAndSendValueToService();

		private void UpdateableOnValueChanged(object sender, EventArgs eventArgs)
		{
			UpdateAndSendValueToService();
		}
	}
}