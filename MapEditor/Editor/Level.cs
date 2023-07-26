using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using Editor.Entities;
using Editor.Utils;

namespace Editor
{
    public class Level
    {
        public LevelData LevelData { get; private set; }
        public readonly List<Entity> Entities = new();
        public TileGrid ForegroundTiles { get; private set; }
        public TileGrid BackgroundTiles { get; private set; }

        public List<Vector2> PlayerSpawns => LevelData.PlayerSpawns;
        public Rectangle Bounds => LevelData.Bounds;
        public Point Position => LevelData.Position;

        public Level(LevelData data)
        {
            LevelData = data;

            foreach (EntityData entityData in LevelData.Entities)
            {
                Entity entity;

                string shortName = entityData.Name;
                if (shortName.StartsWith("spikes"))
                    shortName = "spikes";
                else if (shortName.ToLower().Contains("spring"))
                    shortName = "spring";

                entity = shortName switch
                {
                    "spinner" => new Spinner(entityData, this),
                    "spikes" => new Spikes(entityData, this),
                    "jumpThru" => new JumpThru(entityData, this),
                    "spring" => new Spring(entityData, this),
                    _ => new(entityData, this)
                };
                entity.UpdateTexture();
                Entities.Add(entity);
            }

            ForegroundTiles = Autotiler.ForegroundTiles.GenerateLevel(data.TileBounds.Width, data.TileBounds.Height, LevelData.ForegroundTiles);
            BackgroundTiles = Autotiler.BackgroundTiles.GenerateLevel(data.TileBounds.Width, data.TileBounds.Height, LevelData.BackgroundTiles);
        }

        public void Render(SpriteBatch spriteBatch, Camera camera)
        {
            ForegroundTiles.Render(spriteBatch, camera, Position.ToVector2());
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new RectangleF(camera.MapToWindow(Position.ToVector2()), Bounds.Size.Mul(camera.Zoom)),
                Color.Magenta
            );

        public List<Entity> GetVisibleEntities(RectangleF cameraBounds) => Entities.FindAll(entity => cameraBounds.Intersects(entity.Bounds));

        public IEnumerable<Vector2> GetVisiblePlayerSpawns(RectangleF cameraBounds)
            => PlayerSpawns.FindAll(spawn => cameraBounds.Intersects(new(spawn, MapViewer.PlayerSpawnSize)));
    }
}
