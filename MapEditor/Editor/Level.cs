using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using Editor.Entities;

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
        public readonly List<Trigger> Triggers = new();
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
                    _ => new(entityData, this)
                };
                entity.UpdateTexture();
                Entities.Add(entity);
            }

            foreach (EntityData triggerData in LevelData.Triggers)
                Triggers.Add(new(triggerData, this));

            foreach (Entity entity in Entities)
            {
                if (entity is Spinner spinner)
                    spinner.CreateBackgroundSpinners(Entities, BackgroundSpinners);
            }

            foreach (Spinner.BackgroundSpinner bgSpinner in BackgroundSpinners)
                bgSpinner.UpdateTexture();

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

            foreach (Spinner.BackgroundSpinner bgSpinner in BackgroundSpinners)
                bgSpinner.Render(spriteBatch, camera);
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(camera.MapToWindow(Bounds), EditorColor, camera.GetLineThickness());

        public List<Entity> GetVisibleEntities(RectangleF cameraBounds) => Entities.FindAll(entity => cameraBounds.Intersects(entity.AbsoluteBounds));

        public List<PlayerSpawn> GetVisiblePlayerSpawns(RectangleF cameraBounds)
            => PlayerSpawns.FindAll(spawn => cameraBounds.Intersects(new(spawn.Position + PlayerSpawn.Offset, PlayerSpawn.SizeConst)));

        public List<Trigger> GetVisibleTriggers(RectangleF cameraBounds) => Triggers.FindAll(trigger => cameraBounds.Intersects(trigger.Bounds));
    }
}
