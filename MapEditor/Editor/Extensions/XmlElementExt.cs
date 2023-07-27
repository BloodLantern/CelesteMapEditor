using System;
using System.Globalization;
using System.Xml;

namespace Editor.Extensions
{
    public static class XmlElementExt
    {
        public static bool HasAttr(this XmlElement xml, string attributeName)
            => xml.Attributes[attributeName] != null;

        public static string Attr(this XmlElement xml, string attributeName)
            => xml.Attributes[attributeName].InnerText;

        public static string Attr(this XmlElement xml, string attributeName, string defaultValue)
            => !xml.HasAttr(attributeName) ? defaultValue : xml.Attributes[attributeName].InnerText;

        public static int AttrInt(this XmlElement xml, string attributeName)
            => Convert.ToInt32(xml.Attributes[attributeName].InnerText);

        public static int AttrInt(this XmlElement xml, string attributeName, int defaultValue)
            => !xml.HasAttr(attributeName) ? defaultValue : Convert.ToInt32(xml.Attributes[attributeName].InnerText);

        public static float AttrFloat(this XmlElement xml, string attributeName)
            => Convert.ToSingle(xml.Attributes[attributeName].InnerText, CultureInfo.InvariantCulture);

        public static float AttrFloat(this XmlElement xml, string attributeName, float defaultValue)
            => !xml.HasAttr(attributeName) ? defaultValue : Convert.ToSingle(xml.Attributes[attributeName].InnerText, CultureInfo.InvariantCulture);

        public static bool AttrBool(this XmlElement xml, string attributeName)
            => Convert.ToBoolean(xml.Attributes[attributeName].InnerText);

        public static bool AttrBool(this XmlElement xml, string attributeName, bool defaultValue)
            => !xml.HasAttr(attributeName) ? defaultValue : xml.AttrBool(attributeName);

        public static char AttrChar(this XmlElement xml, string attributeName)
            => Convert.ToChar(xml.Attributes[attributeName].InnerText);

        public static char AttrChar(this XmlElement xml, string attributeName, char defaultValue)
            => !xml.HasAttr(attributeName) ? defaultValue : xml.AttrChar(attributeName);
    }
}
