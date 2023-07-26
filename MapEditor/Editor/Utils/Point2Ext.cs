using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Editor.Utils
{
    public static class Point2Ext
    {
        public static Point ToPoint(this Point2 self) => new((int) self.X, (int) self.Y);
    }
}
