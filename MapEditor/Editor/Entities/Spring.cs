using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Entities
{
    public class Spring : Entity
    {
        public float Rotation = 0f;

        public Vector2 Offset = new(Tileset.TileSize);

        public Spring(EntityData data, Level level)
            : base(data, level)
        {
            string nameLower = Name.ToLower();
            if (nameLower.Contains("wall"))
            {
                if (nameLower.Contains("left"))
                    Rotation = MathHelper.PiOver2;
                else if (nameLower.Contains("right"))
                    Rotation = 3 * MathHelper.PiOver2;
            }
        }

        public override void UpdateTexture()
        {
            Texture = Atlas.Gameplay["objects/spring/00"];
            Texture.Origin = new Vector2(Texture.Width / 2, Texture.Height);
        }

        public override void Render(SpriteBatch spriteBatch, Camera camera)
        {
            Texture.Render(spriteBatch, camera, AbsolutePosition + Offset, Color.White, rotation:Rotation);
        }
    }
}
