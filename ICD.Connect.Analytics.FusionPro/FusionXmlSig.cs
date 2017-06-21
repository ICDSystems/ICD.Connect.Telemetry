using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.CrestronXml;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Analytics.FusionPro
{
	/// <summary>
	/// Parses to/from xml fusion sigs.
	/// </summary>
	public struct FusionXmlSig
	{
		private const string SIG_ELEMENT = "Sig";
		private const string NUMBER_ATTRIBUTE = "number";
		private const string NAME_ELEMENT = "Name";
		private const string TYPE_ELEMENT = "Type";
		private const string MASK_ELEMENT = "Mask";

		private readonly uint m_Number;
		private readonly string m_Name;
		private readonly eSigType m_SigType;
		private readonly eSigIoMask m_SigMask;

		#region Properties

		/// <summary>
		/// Gets the sig number.
		/// </summary>
		public uint Number { get { return m_Number; } }

		/// <summary>
		/// Gets the sig name.
		/// </summary>
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Gets the sig type.
		/// </summary>
		[PublicAPI]
		public eSigType SigType { get { return m_SigType; } }

		/// <summary>
		/// Gets the sig type in crestron format.
		/// </summary>
		public Crestron.SimplSharpPro.eSigType CrestronSigType
		{
			get
			{
				switch (m_SigType)
				{
					case eSigType.Na:
						return Crestron.SimplSharpPro.eSigType.NA;
					case eSigType.Digital:
						return Crestron.SimplSharpPro.eSigType.Bool;
					case eSigType.Analog:
						return Crestron.SimplSharpPro.eSigType.UShort;
					case eSigType.Serial:
						return Crestron.SimplSharpPro.eSigType.String;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Gets the sig mask.
		/// </summary>
		[PublicAPI]
		public eSigIoMask SigMask { get { return m_SigMask; } }

		/// <summary>
		/// Gets the sig mask in crestron format.
		/// </summary>
		public Crestron.SimplSharpPro.eSigIoMask CrestronSigMask
		{
			get
			{
				switch (m_SigMask)
				{
					case eSigIoMask.Na:
						return Crestron.SimplSharpPro.eSigIoMask.NA;
					case eSigIoMask.OutputSigOnly:
						return Crestron.SimplSharpPro.eSigIoMask.OutputSigOnly;
					case eSigIoMask.InputSigOnly:
						return Crestron.SimplSharpPro.eSigIoMask.InputSigOnly;
					case eSigIoMask.InputOutputSig:
						return Crestron.SimplSharpPro.eSigIoMask.InputOutputSig;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="mask"></param>
		public FusionXmlSig(uint number, string name, eSigType type, eSigIoMask mask)
		{
			m_Number = number;
			m_Name = name;
			m_SigType = type;
			m_SigMask = mask;
		}

		#region Methods

		/// <summary>
		/// Generates sig instances from a Sigs xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static IEnumerable<FusionXmlSig> SigsFromXml(string xml)
		{
			return XmlUtils.GetChildElementsAsString(xml).Select(element => FromXml(element));
		}

		/// <summary>
		/// Creates an instance from a Sig xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI]
		public static FusionXmlSig FromXml(string xml)
		{
			uint number = (uint)XmlUtils.GetAttributeAsInt(xml, NUMBER_ATTRIBUTE);
			string name = XmlUtils.ReadChildElementContentAsString(xml, NAME_ELEMENT);
			string typeString = XmlUtils.ReadChildElementContentAsString(xml, TYPE_ELEMENT);
			string maskString = XmlUtils.ReadChildElementContentAsString(xml, MASK_ELEMENT);

			eSigType type = EnumUtils.Parse<eSigType>(typeString, true);
			eSigIoMask mask = EnumUtils.Parse<eSigIoMask>(maskString, true);

			return new FusionXmlSig(number, name, type, mask);
		}

		/// <summary>
		/// Writes the sig to xml.
		/// </summary>
		/// <param name="writer"></param>
		[PublicAPI]
		public void ToXml(XmlWriter writer)
		{
			writer.WriteStartElement(SIG_ELEMENT);
			{
				writer.WriteAttributeString(NUMBER_ATTRIBUTE, XmlConvert.ToString(m_Number));

				writer.WriteElementString(NAME_ELEMENT, m_Name);

				// ReSharper disable ImpureMethodCallOnReadonlyValueField
				writer.WriteElementString(TYPE_ELEMENT, m_SigType.ToString());
				writer.WriteElementString(MASK_ELEMENT, m_SigMask.ToString());
				// ReSharper restore ImpureMethodCallOnReadonlyValueField
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Returns the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("{0}(Number: {1}, Name: {2}, Type: {3}, Mask: {4})",
			                     GetType().Name, Number, Name, SigType, SigMask);
		}

		#endregion
	}
}
