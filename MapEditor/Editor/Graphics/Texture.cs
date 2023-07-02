using Editor.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Editor.Graphics
{
    public class Texture
    {
        private Image<Rgba32> image;
        public Color Color;

        public Image<Rgba32> Image
        {
            get => image?.Clone(i => i.Crop(Rectangle.FromLTRB(ClipRect.Left + DrawOffset.X, ClipRect.Top + DrawOffset.Y, ClipRect.Right, ClipRect.Bottom)));
        }

        public string Name { get; private set; }

        public Size Size { get; private set; }

        public Rectangle ClipRect { get; private set; }

        public Point DrawOffset { get; private set; }

        public Texture(string celesteContentDirectory, string name, ref byte[] buffer, ref byte[] buffer2)
        {
            Name = name;
            image = Load(celesteContentDirectory, ref buffer, ref buffer2);
            if (image != null)
                Size = image.Size;
            ClipRect = new Rectangle(Point.Empty, Size);
        }

        public Texture(string celesteContentDirectory, string name, Size size, Color color, ref byte[] buffer, ref byte[] buffer2)
        {
            Name = name;
            Size = size;
            ClipRect = new Rectangle(Point.Empty, Size);
            Color = color;
            image = Load(celesteContentDirectory, ref buffer, ref buffer2);
        }

        public Texture(
            Texture parent,
            Rectangle clipRect,
            Point drawOffset,
            Size size)
        {
            image = parent.image;
            ClipRect = parent.GetRelativeRect(clipRect);
            DrawOffset = drawOffset;
            Size = size;
        }

        public void Unload()
        {
            image?.Mutate(i => i.Crop(1, 1).Clear(Color.Black));
        }

        /// <summary>
        /// Most of the code in this function comes directly from Celeste so
        /// some variables might have an inaccurate name.
        /// </summary>
        /// <param name="buffer">Byte buffer to store the image data.</param>
        /// <param name="buffer2">Byte buffer to store the image file data.</param>
        private Image<Rgba32> Load(string celesteContentDirectory, ref byte[] buffer, ref byte[] buffer2)
        {
            Unload();

            if (string.IsNullOrEmpty(Name))
            {
                // Set the texture to be filled with 'color'
                Rgba32[] data = new Rgba32[Size.Width * Size.Height];
                for (int i = 0; i < data.Length; i++)
                    data[i] = Color;
                return SixLabors.ImageSharp.Image.LoadPixelData(new ReadOnlySpan<Rgba32>(data), Size.Width, Size.Height);
            }
            else
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(Session.CurrentSession.CelesteContentDirectory, Name));
                fileStream.Read(buffer2, 0, Atlas.ByteArraySize);

                int width = BitConverter.ToInt32(buffer2, 0);
                int height = BitConverter.ToInt32(buffer2, 4);
                bool flag = buffer2[8] == 1;

                int bytesIndex = 9;
                int totalSize = width * height * 4;
                int bufferIndex = 0;
                while (bufferIndex < totalSize)
                {
                    int blockSize = buffer2[bytesIndex] * 4;
                    if (flag)
                    {
                        byte num2 = buffer2[bytesIndex + 1];
                        if (num2 > 0)
                        {
                            buffer[bufferIndex] = buffer2[bytesIndex + 4];
                            buffer[bufferIndex + 1] = buffer2[bytesIndex + 3];
                            buffer[bufferIndex + 2] = buffer2[bytesIndex + 2];
                            buffer[bufferIndex + 3] = num2;
                            bytesIndex += 5;
                        }
                        else
                        {
                            buffer[bufferIndex] = 0;
                            buffer[bufferIndex + 1] = 0;
                            buffer[bufferIndex + 2] = 0;
                            buffer[bufferIndex + 3] = 0;
                            bytesIndex += 2;
                        }
                    }
                    else
                    {
                        buffer[bufferIndex] = buffer2[bytesIndex + 3];
                        buffer[bufferIndex + 1] = buffer2[bytesIndex + 2];
                        buffer[bufferIndex + 2] = buffer2[bytesIndex + 1];
                        buffer[bufferIndex + 3] = byte.MaxValue;
                        bytesIndex += 4;
                    }
                    if (blockSize > 4)
                    {
                        int newBufferIndex = bufferIndex + 4;
                        for (int i = bufferIndex + blockSize; newBufferIndex < i; newBufferIndex += 4)
                        {
                            buffer[newBufferIndex] = buffer[bufferIndex];
                            buffer[newBufferIndex + 1] = buffer[bufferIndex + 1];
                            buffer[newBufferIndex + 2] = buffer[bufferIndex + 2];
                            buffer[newBufferIndex + 3] = buffer[bufferIndex + 3];
                        }
                    }
                    bufferIndex += blockSize;
                    if (bytesIndex > Atlas.ByteArrayCheckSize)
                    {
                        int offset = Atlas.ByteArraySize - bytesIndex;
                        for (int index5 = 0; index5 < offset; ++index5)
                            buffer2[index5] = buffer2[bytesIndex + index5];
                        fileStream.Read(buffer2, offset, Atlas.ByteArraySize - offset);
                        bytesIndex = 0;
                    }
                }

                Size = new Size(width, height);
                return SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(buffer, width, height);
            }
        }

        public Rectangle GetRelativeRect(Rectangle rect) => GetRelativeRect(rect.X, rect.Y, rect.Width, rect.Height);

        public Rectangle GetRelativeRect(int x, int y, int width, int height)
        {
            int num1 = ClipRect.X - DrawOffset.X + x;
            int num2 = ClipRect.Y - DrawOffset.Y + y;

            int resultX = MathHelper.Clamp(num1, ClipRect.Left, ClipRect.Right);
            int resultY = MathHelper.Clamp(num2, ClipRect.Top, ClipRect.Bottom);
            int resultW = Math.Max(0, Math.Min(num1 + width, ClipRect.Right) - resultX);
            int resultH = Math.Max(0, Math.Min(num2 + height, ClipRect.Bottom) - resultY);

            return new Rectangle(resultX, resultY, resultW, resultH);
        }
    }
}
