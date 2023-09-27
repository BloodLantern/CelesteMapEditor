using Editor.Celeste;
using Editor.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects.Entities
{
    public class JumpThru : Entity
    {
        public override Vector2 Position => EntityData.Position;
        public override Point Size => EntityData.Size;

        private Texture leftWallTexture, rightWallTexture;
        private Texture leftTexture, rightTexture;
        private Texture middleTexture;

        public JumpThru(EntityData data, Level level) : base(data, level)
        {
        }

        public override void UpdateTexture()
        {
            string type = EntityData.Attr("texture", "wood");
            if (type == "default")
                type = "wood";
            string typeEntry = $"objects/jumpthru/{type}";
            Texture = Atlas.Gameplay[typeEntry];

            if (Atlas.Gameplay.Textures.ContainsKey(typeEntry + "/leftWall"))
            {
                leftWallTexture = Atlas.Gameplay[typeEntry + "/leftWall"];
                rightWallTexture = Atlas.Gameplay[typeEntry + "/rightWall"];
                leftTexture = Atlas.Gameplay[typeEntry + "/left"];
                rightTexture = Atlas.Gameplay[typeEntry + "/right"];
            }
            else
            {
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/leftWall",
                    leftWallTexture = new(
                        Texture,
                        0, 0,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/middle0",
                    new(
                        Texture,
                        Tileset.TileSize, 0,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/rightWall",
                    rightWallTexture = new(
                        Texture,
                        Texture.Width - Tileset.TileSize, 0,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/left",
                    leftTexture = new(
                        Texture,
                        0, Tileset.TileSize,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/middle1",
                    new(
                        Texture,
                        Tileset.TileSize, Tileset.TileSize,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
                Atlas.Gameplay.Textures.Add(
                    typeEntry + "/right",
                    rightTexture = new(
                        Texture,
                        Texture.Width - Tileset.TileSize, Tileset.TileSize,
                        Tileset.TileSize, Tileset.TileSize
                    )
                );
            }

            middleTexture = Atlas.Gameplay[typeEntry + "/middle" + Calc.Random.Choose(0, 1)];
        }

        public override void Render(SpriteBatch spriteBatch, Camera camera)
        {
            Point tilePosition = MapViewer.ToTilePosition(Position.ToPoint());
            Point tileSize = MapViewer.ToTilePosition(Size);

            int yIndex = Calc.Clamp(tilePosition.Y, 0, Level.ForegroundTiles.TilesY - 1);

            // Left part
            // If there is a wall on the left
            if (Level.ForegroundTiles.Tiles[Calc.Clamp(tilePosition.X - 1, 0, Level.ForegroundTiles.TilesX - 1), yIndex] != null)
                leftWallTexture.Render(spriteBatch, camera, AbsolutePosition);
            else
                leftTexture.Render(spriteBatch, camera, AbsolutePosition);

            // Right part
            // If there is a wall on the right
            if (Level.ForegroundTiles.Tiles[Calc.Clamp(tilePosition.X + Size.X, 0, Level.ForegroundTiles.TilesX - 1), yIndex] != null)
                rightWallTexture.Render(spriteBatch, camera, AbsolutePosition + new Vector2(Size.X - Tileset.TileSize, 0));
            else
                rightTexture.Render(spriteBatch, camera, AbsolutePosition + new Vector2(Size.X - Tileset.TileSize, 0));

            // Middle part
            for (int i = 1; i < tileSize.X - 1; i++)
                middleTexture.Render(spriteBatch, camera, AbsolutePosition + MapViewer.ToPixelPosition(new Vector2(i, 0)));
        }
    }
}
