using Editor.Celeste;
using Editor.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Numerics;

namespace Editor
{
    public class Texture
    {
        public const int ByteArraySize = 0x80000;
        public const int ByteArrayCheckSize = ByteArraySize - 0x20;

        private static byte[] buffer;
        private static byte[] buffer2;

        private readonly Image<Rgba32> fullImage;
        public Color Color;

        public Image<Rgba32> Image { get; private set; }

        public string Name { get; private set; }

        public Size Size { get; private set; }
        public int Width => Size.Width;
        public int Height => Size.Height;

        public Rectangle ClipRect { get; private set; }

        public Point DrawOffset { get; private set; }

        public Texture(string name)
        {
            Name = name;
            fullImage = Load();
            if (fullImage != null)
                Size = fullImage.Size;
            ClipRect = new Rectangle(Point.Empty, Size);
            SetupFinalImage();
        }

        public Texture(string name, Size size, Color color)
        {
            Name = name;
            Size = size;
            ClipRect = new Rectangle(Point.Empty, Size);
            Color = color;
            fullImage = Load();
            SetupFinalImage();
        }

        public Texture(
            Texture parent,
            Rectangle clipRect,
            Point drawOffset,
            Size size)
        {
            fullImage = parent.fullImage;
            ClipRect = parent.GetRelativeRect(clipRect);
            DrawOffset = drawOffset;
            Size = size;
            SetupFinalImage();
        }

        /// <summary>
        /// Tile constructor.
        /// </summary>
        /// <param name="tileset">The tileset texture.</param>
        /// <param name="x">The X offset in pixels.</param>
        /// <param name="y">The Y offset in pixels.</param>
        public Texture(
            Texture tileset,
            int x,
            int y)
        {
            fullImage = tileset.fullImage;
            ClipRect = tileset.GetRelativeRect(x, y, Tileset.TileSize, Tileset.TileSize);
            DrawOffset = new Point(-Math.Min(x - tileset.DrawOffset.X, 0), -Math.Min(y - tileset.DrawOffset.Y, 0));
            Size = new Size(Tileset.TileSize);
            SetupFinalImage();
        }

        public void Unload()
        {
            fullImage?.Mutate(i => i.Crop(1, 1).Clear(Color.Black));
        }

        /// <summary>
        /// Most of the code in this function comes directly from Celeste so
        /// some variables might have an inaccurate name.
        /// </summary>
        private Image<Rgba32> Load()
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
                fileStream.Read(buffer2, 0, ByteArraySize);

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
                    if (bytesIndex > ByteArrayCheckSize)
                    {
                        int offset = ByteArraySize - bytesIndex;
                        for (int index5 = 0; index5 < offset; ++index5)
                            buffer2[index5] = buffer2[bytesIndex + index5];
                        fileStream.Read(buffer2, offset, ByteArraySize - offset);
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
            int relativeX = ClipRect.X - DrawOffset.X + x;
            int relativeY = ClipRect.Y - DrawOffset.Y + y;

            int resultX = Calc.Clamp(relativeX, ClipRect.Left, ClipRect.Right);
            int resultY = Calc.Clamp(relativeY, ClipRect.Top, ClipRect.Bottom);
            int resultW = Math.Max(0, Math.Min(relativeX + width, ClipRect.Right) - resultX);
            int resultH = Math.Max(0, Math.Min(relativeY + height, ClipRect.Bottom) - resultY);

            return new Rectangle(resultX, resultY, resultW, resultH);
        }

        public static void AllocateBuffers()
        {
            buffer = new byte[0x04000000];
            buffer2 = new byte[ByteArraySize];
        }

        public static void DeallocateBuffers()
        {
            buffer = null;
            buffer2 = null;
        }

        private void SetupFinalImage() => Image = fullImage?.Clone(o => o.Crop(ClipRect));
    }
}
