namespace ICD.Connect.Telemetry.EventArguments
{
	public class IndexedUshortEventArgs : System.EventArgs
	{
		public int Index { get; private set; }
		public ushort Value { get; private set; }

		public IndexedUshortEventArgs(int index, ushort value)
		{
			Index = index;
			Value = value;
		}
	}
}