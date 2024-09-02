using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Editor
{
    public class Filler
    {
        public const string DefaultDisplayName = "FILLER";

        public readonly string DisplayName;
        public Rectangle Bounds;
        public bool Selected;

        public Point Center => Bounds.Center;

        public Filler(Rectangle bounds, int index)
        {
            Bounds = bounds;
            DisplayName = DefaultDisplayName + index;
        }

        public Filler(int x, int y, int width, int height, int index)
            : this(new(x, y, width, height), index)
        {
        }

        public static implicit operator Rectangle(Filler filler) => filler.Bounds;

        public static implicit operator RectangleF(Filler filler) => filler.Bounds;
    }
}
