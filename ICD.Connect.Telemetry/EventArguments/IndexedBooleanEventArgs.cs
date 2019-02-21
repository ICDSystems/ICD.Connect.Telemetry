namespace ICD.Connect.Telemetry.EventArguments
{
	public class IndexedBooleanEventArgs : System.EventArgs
	{
		public int Index { get; private set; }
		public bool Value { get; private set; }

		public IndexedBooleanEventArgs(int index, bool value)
		{
			Index = index;
			Value = value;
		}
	}
}