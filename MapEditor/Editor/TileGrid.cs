using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using static System.Net.Mime.MediaTypeNames;

namespace Editor
{
    /// <summary>
    /// A TileGrid represents the tilemap for a single <see cref="Level"/>.
    /// </summary>
    public class TileGrid
    {
        public Texture[,] Tiles;

        public int TilesX => Tiles.GetLength(0);
        public int TilesY => Tiles.GetLength(1);

        /// <summary>
        /// Constructs a new TileGrid with a set size.
        /// </summary>
        /// <param name="tilesX">The width in tiles.</param>
        /// <param name="tilesY">The height in tiles.</param>
        internal TileGrid(int tilesX, int tilesY)
        {
            Tiles = new Texture[tilesX, tilesY];
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
