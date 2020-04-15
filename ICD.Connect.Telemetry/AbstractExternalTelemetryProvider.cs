namespace ICD.Connect.Telemetry
{
	public abstract class AbstractExternalTelemetryProvider<TParent> : AbstractTelemetryProvider, IExternalTelemetryProvider
		where TParent : ITelemetryProvider
	{
		private TParent m_Parent;

		/// <summary>
		/// Gets the parent telemetry provider.
		/// </summary>
		protected TParent Parent { get { return m_Parent; } }

		#region Methods

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
		/// <param name="parent"></param>
		public virtual void SetParent(TParent parent)
		{
			Unsubscribe(m_Parent);
			m_Parent = parent;
			Subscribe(m_Parent);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected virtual void Subscribe(TParent parent)
		{
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected virtual void Unsubscribe(TParent parent)
		{
		}

		#endregion
	}
}