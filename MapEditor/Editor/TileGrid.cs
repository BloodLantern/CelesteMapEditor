using Editor.Objects;
using Microsoft.Xna.Framework;

namespace Editor
{
    /// <summary>
    /// A TileGrid represents the tilemap for a single <see cref="Level"/>.
    /// </summary>
    public class TileGrid
    {
        public readonly Tile[,] Tiles;

        public int TilesX => Tiles.GetLength(0);
        public int TilesY => Tiles.GetLength(1);

        public readonly Level Level;

        /// <summary>
        /// Constructs a new TileGrid with a set size.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="tilesX">The width in tiles.</param>
        /// <param name="tilesY">The height in tiles.</param>
        internal TileGrid(Level level, int tilesX, int tilesY)
        {
            Tiles = new Tile[tilesX, tilesY];
            Level = level;
        }

        public void Clear()
        {
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                    Tiles[x, y] = null;
            }
        }

        public Tile this[int x, int y]
        {
            get => Tiles[x, y];
            set => Tiles[x, y] = value;
        }

        public Tile this[Point tilePosition]
        {
            get => Tiles[tilePosition.X, tilePosition.Y];
            set => Tiles[tilePosition.X, tilePosition.Y] = value;
        }

        public Tile this[float x, float y]
        {
            get => Tiles[(int) x, (int) y];
            set => Tiles[(int) x, (int) y] = value;
        }

        public Tile this[Vector2 tilePosition]
        {
            get => Tiles[(int) tilePosition.X, (int) tilePosition.Y];
            set => Tiles[(int) tilePosition.X, (int) tilePosition.Y] = value;
        }
    }
}
