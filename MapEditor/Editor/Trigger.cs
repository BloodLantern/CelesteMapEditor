using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Editor
{
    public class Trigger
    {
        public static readonly Color DefaultColor = Color.MediumPurple;

        public readonly EntityData EntityData;
        public readonly Level Level;

        public string Name => EntityData.Name;
        public readonly string DisplayName;

        public Vector2 Position => EntityData.Position;
        public Vector2 AbsolutePosition => Level.Position.ToVector2() + Position;
        public virtual Point Size => EntityData.Size;

        public RectangleF Bounds => new(AbsolutePosition, Size);

        public Trigger(EntityData data, Level level)
        {
            EntityData = data;
            Level = level;

            DisplayName = Calc.HumanizeString(Name);
        }

        public virtual void Render(SpriteBatch spriteBatch, Camera camera)
        {
            SpriteFont font = Session.Current.PixelatedFont;
            float stringScale = camera.GetStringScale();
            string str = DisplayName;
            int strIndex = 0, strLength = str.Length;
            Vector2 strSize;
            Vector2 drawOffset = Vector2.Zero;

            while (true)
            {
                // Try to divide the string to make it fit on the X axis
                strSize = font.MeasureString(str);
                if (strSize.X > Size.X && str.Length != 1)
                {
                    str = DisplayName.Substring(strIndex, strLength /= 2);
                    continue;
                }
                else if (strSize.X < Size.X) // Add a letter if it is not wide enough
                {
                    // TODO
                }

                // Then render all the divided parts
                spriteBatch.DrawString(font, str, camera.MapToWindow(AbsolutePosition) + drawOffset, Color.White, 0f, -Size.ToVector2() / 2 + strSize / 2, stringScale, SpriteEffects.None, 0f);
                drawOffset.Y += strSize.Y;

                strIndex += strLength;
                // Stop if the current part is the last one
                if (strIndex + strLength >= DisplayName.Length)
                    break;
                str = DisplayName.Substring(strIndex, strLength);
            }

            //spriteBatch.FillRectangle(camera.MapToWindow(Bounds), DefaultColor);
            spriteBatch.DrawRectangle(camera.MapToWindow(Bounds), DefaultColor, camera.GetLineThickness());
        }
    }
}
