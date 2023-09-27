using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects
{
    public class Tile : MapObject
    {
        public readonly Texture Texture;

        public Tile(Level level, Texture texture) : base(level, MapObjectType.Tile)
        {
            Texture = texture;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset) => Texture.Render(spriteBatch, camera, offset);
    }
}
