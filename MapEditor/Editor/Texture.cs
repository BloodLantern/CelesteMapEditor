using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.Celeste;
using Editor.Utils;
using System;
using System.IO;
using ImGuiNET;
using Editor.Logging;

namespace Editor
{
    public class Texture
    {
        public const int ByteArraySize = 0x80000;
        public const int ByteArrayCheckSize = ByteArraySize - 0x20;

        private static byte[] buffer;
        private static byte[] buffer2;

        public Color Color;

        public Texture2D Image;

        public string Name { get; }

        public Point Size { get; private set; }
        public int Width => Size.X;
        public int Height => Size.Y;

        public Rectangle ClipRect { get; }

        public Point DrawOffset { get; }

        public Vector2 Origin = Vector2.Zero;
        public Vector2 Offset = Vector2.Zero;

        public float Rotation = 0f;

        public Texture(string directory, string name)
        {
            Name = name;
            Load(directory);
            if (Image != null)
                Size = Image.Bounds.Size;
            ClipRect = new(Point.Zero, Size);
        }

        public Texture(Stream stream, string extension, string name)
        {
            Name = Path.ChangeExtension(name, null);
            Load(stream, extension);
            if (Image != null)
                Size = Image.Bounds.Size;
            ClipRect = new(Point.Zero, Size);
        }

        public Texture(string directory, string name, Point size, Color color)
        {
            Name = name;
            Size = size;
            ClipRect = new(Point.Zero, Size);
            Color = color;
            Load(directory);
        }

        public Texture(
            Texture parent,
            Rectangle clipRect,
            Point drawOffset,
            Point? size = null)
        {
            Image = parent.Image;
            ClipRect = parent.GetRelativeRect(clipRect);
            DrawOffset = drawOffset;
            Size = size ?? ClipRect.Size;
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
            : this(tileset, x, y, Tileset.TileSize, Tileset.TileSize)
        {
        }

        /// <summary>
        /// Subtexture constructor.
        /// </summary>
        /// <param name="parent">The parent texture.</param>
        /// <param name="x">The X offset in pixels.</param>
        /// <param name="y">The Y offset in pixels.</param>
        public Texture(
            Texture parent,
            int x,
            int y,
            int width,
            int height)
        {
            Image = parent.Image;
            ClipRect = parent.GetRelativeRect(x, y, width, height);
            DrawOffset = new(-Math.Min(x - parent.DrawOffset.X, 0), -Math.Min(y - parent.DrawOffset.Y, 0));
            Size = new(width, height);
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">A texture to copy the data from.</param>
        public Texture(Texture other)
        {
            Image = other.Image;
            ClipRect = other.ClipRect;
            DrawOffset = other.DrawOffset;
            Size = other.Size;
        }

        public Texture(Texture2D texture)
        {
            Image = texture;
            ClipRect = new(0, 0, Image.Width, Image.Height);
            DrawOffset = Point.Zero;
            Size = ClipRect.Size;
        }

        public void Dispose() => Image.Dispose();

        /// <summary>
        /// Most of the code in this function comes directly from Celeste so
        /// some variables might have an inaccurate name.
        /// </summary>
        private void Load(string directory)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Image = new(Application.Instance.GraphicsDevice, Size.X, Size.Y);
                // Set the texture to be filled with 'color'
                Color[] data = new Color[Size.X * Size.Y];
                for (int i = 0; i < data.Length; i++)
                    data[i] = Color;
                Image.SetData(data);
            }
            else
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(directory, Name));
                Load(fileStream, Path.GetExtension(Name));
            }
        }

        private void Load(Stream stream, string extension)
        {
            switch (extension)
            {
                case ".data":
                    stream.Read(buffer2, 0, ByteArraySize);

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
                            for (int i = 0; i < offset; i++)
                                buffer2[i] = buffer2[bytesIndex + i];
                            stream.Read(buffer2, offset, ByteArraySize - offset);
                            bytesIndex = 0;
                        }
                    }

                    Size = new(width, height);
                    Image = new(Application.Instance.GraphicsDevice, width, height);
                    Image.SetData(buffer, 0, totalSize);
                    break;

                case ".png":
                    try
                    {
                        Image = Texture2D.FromStream(Application.Instance.Graphics.GraphicsDevice, stream);
                        int elementCount = Image.Width * Image.Height;
                        Color[] data = new Color[elementCount];
                        Image.GetData(data, 0, elementCount);
                        for (int i = 0; i < elementCount; i++)
                        {
                            data[i].R = (byte) (data[i].R * (data[i].A / (float) byte.MaxValue));
                            data[i].G = (byte) (data[i].G * (data[i].A / (float) byte.MaxValue));
                            data[i].B = (byte) (data[i].B * (data[i].A / (float) byte.MaxValue));
                        }
                        Image.SetData(data, 0, elementCount);
                    }
                    catch (InvalidOperationException e)
                    {
                        Logger.Log($"Couldn't load texture: {Name}, error: {e}", LogLevel.Error);
                    }
                    break;
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

            return new(resultX, resultY, resultW, resultH);
        }

        public void JustifyOrigin(float x, float y)
            => Origin = new(Width * x, Height * y);

        public void JustifyOrigin(Vector2 xy)
            => Origin = new(Width * xy.X, Height * xy.Y);

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset)
            => Render(spriteBatch, camera, offset, Color.White);

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset, Color color, float scale = 1f)
            => Render(spriteBatch, camera, offset, color, new Vector2(scale));

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset, Color color, Vector2 scale)
        {
            if ((Size.ToVector2() * camera.Zoom).Length() < 1f)
                return;

            spriteBatch.Draw(
                        Image,
                        camera.MapPositionToWindow(offset + Offset),
                        ClipRect,
                        color,
                        Rotation,
                        Origin - DrawOffset.ToVector2(),
                        scale * camera.Zoom,
                        SpriteEffects.None,
                        0f
                    );
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
    }
}
