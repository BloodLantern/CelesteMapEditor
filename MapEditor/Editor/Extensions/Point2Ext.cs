using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Editor.Extensions
{
    public static class Point2Ext
    {
        public static Point ToPoint(this Point2 self) => new((int)self.X, (int)self.Y);
    }
}
