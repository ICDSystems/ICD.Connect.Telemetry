using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public abstract class AbstractMockFusionAsset: IMockFusionAsset
	{

		protected AbstractMockFusionAsset(uint number, string name, string type, string instanceId)
		{
			Name = name;
			Type = type;
			InstanceId = instanceId;
			Number = number;
		}

		public abstract eAssetType AssetType { get; }

		/// <summary>
		/// Gets the name of the asset.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the user defined name of the asset type.
		/// </summary>
		public string Type { get; private set; }

		/// <summary>
		/// Gets the user defined instance id of the asset type.
		/// </summary>
		public string InstanceId { get; private set; }

		/// <summary>
		/// Gets the user defined number of the asset type.
		/// </summary>
		public uint Number { get; private set; }

		public FusionAssetData GetFusionAssetData()
		{
			return new FusionAssetData(Number, AssetType, this);
		}

		#region Console

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return string.Format("{0}-{1}", Name, AssetType.ToString()); } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return ""; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Name", Name);
			addRow("Number", Number);
			addRow("Type", Type);
			addRow("InstanceId", InstanceId);
			addRow("AssetType", AssetType);

		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}

		#endregion
	}
}