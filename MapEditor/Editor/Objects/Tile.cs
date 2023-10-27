using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects
{
    public class Tile : MapObject
    {
        public readonly Texture Texture;
        public readonly int X, Y;
        public readonly Autotiler.TileType TileType;

        public Tile(Level level, Texture texture, int x, int y, Autotiler.TileType tileType) : base(level, MapObjectType.Tile)
        {
            Texture = texture;
            X = x;
            Y = y;
            TileType = tileType;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 offset) => Texture.Render(spriteBatch, camera, offset);
    }
}
