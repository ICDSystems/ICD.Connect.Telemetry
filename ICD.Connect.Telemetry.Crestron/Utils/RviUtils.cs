using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Globalization;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Utils
{
	public static class RviUtils
	{
		// This gets added to the join number, cause Crestron
		private const int JOIN_OFFSET = 49;
		private const string ELEMENT_SLOT = "Slot";
		private const string ELEMENT_SLOT_NUMBER = "SlotNum";
		private const string ELEMENT_SLOT_NAME = "SlotName";

		private const string DIGITAL = "Digital";
		private const string ANALOG = "Analog";
		private const string SERIAL = "Serial";

		private static readonly IcdHashSet<IFusionRoom> s_FusionRooms;
		private static readonly SafeCriticalSection s_FusionRoomsSection;

		private static readonly KeyValuePair<SigInfo, eTelemetryIoMask>[] s_StaticAssetDefaultSigs = new[]
		{
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 1, "PowerIsOn", 0),
			                                            eTelemetryIoMask.BiDirectional),
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 2, "PowerOff", 0),
			                                            eTelemetryIoMask.ServiceToProgram),
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 3, "Connected", 0), 
			                                            eTelemetryIoMask.ProgramToService), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Serial, 1, "AssetUsage", 0),
			                                            eTelemetryIoMask.ProgramToService), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Serial, 2, "AssetError", 0),
			                                            eTelemetryIoMask.ProgramToService)
		};

		private static readonly KeyValuePair<SigInfo, eTelemetryIoMask>[] s_OccupancyAssetDefaultSigs = new[]
		{
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 1, "OccSensorEnabled", 0),
			                                            eTelemetryIoMask.BiDirectional ), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 2, "DisableOccSensor", 0),
			                                            eTelemetryIoMask.ServiceToProgram), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, 3, "RoomOccupied",0),
			                                            eTelemetryIoMask.BiDirectional), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Analog, 1, "OccSensorTimeout", 0), 
			                                            eTelemetryIoMask.BiDirectional), 
			new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Serial, 12, "RoomOccupancyInfo", 0), 
			                                            eTelemetryIoMask.BiDirectional), 
		};

		/// <summary>
		/// Gets the absolute path for the fusion RVI file.
		/// </summary>
		public static string RviPath
		{
			get
			{
				if (IcdEnvironment.RuntimeEnvironment == IcdEnvironment.eRuntimeEnvironment.Standard)
					return PathUtils.Join(PathUtils.ProgramConfigPath, "ICD.Connect.Core.rvi");

				if (PathUtils.ProgramFilePath != null)
					return IcdPath.ChangeExtension(PathUtils.ProgramFilePath, ".rvi");

				throw new NotSupportedException("ProgramFilePath is null");
			}
		}

		/// <summary>
		/// Static constructor.
		/// </summary>
		static RviUtils()
		{
			s_FusionRooms = new IcdHashSet<IFusionRoom>();
			s_FusionRoomsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Adds the fusion room for RVI generation.
		/// </summary>
		/// <param name="fusionRoom"></param>
		[PublicAPI]
		public static void RegisterFusionRoom([NotNull] IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			s_FusionRoomsSection.Execute(() => s_FusionRooms.Add(fusionRoom));
		}

		/// <summary>
		/// Removes the fusion room from RVI generation.
		/// </summary>
		/// <param name="fusionRoom"></param>
		[PublicAPI]
		public static void UnregisterFusionRoom([NotNull] IFusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			s_FusionRoomsSection.Execute(() => s_FusionRooms.Remove(fusionRoom));
		}

		/// <summary>
		/// Writes an RVI file for the registered fusion rooms.
		/// </summary>
		[PublicAPI]
		public static void GenerateFileForAllFusionDevices()
		{
			string file = RviPath;

			IcdConsole.PrintLine("Starting RVI Generation to: {0}", file);

			using (IcdFileStream stream = IcdFile.Open(file, eIcdFileMode.Create))
			{
				using (IcdXmlTextWriter writer = new IcdXmlTextWriter(stream, new UTF8Encoding(false)))
				{
					writer.WriteStartDocument();
					{
						WriteRoomViewInfo(writer);
					}
					writer.WriteEndDocument();
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Formats the number to a 2 character string with leading zeroes.
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		private static string GetSlotNumber(int number)
		{
			return string.Format("{0:D2}", number);
		}

		/// <summary>
		/// Writes the RoomViewInfo element and children.
		/// </summary>
		/// <param name="writer"></param>
		private static void WriteRoomViewInfo([NotNull] IcdXmlTextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			writer.WriteStartElement("RoomViewInfo");
			{
				// Write the local time in en-US format because Crestron...
				CultureInfo culture = IcdCultureInfo.CreateSpecificCulture("en-US");
				string date = IcdEnvironment.GetLocalTime().ToString(culture);
				writer.WriteElementString("TimeDateStamp", date);

				// Write the fusion rooms
				foreach (IFusionRoom fusionRoom in s_FusionRoomsSection.Execute(() => s_FusionRooms.OrderBy(f => f.IpId).ToArray()))
					WriteSymbolInfo(writer, fusionRoom);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes the SymbolInfo element and children.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="fusionRoom"></param>
		private static void WriteSymbolInfo([NotNull] IcdXmlTextWriter writer, [NotNull] IFusionRoom fusionRoom)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			writer.WriteStartElement("SymbolInfo");
			{
				writer.WriteElementString("Version", "8.1");
				writer.WriteElementString("RoomName", fusionRoom.RoomName);
				writer.WriteElementString("InstanceID", fusionRoom.RoomId);
				writer.WriteElementString("IPID", string.Format("{0:X2}", fusionRoom.IpId));

				// Room
				WriteFusionDigitalsSlot(writer, fusionRoom);
				WriteFusionAnalogsSlot(writer, fusionRoom);
				WriteFusionSerialsSlot(writer, fusionRoom);

				// Assets
				foreach (FusionAssetData assetData in fusionRoom.UserConfigurableAssetDetails)
					WriteAssetSlot(writer, assetData);

			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes the SignalPair element and children for the given values.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="join"></param>
		/// <param name="type"></param>
		/// <param name="label"></param>
		/// <param name="ioMask"></param>
		/// <param name="joinOffset"></param>
		private static void WriteSignalPair([NotNull] IcdXmlTextWriter writer, ushort join, string type, string label,
		                                    int ioMask, int joinOffset)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			writer.WriteStartElement("SignalPair");
			{
				writer.WriteElementString("Join", IcdXmlConvert.ToString(join + joinOffset));
				writer.WriteElementString("Type", type);
				writer.WriteElementString("Label", label);
				writer.WriteElementString("IOmask", IcdXmlConvert.ToString(ioMask));
			}
			writer.WriteEndElement();
		}

		private static void WriteSignalPair([NotNull] IcdXmlTextWriter writer, KeyValuePair<SigInfo, eTelemetryIoMask> sig)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			WriteSignalPair(writer, sig ,JOIN_OFFSET);
		}

		private static void WriteSignalPair([NotNull] IcdXmlTextWriter writer, KeyValuePair<SigInfo, eTelemetryIoMask> sig, int joinOffset)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			WriteSignalPair(writer, (ushort)sig.Key.Number, sig.Key.Type.ToString(), sig.Key.Name, (int)sig.Value, joinOffset);
		}

		#endregion

		#region Fusion Room

		/// <summary>
		/// Writes the Fusion Digitals Slot element and children.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="fusionRoom"></param>
		private static void WriteFusionDigitalsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] IFusionRoom fusionRoom)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(1));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Digitals");

				// Reserved
				WriteSignalPair(writer, 3, DIGITAL, "SystemPowerIsOn", (int)eTelemetryIoMask.BiDirectional, 0);
				WriteSignalPair(writer, 4, DIGITAL, "SystemPowerOff", (int)eTelemetryIoMask.ServiceToProgram, 0);
				WriteSignalPair(writer, 5, DIGITAL, "DisplayPowerIsOn", (int)eTelemetryIoMask.BiDirectional, 0);
				WriteSignalPair(writer, 6, DIGITAL, "DisplayPowerOff", (int)eTelemetryIoMask.ServiceToProgram, 0);
				WriteSignalPair(writer, 22, DIGITAL, "MsgBroadcastEnabled", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 30, DIGITAL, "AuthenticateSucceeded", (int)eTelemetryIoMask.ServiceToProgram, 0);
				WriteSignalPair(writer, 31, DIGITAL, "AuthenticateFailed", (int)eTelemetryIoMask.ServiceToProgram, 0);

				// Custom
				foreach (KeyValuePair<SigInfo, eTelemetryIoMask> sig in fusionRoom.GetUserDefinedBoolSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes the Fusion Analogs Slot element and children.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="fusionRoom"></param>
		private static void WriteFusionAnalogsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] IFusionRoom fusionRoom)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(2));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Analogs");

				// Reserved
				WriteSignalPair(writer, 2, ANALOG, "DisplayUsage", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 22, ANALOG, "BroadcastMsgType", (int)eTelemetryIoMask.ServiceToProgram, 0);

				// Custom
				foreach (KeyValuePair<SigInfo, eTelemetryIoMask> sig in fusionRoom.GetUserDefinedUShortSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes the Fusion Serials Slot element and children.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="fusionRoom"></param>
		private static void WriteFusionSerialsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] IFusionRoom fusionRoom)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (fusionRoom == null)
				throw new ArgumentNullException("fusionRoom");

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(3));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Serials");

				// Reserved
				WriteSignalPair(writer, 1, SERIAL, "HelpRequest", (int)eTelemetryIoMask.BiDirectional, 0);
				WriteSignalPair(writer, 2, SERIAL, "ErrorMessage", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 3, SERIAL, "LogText", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 5, SERIAL, "DeviceUsage", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 6, SERIAL, "TextMessage", (int)eTelemetryIoMask.BiDirectional, 0);
				WriteSignalPair(writer, 22, SERIAL, "BroadcastMsgResponse", (int)eTelemetryIoMask.BiDirectional, 0);
				WriteSignalPair(writer, 23, SERIAL, "FreeBusyStatus", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 30, SERIAL, "AuthenticateRequest", (int)eTelemetryIoMask.ProgramToService, 0);
				WriteSignalPair(writer, 31, SERIAL, "GroupMembership", (int)eTelemetryIoMask.ServiceToProgram, 0);

				// Custom
				foreach (KeyValuePair<SigInfo, eTelemetryIoMask> sig in fusionRoom.GetUserDefinedStringSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		#endregion

		#region Asset

		/// <summary>
		/// Writes the Slot element and children for the given asset.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="assetData"></param>
		private static void WriteAssetSlot([NotNull] IcdXmlTextWriter writer, [NotNull] FusionAssetData assetData)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (assetData == null)
				throw new ArgumentNullException("assetData");

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber((int)assetData.Number + 3));
				writer.WriteElementString(ELEMENT_SLOT_NAME, GetAssetSlotName(assetData.Type));
				WriteAssetParam(writer, "AssetName", eSigType.Serial, assetData.Asset.Name);
				WriteAssetParam(writer, "AssetType", eSigType.Serial, assetData.Asset.Type);
				WriteAssetParam(writer, "InstanceID", eSigType.Serial, assetData.Asset.InstanceId);
				
				if (assetData.Asset is IFusionStaticAsset)
				{
					WriteAssetParam(writer, "Make", eSigType.Serial, null);
					WriteAssetParam(writer, "Model", eSigType.Serial, null);
				}

				foreach (var sig in GetDefaultSigsForAsset(assetData))
					WriteSignalPair(writer, sig, 0);

				WriteAssetDigitalsSlot(writer, assetData);
				WriteAssetAnalogsSlot(writer, assetData);
				WriteAssetSerialsSlot(writer, assetData);

			}
			writer.WriteEndElement();
		}

		private static void WriteAssetDigitalsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] FusionAssetData assetData)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			if (assetData == null)
				throw new ArgumentNullException("assetData");

			IFusionStaticAsset asset = assetData.Asset as IFusionStaticAsset;
			if (asset == null)
				return;

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(1));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Generic Asset Digitals");

				foreach (var sig in asset.GetUserDefinedBooleanSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		private static void WriteAssetAnalogsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] FusionAssetData assetData)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			if (assetData == null)
				throw new ArgumentNullException("assetData");

			IFusionStaticAsset asset = assetData.Asset as IFusionStaticAsset;
			if (asset == null)
				return;

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(2));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Generic Asset Analogs");

				foreach (var sig in asset.GetUserDefinedUShortSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		private static void WriteAssetSerialsSlot([NotNull] IcdXmlTextWriter writer, [NotNull] FusionAssetData assetData)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			if (assetData == null)
				throw new ArgumentNullException("assetData");

			IFusionStaticAsset asset = assetData.Asset as IFusionStaticAsset;
			if (asset == null)
				return;

			writer.WriteStartElement(ELEMENT_SLOT);
			{
				writer.WriteElementString(ELEMENT_SLOT_NUMBER, GetSlotNumber(3));
				writer.WriteElementString(ELEMENT_SLOT_NAME, "Fusion Generic Asset Serials");

				foreach (var sig in asset.GetUserDefinedSerialSigs().OrderBy(kvp => kvp.Key.Number))
					WriteSignalPair(writer, sig);
			}
			writer.WriteEndElement();
		}

		private static void WriteAssetParam([NotNull] IcdXmlTextWriter writer, [NotNull] string name, eSigType type,
		                                    [CanBeNull] string value)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			if (name == null)
				throw new ArgumentNullException("name");

			writer.WriteStartElement("Param");
			{
				writer.WriteElementString("Name", name);
				writer.WriteElementString("DataType", type.ToString());
				writer.WriteElementString("Value", IcdXmlConvert.ToString(value));
			}
			writer.WriteEndElement();
		}

		private static string GetAssetSlotName(eAssetType type)
		{
			switch (type)
			{
				case eAssetType.StaticAsset:
					return "Fusion Static Asset";
				case eAssetType.OccupancySensor:
					return "Fusion Occupancy Sensor";
				default:
					throw new ArgumentOutOfRangeException("type", string.Format("{0} is not a handled asset type", type));
			}
		}

		private static IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetDefaultSigsForAsset([NotNull] FusionAssetData assetData)
		{
			if (assetData == null)
				throw new ArgumentNullException("assetData");

			switch (assetData.Type)
			{
				case eAssetType.StaticAsset:
					return s_StaticAssetDefaultSigs;
				case eAssetType.OccupancySensor:
					return s_OccupancyAssetDefaultSigs;
				default:
					throw new ArgumentOutOfRangeException("assetData");
			}
		}

		#endregion
	}
}
