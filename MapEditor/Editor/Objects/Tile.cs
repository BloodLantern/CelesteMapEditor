using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects
{
    public class Tile : MapObject
    {
        public readonly Texture Texture;
        public readonly Vector2 TilePosition;
        public readonly Autotiler.TileType TileType;

        public override Vector2 Position => TilePosition * Size;
        public override Vector2 Size => new(Tileset.TileSize);

        public Tile(Level level, Texture texture, Vector2 tilePosition, Autotiler.TileType tileType) : base(level, MapObjectType.Tile)
        {
            Texture = texture;
            TilePosition = tilePosition;
            TileType = tileType;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera) => Texture.Render(spriteBatch, camera, AbsolutePosition);
    }
}
