using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using Editor.Objects.Entities;
using Editor.Objects;

namespace Editor
{
    public class Level
    {
        private static readonly Color[] EditorColors = new Color[7]
        {
            Color.White,      // Default white
            new(0xFF5E73F6),  // Orange
            new(0xFF5EF685),  // Green
            new(0xFFE3D737),  // Light blue
            new(0xFFE36B37),  // Blue
            new(0xFFE337C3),  // Purple
            new(0xFF7337E3)   // Pink
        };

        public readonly LevelData LevelData;
        public readonly List<Entity> Entities = new();
        public readonly List<Trigger> Triggers = new();
        public readonly List<Spinner> Spinners = new();
        public readonly List<Spinner.BackgroundSpinner> BackgroundSpinners = new();
        public readonly TileGrid ForegroundTiles;
        public readonly TileGrid BackgroundTiles;
        public readonly List<Decal> ForegroundDecals = new();
        public readonly List<Decal> BackgroundDecals = new();
        public readonly Color EditorColor;

        public List<PlayerSpawn> PlayerSpawns { get; private set; } = new();
        public Rectangle Bounds => LevelData.Bounds;
        public Point Position => Bounds.Location;
        public Point Size => Bounds.Size;
        public string Name => LevelData.Name;

        public bool Selected = false;

        public Vector2 Center => Position.ToVector2() + Size.ToVector2() / 2;
        /// <summary>
        /// Whether the level is considered to be a room filler.
        /// </summary>
        public bool Filler => PlayerSpawns.Count == 0;

        public Level(LevelData data)
        {
            LevelData = data;

            foreach (Vector2 spawn in LevelData.PlayerSpawns)
                PlayerSpawns.Add(new PlayerSpawn(this, spawn));

            foreach (EntityData entityData in LevelData.Entities)
            {
                Entity entity;

                string shortName = entityData.Name.ToLower();
                if (shortName.StartsWith("spikes"))
                    shortName = "spikes";
                else if (shortName.Contains("spring"))
                    shortName = "spring";

                entity = shortName switch
                {
                    "spinner" => new Spinner(entityData, this),
                    "spikes" => new Spikes(entityData, this),
                    "jumpthru" => new JumpThru(entityData, this),
                    "spring" => new Spring(entityData, this),
                    _ => new Entity(entityData, this)
                };
                entity.UpdateTexture();
                Entities.Add(entity);

                switch (entity)
                {
                    case Spinner spinner:
                        Spinners.Add(spinner);
                        break;
                }
            }

            foreach (EntityData triggerData in LevelData.Triggers)
                Triggers.Add(new Trigger(triggerData, this));

            foreach (Spinner spinner in Spinners)
            {
                spinner.CreateBackgroundSpinners(Spinners);
                BackgroundSpinners.AddRange(spinner.BackgroundSpinners);
            }

            foreach (Spinner.BackgroundSpinner bgSpinner in BackgroundSpinners)
                bgSpinner.UpdateTexture();

            ForegroundTiles = Autotiler.ForegroundTiles.GenerateLevel(this, data.TileBounds.Width, data.TileBounds.Height, LevelData.ForegroundTiles);
            BackgroundTiles = Autotiler.BackgroundTiles.GenerateLevel(this, data.TileBounds.Width, data.TileBounds.Height, LevelData.BackgroundTiles);

            foreach (DecalData decalData in data.FgDecals)
                ForegroundDecals.Add(new Decal(this, decalData));
            foreach (DecalData decalData in data.BgDecals)
                BackgroundDecals.Add(new Decal(this, decalData));

            EditorColor = EditorColors[data.EditorColorIndex];
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(camera.MapAreaToWindow(Bounds), EditorColor, camera.GetLineThickness());

        public List<Entity> GetVisibleEntities(RectangleF cameraBounds) => Entities.FindAll(entity => cameraBounds.Intersects(entity.AbsoluteBounds));

        public List<PlayerSpawn> GetVisiblePlayerSpawns(RectangleF cameraBounds)
            => PlayerSpawns.FindAll(spawn => cameraBounds.Intersects(new(spawn.AbsolutePosition, PlayerSpawn.SizeConst)));

        public List<Trigger> GetVisibleTriggers(RectangleF cameraBounds) => Triggers.FindAll(trigger => cameraBounds.Intersects(trigger.AbsoluteBounds));

        public List<Spinner.BackgroundSpinner> GetVisibleBackgroundSpinners(RectangleF cameraBounds) => BackgroundSpinners.FindAll(bgSpinner => cameraBounds.Intersects(bgSpinner.AbsoluteBounds));

        public List<Tile> GetVisibleForegroundTiles(RectangleF cameraBounds)
        {
            List<Tile> result = new();
            foreach (Tile tile in ForegroundTiles.Tiles)
            {
                if (tile == null)
                    continue;

                if (cameraBounds.Intersects(tile.AbsoluteBounds))
                    result.Add(tile);
            }

            return result;
        }

        public List<Tile> GetVisibleBackgroundTiles(RectangleF cameraBounds)
        {
            List<Tile> result = new();
            foreach (Tile tile in BackgroundTiles.Tiles)
            {
                if (tile == null)
                    continue;

                if (cameraBounds.Intersects(tile.AbsoluteBounds))
                    result.Add(tile);
            }

            return result;
        }

        public List<Decal> GetVisibleForegroundDecals(RectangleF cameraBounds) => ForegroundDecals.FindAll(decal => cameraBounds.Intersects(decal.AbsoluteBounds));

        public List<Decal> GetVisibleBackgroundDecals(RectangleF cameraBounds) => BackgroundDecals.FindAll(decal => cameraBounds.Intersects(decal.AbsoluteBounds));

        public void Remove(MapObject mapObject)
        {
            switch (mapObject)
            {
                case Entity entity:
                    Entities.Remove(entity);
                    break;
                case Trigger trigger:
                    Triggers.Remove(trigger);
                    break;
                case PlayerSpawn spawn:
                    PlayerSpawns.Remove(spawn);
                    break;
                case Tile tile:
                    switch (tile.TileType)
                    {
                        case Autotiler.TileType.Foreground:
                            ForegroundTiles[tile.TilePosition] = null;
                            break;
                        case Autotiler.TileType.Background:
                            BackgroundTiles[tile.TilePosition] = null;
                            break;
                        case Autotiler.TileType.Animated:
                            // Nothing for now
                            break;
                    }
                    break;
                case Decal decal:
                    ForegroundDecals.Remove(decal);
                    BackgroundDecals.Remove(decal);
                    break;
            }
        }
    }
}
