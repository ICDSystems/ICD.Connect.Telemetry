using System;
using ICD.Common.Utils.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.Bindings
{
	public abstract class AbstractTelemetryBinding : IDisposable
	{
		protected static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }
		public IFeedbackTelemetryItem GetTelemetry { get; private set; }
		public IManagementTelemetryItem SetTelemetry { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		protected AbstractTelemetryBinding(ITelemetryItem getTelemetry, ITelemetryItem setTelemetry)
		{
			
			GetTelemetry = getTelemetry as IFeedbackTelemetryItem;
			SetTelemetry = setTelemetry as IManagementTelemetryItem;

			IUpdatableTelemetryNodeItem updatable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updatable != null)
				updatable.OnValueChanged += UpdatableOnValueChanged;
		}

		public abstract void UpdateLocalNodeValueFromService();
		protected abstract void UpdateAndSendValueToService();

		private void UpdatableOnValueChanged(object sender, EventArgs eventArgs)
		{
			UpdateAndSendValueToService();
		}

		public void Dispose()
		{
			IUpdatableTelemetryNodeItem updatable = GetTelemetry as IUpdatableTelemetryNodeItem;
			if (updatable != null)
				updatable.OnValueChanged -= UpdatableOnValueChanged;
		}
	}
}