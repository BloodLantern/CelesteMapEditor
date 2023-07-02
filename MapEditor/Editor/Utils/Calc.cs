using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

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
    }
}
