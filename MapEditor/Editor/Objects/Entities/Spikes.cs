using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Editor.Objects.Entities
{
    public class Spikes : Entity
    {
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right,
        }

        private const string TentacleType = "tentacles";
        private static readonly string SpikesTextureBaseName = "danger/spikes/";

        public readonly Directions Direction;
        public readonly string SpikeType;
        public readonly int Length;
        private readonly List<Texture> textures = new();

        public override Vector2 Position => EntityData.Position;

        public Spikes(EntityData data, Level level) : base(data, level)
        {
            SpikeType = EntityData.Attr("type", "default");
            switch (Name[6..].ToLower())
            {
                case "up":
                    Direction = Directions.Up;
                    Length = (int) data.Size.X;
                    break;
                case "down":
                    Direction = Directions.Down;
                    Length = (int) data.Size.X;
                    break;
                case "left":
                    Direction = Directions.Left;
                    Length = (int) data.Size.Y;
                    break;
                case "right":
                    Direction = Directions.Right;
                    Length = (int) data.Size.Y;
                    break;
                default:
                    throw new ArgumentException("Spike direction not handled");
            }
        }

        public override void UpdateTexture()
        {
            if (SpikeType == TentacleType)
            {
                return;
            }

            Texture = Atlas.Gameplay[SpikesTextureBaseName + SpikeType + '_' + Direction.ToString() + "00"];
            for (int i = 0; i < Length / Tileset.TileSize; i++)
            {
                Texture texture = new(Texture);
                switch (Direction)
                {
                    case Directions.Up:
                        texture.JustifyOrigin(0.5f, 1f);
                        texture.Offset = new((i + 0.5f) * Tileset.TileSize, 1f);
                        break;
                    
                    case Directions.Down:
                        texture.JustifyOrigin(0.5f, 0f);
                        texture.Offset = new((i + 0.5f) * Tileset.TileSize, -1f);
                        break;
                    
                    case Directions.Left:
                        texture.JustifyOrigin(1f, 0.5f);
                        texture.Offset = new(1f, (i + 0.5f) * Tileset.TileSize);
                        break;
                    
                    case Directions.Right:
                        texture.JustifyOrigin(0f, 0.5f);
                        texture.Offset = new(-1f, (i + 0.5f) * Tileset.TileSize);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                textures.Add(texture);
            }
        }

        public override void Render(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (Texture texture in textures)
                texture.Render(spriteBatch, camera, AbsolutePosition);
        }
    }
}
