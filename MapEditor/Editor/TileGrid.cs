using Editor.Celeste;
using Editor.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor
{
    /// <summary>
    /// A TileGrid represents the tilemap for a single <see cref="Level"/>.
    /// </summary>
    public class TileGrid
    {
        public Tile[,] Tiles;

        public int TilesX => Tiles.GetLength(0);
        public int TilesY => Tiles.GetLength(1);

        public readonly Level Level;

        /// <summary>
        /// Constructs a new TileGrid with a set size.
        /// </summary>
        /// <param name="tilesX">The width in tiles.</param>
        /// <param name="tilesY">The height in tiles.</param>
        internal TileGrid(Level level, int tilesX, int tilesY)
        {
            Tiles = new Tile[tilesX, tilesY];
            Level = level;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 levelPosition)
        {
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                    Tiles[x, y]?.Render(spriteBatch, camera, levelPosition + new Vector2(x * Tileset.TileSize, y * Tileset.TileSize));
            }
        }

        public void Clear()
        {
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                    Tiles[x, y] = null;
            }
        }
    }
}
