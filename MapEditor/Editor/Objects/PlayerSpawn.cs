using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Editor.Objects
{
    public class PlayerSpawn : MapObject
    {
        public static readonly Vector2 SizeConst = new(13f, 17f);
        public static readonly Vector2 Offset = new(-SizeConst.X * 0.5f, -SizeConst.Y);
        private static Texture sprite;

        public override Vector2 Size => SizeConst;

        public PlayerSpawn(Level level, Vector2 offset) : base(level, offset + Offset, MapObjectType.PlayerSpawn)
        {
        }

        public void Render(SpriteBatch spriteBatch, Camera camera) => sprite.Render(spriteBatch, camera, AbsolutePosition);

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new(camera.MapPositionToWindow(Position + Offset), Size * camera.Zoom),
                Color.DarkGreen,
                camera.GetLineThickness()
            );

        internal static void SetupSprite() => sprite = new(Atlas.Gameplay["characters/player/fallPose10"], 8, 15, (int) SizeConst.X, (int) SizeConst.Y);
    }
}
