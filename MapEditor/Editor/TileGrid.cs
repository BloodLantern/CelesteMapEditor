using Editor.Celeste;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

        public float Alpha = 1f;

        /// <summary>
        /// Constructs a new TileGrid with a set size.
        /// </summary>
        /// <param name="tilesX">The width in tiles.</param>
        /// <param name="tilesY">The height in tiles.</param>
        internal TileGrid(int tilesX, int tilesY)
        {
            Tiles = new Texture[tilesX, tilesY];
        }

        public int Render(RectangleF cameraBounds, Image<Rgba32> image, Point levelPosition)
        {
            int count = 0;
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                {
                    Texture tile = Tiles[x, y];
                    if (tile == null)
                        continue;

                    Point absolutePosition = levelPosition + new Size(x * Tileset.TileSize, y * Tileset.TileSize);
                    if (absolutePosition.X >= cameraBounds.Right
                        || absolutePosition.Y >= cameraBounds.Bottom
                        || absolutePosition.X + Tileset.TileSize <= cameraBounds.Left
                        || absolutePosition.Y + Tileset.TileSize <= cameraBounds.Top)
                        continue;

                    image.Mutate(o => o.DrawImage(tile.Image, absolutePosition - (Size) Point.Round(cameraBounds.Position()), Alpha));
                    count++;
                }
            }

            return count;
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
