using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Text;

namespace Editor.Utils
{
    public static class XmlSerializationHelper
    {
        public static T LoadFromXml<T>(this string xmlString, XmlSerializer serial = null)
        {
            serial ??= new XmlSerializer(typeof(T));
            T returnValue = default;
            using (StringReader reader = new(xmlString))
            {
                object result = serial.Deserialize(reader);
                if (result is T t)
                    returnValue = t;
            }
            return returnValue;
        }

        public static string GetXml<T>(this T obj, bool omitStandardNamespaces) => obj.GetXml(null, omitStandardNamespaces);

        public static string GetXml<T>(this T obj, XmlSerializer serializer = null, bool omitStandardNamespaces = false)
        {
            XmlSerializerNamespaces ns = null;
            if (omitStandardNamespaces)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add("", ""); // Disable the xmlns:xsi and xmlns:xsd lines.
            }
            using var textWriter = new StringWriter();
            var settings = new XmlWriterSettings() { Indent = true, IndentChars = "    ", Encoding = Encoding.UTF8 }; // For cosmetic purposes.
            using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                (serializer ?? new XmlSerializer(obj.GetType())).Serialize(xmlWriter, obj, ns);
            return textWriter.ToString();
        }
    }
}
