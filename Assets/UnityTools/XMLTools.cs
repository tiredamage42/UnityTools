using System.Collections.Generic;

using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Linq;


namespace UnityTools {

    public delegate void XMLWriteMethod (XmlWriter writer);
    
    public class XMLTools 
    {
        static XmlWriter CreateXmlWriter(string fileName, XmlWriterSettings settings)
		{
            return fileName != null ? XmlWriter.Create(fileName, settings) : null;
		}

        public static void CreateXML (string fileName, XMLWriteMethod writeMethod) {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
			xmlSettings.Encoding = Encoding.UTF8;
			xmlSettings.Indent = true;

			using(XmlWriter writer = CreateXmlWriter(fileName, xmlSettings))
			{
                if (writer != null) {
                    writer.WriteStartDocument(true);
                    writeMethod(writer);
                    writer.WriteEndDocument();
                }
			}
        }

        public static void WriteSection (string name, XmlWriter writer, XMLWriteMethod writeMethod, string[] attributeNames, string[] attributes) {
            writer.WriteStartElement(name);
            
            if (attributeNames != null) {
                for (int i = 0; i < attributeNames.Length; i++) 
                    writer.WriteAttributeString(attributeNames[i], attributes[i]);
            }
            
            writeMethod(writer);
			writer.WriteEndElement();
        }


        public static XmlDocument LoadXML(string fileName)
		{
			if(fileName != null)
			{
                XmlDocument doc;
				using(StreamReader reader = File.OpenText(fileName))
				{
					doc = new XmlDocument();
					doc.Load(reader);
				}
                return doc;
			}
			
			return null;
		}
        public static XmlDocument LoadXML(TextReader textReader)
		{
			
			if (textReader != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(textReader);
				return doc;
			}

			return null;
		}

        public static XmlNode SelectSingleNode(XmlNode parent, string name)
		{
			return parent.SelectSingleNode(name);
		}
		public static IEnumerable<XmlNode> SelectSubNodesByName(XmlNode parent, string name)
		{
			return parent.SelectNodes(name).Cast<XmlNode>();
		}

		public static string ReadNode(XmlNode node, string defValue = null)
		{
			return node != null ? node.InnerText : defValue;
		}
		public static string ReadAttribute(XmlNode node, string attribute, string defValue = null)
		{
			if(node.Attributes[attribute] != null) return node.Attributes[attribute].InnerText;
			return defValue;
		}
		public static int ReadAsInt(XmlNode node, int defValue = 0)
		{
			int value = 0;
			if(int.TryParse(node.InnerText, out value)) return value;
			return defValue;
		}
		public static float ReadAsFloat(XmlNode node, float defValue = 0.0f)
		{
			float value = 0;
			if(float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value;
			return defValue;
		}
		public static bool ReadAsBool(XmlNode node, bool defValue = false)
		{
			bool value = false;
			if(bool.TryParse(node.InnerText.ToLower(), out value)) return value;
			return defValue;
		}
        public static T ReadAsEnum<T> (XmlNode node, T defValue) {
            return SystemTools.StringToEnum(node.InnerText, defValue);
        }
                
    }
}
