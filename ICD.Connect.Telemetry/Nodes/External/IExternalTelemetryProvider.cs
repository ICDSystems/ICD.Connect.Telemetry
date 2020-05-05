namespace ICD.Connect.Telemetry.Nodes.External
{
	public interface IExternalTelemetryProvider : ITelemetryProvider
	{
		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="provider"></param>
		void SetParent(ITelemetryProvider provider);
	}
}
