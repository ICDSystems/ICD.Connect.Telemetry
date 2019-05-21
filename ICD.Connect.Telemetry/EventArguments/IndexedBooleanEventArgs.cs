namespace ICD.Connect.Telemetry.EventArguments
{
	public sealed class IndexedBooleanEventArgs : System.EventArgs
	{
		private readonly int m_Index;
		private readonly bool m_Value;

		public int Index { get { return m_Index; } }
		public bool Value { get { return m_Value; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public IndexedBooleanEventArgs(int index, bool value)
		{
			m_Index = index;
			m_Value = value;
		}
	}
}