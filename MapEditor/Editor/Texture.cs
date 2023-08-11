using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.Celeste;
using Editor.Utils;
using System;
using System.IO;

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

        public string Name { get; private set; }

        public Point Size { get; private set; }
        public int Width => Size.X;
        public int Height => Size.Y;

        public Rectangle ClipRect { get; private set; }

        public Point DrawOffset { get; private set; }

        public Vector2 Origin = Vector2.Zero;
        public Vector2 Offset = Vector2.Zero;

        public float Rotation = 0f;

        public Texture(string name)
        {
            Name = name;
            Load();
            if (Image != null)
                Size = Image.Bounds.Size;
            ClipRect = new Rectangle(Point.Zero, Size);
        }

        public Texture(string name, Point size, Color color)
        {
            Name = name;
            Size = size;
            ClipRect = new Rectangle(Point.Zero, Size);
            Color = color;
            Load();
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
            DrawOffset = new Point(-Math.Min(x - parent.DrawOffset.X, 0), -Math.Min(y - parent.DrawOffset.Y, 0));
            Size = new Point(width, height);
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
        private void Load()
        {
            if (string.IsNullOrEmpty(Name))
            {
                Image = new(MapEditor.Instance.GraphicsDevice, Size.X, Size.Y);
                // Set the texture to be filled with 'color'
                Color[] data = new Color[Size.X * Size.Y];
                for (int i = 0; i < data.Length; i++)
                    data[i] = Color;
                Image.SetData(data);
            }
            else
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(Session.Current.CelesteContentDirectory, Name));
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

                Size = new Point(width, height);
                Image = new(MapEditor.Instance.GraphicsDevice, width, height);
                Image.SetData(buffer, 0, totalSize);
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

        public void JustifyOrigin(float x, float y)
            => Origin = new(Width * x, Height * y);

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset)
            => Render(spriteBatch, camera, offset, Color.White);

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset, Color color, float scale = 1f)
            => Render(spriteBatch, camera, offset, color, new Vector2(scale));

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset, Color color, Vector2 scale)
            => spriteBatch.Draw(
                Image,
                camera.MapToWindow(offset + Offset),
                ClipRect,
                color,
                Rotation,
                Origin - DrawOffset.ToVector2(),
                scale * camera.Zoom,
                SpriteEffects.None,
                0f
            );

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
