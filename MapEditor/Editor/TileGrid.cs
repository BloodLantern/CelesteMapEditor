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

        public Image<Rgba32> Image { get; private set; } = null;

        /// <summary>
        /// Constructs a new TileGrid with a set size.
        /// </summary>
        /// <param name="tilesX">The width in tiles.</param>
        /// <param name="tilesY">The height in tiles.</param>
        internal TileGrid(int tilesX, int tilesY)
        {
            Tiles = new Texture[tilesX, tilesY];
        }

        public void Invalidate() => Image = null;

        public void Render(RectangleF cameraBounds, Image<Rgba32> image, Point levelPosition)
        {
            if (Image != null)
            {
                image.Mutate(o => o.DrawImage(Image, levelPosition - (Size) Point.Round(cameraBounds.Position()), Alpha));
                return;
            }

            Image = new(TilesX * Tileset.TileSize, TilesY * Tileset.TileSize);
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                {
                    Texture tile = Tiles[x, y];
                    if (tile == null)
                        continue;

                    Image.Mutate(o => o.DrawImage(tile.Image, new Point(x * Tileset.TileSize, y * Tileset.TileSize), Alpha));
                }
            }

            Render(cameraBounds, image, levelPosition);
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
