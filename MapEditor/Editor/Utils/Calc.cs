using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.CodeDom;
using System.IO;
using System.Runtime.CompilerServices;

namespace Editor
{
    public static class Calc
    {
        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((ch = stream.ReadChar()) != char.MinValue)
                str += ch.ToString();
            return str;
        }

        public static System.Drawing.Bitmap ToBitmap<TPixel>(this Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using MemoryStream memoryStream = new();
            IImageEncoder imageEncoder = image.GetConfiguration().ImageFormatsManager.GetEncoder(PngFormat.Instance);
            image.Save(memoryStream, imageEncoder);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return new System.Drawing.Bitmap(memoryStream);
        }

        public static float Sqrt(float x)
        {
            return (float) Math.Sqrt(x);
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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
    }
}
