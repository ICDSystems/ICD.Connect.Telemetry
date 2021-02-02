namespace ICD.Connect.Telemetry.Debounce
{
	public enum eDebounceMode
	{
		/// <summary>
		/// Changes are reported immediately.
		/// </summary>
		None,

		/// <summary>
		/// All changes are held for a duration.
		/// </summary>
		All,

		/// <summary>
		/// Default to non-default changes are held for a duration.
		/// </summary>
		RisingEdge,

		/// <summary>
		/// Non-default to default changes are held for a duration.
		/// </summary>
		FallingEdge
	}
}