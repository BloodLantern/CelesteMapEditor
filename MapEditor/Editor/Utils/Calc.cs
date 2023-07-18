using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static Point[] ToPointArray(this Rectangle self)
        {
            Point position = self.Position();
            return new Point[] {
                position,
                position + new Size(self.Width, 0),
                position + self.Size(),
                position + new Size(0, self.Height)
            };
        }

        public static PointF[] ToPointFArray(this Rectangle self)
        {
            PointF position = self.Position();
            return new PointF[] {
                position,
                position + new SizeF(self.Width, 0),
                position + self.Size(),
                position + new SizeF(0, self.Height)
            };
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

        public static T Choose<T>(this Random random, List<T> choices) => choices[random.Next(choices.Count)];
    }
}
