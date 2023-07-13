namespace Editor.Celeste
{
    public class Tileset
    {
        public const int TileSize = 8;

        public Texture Texture { get; private set; }

        private readonly Texture[,] tiles;

        public Tileset(Texture texture)
        {
            Texture = texture;

            int tileWidth = Texture.Width / TileSize, tileHeight = Texture.Height / TileSize;
            tiles = new Texture[tileWidth, tileHeight];

            for (int x = 0; x < tileWidth; x++)
            {
                for (int y = 0; y < tileHeight; y++)
                    tiles[x, y] = new Texture(Texture, x * TileSize, y * TileSize);
            }
        }

        public Texture this[int x, int y] => tiles[x, y];

        public Texture this[int index] => index < 0 ? null : tiles[index % tiles.GetLength(0), index / tiles.GetLength(0)];
    }
}
