using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace Editor.Entities
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
        private static readonly string spikesTextureBaseName = "danger/spikes/{type}_{direction}";

        public readonly Directions Direction;
        public readonly string Type;
        public readonly int Length;
        private readonly List<Texture> textures = new();

        public override Vector2 Position => EntityData.Position;

        public Spikes(EntityData data, Level level) : base(data, level)
        {
            Type = EntityData.Attr("type", "default");
            switch (Name[6..].ToLower())
            {
                case "up":
                    Direction = Directions.Up;
                    Length = data.Size.X;
                    break;
                case "down":
                    Direction = Directions.Down;
                    Length = data.Size.X;
                    break;
                case "left":
                    Direction = Directions.Left;
                    Length = data.Size.Y;
                    break;
                case "right":
                    Direction = Directions.Right;
                    Length = data.Size.Y;
                    break;
                default:
                    throw new ArgumentException("Spike direction not handled");
            }
        }

        public override void UpdateTexture()
        {
            if (Type == TentacleType)
            {
                return;
            }

            Texture = Atlas.Gameplay[spikesTextureBaseName.Replace("{type}", Type).Replace("{direction}", Direction.ToString()) + "00"];
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
