namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public struct AssetInfo
	{
		private readonly eAssetType m_AssetType;
		private readonly uint m_Number;
		private readonly string m_Name;
		private readonly string m_Type;
		private readonly string m_InstanceId;

		private readonly string m_Make;
		private readonly string m_Model;

		public eAssetType AssetType { get { return m_AssetType; } }
		public uint Number { get { return m_Number; } }
		public string Name { get { return m_Name; } }
		public string Type { get { return m_Type; } }
		public string InstanceId { get { return m_InstanceId; } }
		public string Make {get { return m_Make; }}
		public string Model {get { return m_Model; }}

		public AssetInfo(eAssetType assetType, uint number, string name, string type, string instanceId, string make, string model)
		{
			m_AssetType = assetType;
			m_Number = number;
			m_Name = name;
			m_Type = type;
			m_InstanceId = instanceId;
			m_Make = make;
			m_Model = model;
		}
	}
}