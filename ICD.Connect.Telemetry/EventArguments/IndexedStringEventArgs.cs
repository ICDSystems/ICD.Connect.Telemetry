namespace ICD.Connect.Telemetry.EventArguments
{
	public class IndexedStringEventArgs : System.EventArgs
	{
		public int Index { get; private set; }
		public string Value { get; private set; }

		public IndexedStringEventArgs(int index, string value)
		{
			Index = index;
			Value = value;
		}
	}
}