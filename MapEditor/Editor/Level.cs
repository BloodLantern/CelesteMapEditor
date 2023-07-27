using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using Editor.Entities;
using Editor.Extensions;
using System;

namespace Editor
{
    public class Level
    {
        private static readonly Color[] EditorColors = new Color[7] {
            Color.White,                // Default white
            Calc.HexToColor("f6735e"),  // Orange
            Calc.HexToColor("85f65e"),  // Green
            Calc.HexToColor("37d7e3"),  // Light blue
            Calc.HexToColor("376be3"),  // Blue
            Calc.HexToColor("c337e3"),  // Purple
            Calc.HexToColor("e33773")   // Pink
        };

        public readonly LevelData LevelData;
        public readonly List<Entity> Entities = new();
        public readonly TileGrid ForegroundTiles;
        public readonly TileGrid BackgroundTiles;
        public readonly List<Decal> ForegroundDecals = new();
        public readonly List<Decal> BackgroundDecals = new();
        public readonly Color EditorColor;

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

            foreach (DecalData decalData in data.FgDecals)
                ForegroundDecals.Add(new Decal(decalData));
            foreach (DecalData decalData in data.BgDecals)
                BackgroundDecals.Add(new Decal(decalData));

            EditorColor = EditorColors[data.EditorColorIndex];
        }

        public void RenderForeground(SpriteBatch spriteBatch, Camera camera)
        {
            Vector2 position = Position.ToVector2();
            ForegroundTiles.Render(spriteBatch, camera, position);
            foreach (Decal decal in ForegroundDecals)
                decal.Render(spriteBatch, camera, position);
        }

        public void RenderBackground(SpriteBatch spriteBatch, Camera camera)
        {
            Vector2 position = Position.ToVector2();
            BackgroundTiles.Render(spriteBatch, camera, position);
            foreach (Decal decal in BackgroundDecals)
                decal.Render(spriteBatch, camera, position);
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new RectangleF(camera.MapToWindow(Position.ToVector2()), Bounds.Size.Mul(camera.Zoom)),
                EditorColor,
                Math.Max(camera.Zoom, 1f)
            );

        public List<Entity> GetVisibleEntities(RectangleF cameraBounds) => Entities.FindAll(entity => cameraBounds.Intersects(entity.Bounds));

        public IEnumerable<Vector2> GetVisiblePlayerSpawns(RectangleF cameraBounds)
            => PlayerSpawns.FindAll(spawn => cameraBounds.Intersects(new(spawn + MapViewer.PlayerSpawnOffset, MapViewer.PlayerSpawnSize)));
    }
}
