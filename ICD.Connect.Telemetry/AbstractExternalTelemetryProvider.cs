namespace ICD.Connect.Telemetry
{
	public abstract class AbstractExternalTelemetryProvider<TParent> : AbstractTelemetryProvider, IExternalTelemetryProvider
		where TParent : ITelemetryProvider
	{
		private TParent m_Provider;

		/// <summary>
		/// Gets the parent telemetry provider.
		/// </summary>
		protected TParent Parent { get { return m_Provider; } }

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="provider"></param>
		void IExternalTelemetryProvider.SetParent(ITelemetryProvider provider)
		{
			SetParent((TParent)provider);
		}

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="provider"></param>
		public void SetParent(TParent provider)
		{
			m_Provider = provider;
		}
	}
}