using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Editor
{
    public static class Calc
    {
        public static Random Random = new();

        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((ch = stream.ReadChar()) != char.MinValue)
                str += ch.ToString();
            return str;
        }

        public static System.Drawing.Bitmap ToBitmap(this Image<Rgba32> image)
        {
            using (MemoryStream memoryStream = new())
            {
                image.SaveAsBmp(memoryStream);
                return new System.Drawing.Bitmap(memoryStream);
            };
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static Point Position(this Rectangle self)
        {
            return new Point(self.X, self.Y);
        }

        public static Size Size(this Rectangle self)
        {
            return new Size(self.Width, self.Height);
        }

        public static PointF Position(this RectangleF self)
        {
            return new PointF(self.X, self.Y);
        }

        public static SizeF Size(this RectangleF self)
        {
            return new SizeF(self.Width, self.Height);
        }

        public static int Round(float value)
        {
            return (int) MathF.Round(value);
        }

        public static XmlDocument LoadContentXML(string filename)
        {
            XmlDocument xmlDocument = new();
            using (StreamReader inStream = new(Path.Combine(Session.CurrentSession.CelesteContentDirectory, filename)))
                xmlDocument.Load(inStream);
            return xmlDocument;
        }

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

        public static T Choose<T>(this Random random, List<T> choices) => choices[random.Next(choices.Count)];
    }
}
