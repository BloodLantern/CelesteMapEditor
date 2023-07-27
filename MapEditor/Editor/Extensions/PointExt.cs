using Microsoft.Xna.Framework;

namespace Editor.Extensions
{
    public static class PointExt
    {
        public static Point Mul(this Point self, float factor) => new((int)(self.X * factor), (int)(self.Y * factor));

        public static Point Div(this Point self, float factor) => new((int)(self.X / factor), (int)(self.Y / factor));
    }
}
