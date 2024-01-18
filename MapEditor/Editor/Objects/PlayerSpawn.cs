using Editor.Celeste;
using Editor.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using MonoGame.Extended;

namespace Editor.Objects
{
    public class PlayerSpawn : MapObject
    {
        public static readonly Point SizeConst = new(13, 17);
        public static readonly Vector2 Offset = new(-SizeConst.X / 2, -SizeConst.Y);
        private static Texture sprite;

        public override Point Size => SizeConst;

        public PlayerSpawn(Level level, Vector2 offset) : base(level, offset + Offset, MapObjectType.PlayerSpawn)
        {
        }

        public void Render(SpriteBatch spriteBatch, Camera camera) => sprite.Render(spriteBatch, camera, AbsolutePosition);

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new RectangleF(camera.MapPositionToWindow(Position + Offset), Size.Mul(camera.Zoom)),
                Color.DarkGreen,
                camera.GetLineThickness()
            );

        internal static void SetupSprite() => sprite = new(Atlas.Gameplay["characters/player/fallPose10"], 8, 15, SizeConst.X, SizeConst.Y);
    }
}
