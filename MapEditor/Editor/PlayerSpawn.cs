using Editor.Celeste;
using Editor.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using MonoGame.Extended;

namespace Editor
{
    public class PlayerSpawn : MapObject
    {
        // Need to name this differently because of MapObject.Size
        public static readonly Point SizeConst = new(13, 17);
        public static readonly Vector2 Offset = new(-SizeConst.X / 2, -SizeConst.Y);
        private static Texture Sprite;

        public PlayerSpawn(Level level, Vector2 offset) : base(level, offset)
        {
            Size = SizeConst;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera) => Sprite.Render(spriteBatch, camera, Position + Offset);

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new RectangleF(camera.MapToWindow(Position + Offset), Size.Mul(camera.Zoom)),
                Color.DarkGreen,
                camera.GetLineThickness()
            );

        internal static void SetupSprite() => Sprite = new(Atlas.Gameplay["characters/player/fallPose10"], 8, 15, SizeConst.X, SizeConst.Y);
    }
}
