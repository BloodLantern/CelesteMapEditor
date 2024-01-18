using Editor.Celeste;
using Editor.Edits;
using Editor.Extensions;
using Editor.Logging;
using Editor.Objects;
using Editor.Objects.Entities;
using Editor.Saved;
using Editor.Saved.Keybinds;
using Editor.UI.Components;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using static Editor.UI.Components.LayerSelection;

namespace Editor
{
    public class MapViewer
    {
        public readonly Session Session;
        public readonly Application App;
        public Map CurrentMap;
        public Camera Camera;
        public readonly MapViewerConfig Config;
        public readonly MapViewerKeybindsConfig Keybinds;

        private List<Level> visibleLevels = new();
        private List<Filler> visibleFillers = new();
        private readonly List<MapObject> objects = new();
        public IEnumerable<MapObject> ObjectsEnumerator => objects;
        private readonly List<MapObject> visibleObjects = new();
        private readonly List<Entity> visibleEntities = new();
        private readonly List<Trigger> visibleTriggers = new();
        private readonly List<PlayerSpawn> visiblePlayerSpawns = new();
        private readonly List<Spinner.BackgroundSpinner> visibleBackgroundSpinners = new();
        private readonly List<Tile> visibleForegroundTiles = new();
        private readonly List<Tile> visibleBackgroundTiles = new();
        private readonly List<Decal> visibleForegroundDecals = new();
        private readonly List<Decal> visibleBackgroundDecals = new();

        private readonly List<Edit> edits = new();

        public Selection Selection { get; private set; }

        public LayerSelection LayerSelection;

        public MapViewer(Application app)
        {
            App = app;
            Session = app.Session;
            Config = Session.Config.MapViewer;
            Keybinds = Session.Config.Keybinds.MapViewer;

            PlayerSpawn.SetupSprite();
        }

        public void Initialize()
        {
            LayerSelection = App.UiManager.GetComponent<LayerSelection>();
        }

        public void InitializeCamera()
        {
            Camera = new(App, this, Vector2.Zero, 1e-6f);
            Camera.ZoomToDefault(Camera.DefaultZoomDuration * 3f);

            Selection = new(this);
        }

        public void Update(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            if (CurrentMap == null)
                return;

            visibleLevels = CurrentMap.GetVisibleLevels(Camera.Bounds);
            visibleFillers = CurrentMap.GetVisibleFillers(Camera.Bounds);

            ClearObjectLists();

            RectangleF cameraBounds = Camera.Bounds;
            foreach (Level level in visibleLevels)
                AddLevelObjectsToLists(level, cameraBounds);

            AddObjectsToMainList();

            ImGuiIOPtr imGuiIo = ImGui.GetIO();
            if (imGuiIo.WantCaptureMouse || imGuiIo.WantCaptureKeyboard)
                return;

            HandleInputs(time, mouse, keyboard);
        }

        private void ClearObjectLists()
        {
            visibleObjects.Clear();
            visibleEntities.Clear();
            visibleTriggers.Clear();
            visiblePlayerSpawns.Clear();
            visibleBackgroundSpinners.Clear();
            visibleForegroundTiles.Clear();
            visibleBackgroundTiles.Clear();
            visibleForegroundDecals.Clear();
            visibleBackgroundDecals.Clear();
        }

        private void AddLevelObjectsToLists(Level level, RectangleF cameraBounds)
        {
            visibleEntities.AddRange(level.GetVisibleEntities(cameraBounds));
            visibleTriggers.AddRange(level.GetVisibleTriggers(cameraBounds));
            visiblePlayerSpawns.AddRange(level.GetVisiblePlayerSpawns(cameraBounds));
            visibleBackgroundSpinners.AddRange(level.GetVisibleBackgroundSpinners(cameraBounds));
            visibleForegroundTiles.AddRange(level.GetVisibleForegroundTiles(cameraBounds));
            visibleBackgroundTiles.AddRange(level.GetVisibleBackgroundTiles(cameraBounds));
            visibleForegroundDecals.AddRange(level.GetVisibleForegroundDecals(cameraBounds));
            visibleBackgroundDecals.AddRange(level.GetVisibleBackgroundDecals(cameraBounds));
        }

        private void AddObjectsToMainList()
        {
            visibleObjects.AddRange(visibleEntities);
            visibleObjects.AddRange(visibleTriggers);
            visibleObjects.AddRange(visiblePlayerSpawns);
            visibleObjects.AddRange(visibleBackgroundSpinners);
            visibleObjects.AddRange(visibleForegroundTiles);
            visibleObjects.AddRange(visibleBackgroundTiles);
            visibleObjects.AddRange(visibleForegroundDecals);
            visibleObjects.AddRange(visibleBackgroundDecals);
        }

        private void HandleInputs(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            Camera.HandleInputs(mouse, keyboard);

            Selection.HandleInputs(time, mouse, keyboard);
        }

        public Level GetLevelAt(Vector2 mapPosition)
        {
            foreach (Level level in visibleLevels)
            {
                if (level.Bounds.Contains(mapPosition))
                    return level;
            }
            return null;
        }

        public MapObject GetObjectAt(Vector2 mapPosition, Layers layers = Layers.All)
        {
            switch (layers)
            {
                case Layers.All:
                    foreach (MapObject obj in visibleObjects)
                    {
                        if (obj.AbsoluteBounds.Contains(mapPosition))
                            return obj;
                    }
                    break;
                case Layers.ForegroundTiles:
                    foreach (Tile tile in visibleForegroundTiles)
                    {
                        if (tile.AbsoluteBounds.Contains(mapPosition))
                            return tile;
                    }
                    break;
                case Layers.BackgroundTiles:
                    foreach (Tile tile in visibleBackgroundTiles)
                    {
                        if (tile.AbsoluteBounds.Contains(mapPosition))
                            return tile;
                    }
                    break;
                case Layers.Entities:
                    foreach (Entity entity in visibleEntities)
                    {
                        if (entity.AbsoluteBounds.Contains(mapPosition))
                            return entity;
                    }
                    break;
                case Layers.Triggers:
                    foreach (Trigger trigger in visibleTriggers)
                    {
                        if (trigger.AbsoluteBounds.Contains(mapPosition))
                            return trigger;
                    }
                    break;
                case Layers.PlayerSpawns:
                    foreach (PlayerSpawn spawn in visiblePlayerSpawns)
                    {
                        if (spawn.AbsoluteBounds.Contains(mapPosition))
                            return spawn;
                    }
                    break;
                case Layers.ForegroundDecals:
                    foreach (Decal decal in visibleForegroundDecals)
                    {
                        if (decal.AbsoluteBounds.Contains(mapPosition))
                            return decal;
                    }
                    break;
                case Layers.BackgroundDecals:
                    foreach (Decal decal in visibleBackgroundDecals)
                    {
                        if (decal.AbsoluteBounds.Contains(mapPosition))
                            return decal;
                    }
                    break;
            }
            return null;
        }

        public List<MapObject> GetObjectsAt(Vector2 mapPosition, Layers layers = Layers.All)
        {
            List<MapObject> result = new();
            switch (layers)
            {
                case Layers.All:
                    foreach (MapObject obj in visibleObjects)
                    {
                        if (obj.AbsoluteBounds.Contains(mapPosition))
                            result.Add(obj);
                    }
                    break;
                case Layers.ForegroundTiles:
                    foreach (Tile tile in visibleForegroundTiles)
                    {
                        if (tile.AbsoluteBounds.Contains(mapPosition))
                            result.Add(tile);
                    }
                    break;
                case Layers.BackgroundTiles:
                    foreach (Tile tile in visibleBackgroundTiles)
                    {
                        if (tile.AbsoluteBounds.Contains(mapPosition))
                            result.Add(tile);
                    }
                    break;
                case Layers.Entities:
                    foreach (Entity entity in visibleEntities)
                    {
                        if (entity.AbsoluteBounds.Contains(mapPosition))
                            result.Add(entity);
                    }
                    break;
                case Layers.Triggers:
                    foreach (Trigger trigger in visibleTriggers)
                    {
                        if (trigger.AbsoluteBounds.Contains(mapPosition))
                            result.Add(trigger);
                    }
                    break;
                case Layers.PlayerSpawns:
                    foreach (PlayerSpawn spawn in visiblePlayerSpawns)
                    {
                        if (spawn.AbsoluteBounds.Contains(mapPosition))
                            result.Add(spawn);
                    }
                    break;
                case Layers.ForegroundDecals:
                    foreach (Decal decal in visibleForegroundDecals)
                    {
                        if (decal.AbsoluteBounds.Contains(mapPosition))
                            result.Add(decal);
                    }
                    break;
                case Layers.BackgroundDecals:
                    foreach (Decal decal in visibleBackgroundDecals)
                    {
                        if (decal.AbsoluteBounds.Contains(mapPosition))
                            result.Add(decal);
                    }
                    break;
            }
            return result;
        }

        public List<MapObject> GetObjectsInArea(RectangleF mapArea, Layers layers = Layers.All)
        {
            List<MapObject> result = new();
            switch (layers)
            {
                case Layers.All:
                    foreach (MapObject obj in visibleObjects)
                    {
                        if (mapArea.Intersects(obj.AbsoluteBounds))
                            result.Add(obj);
                    }
                    break;
                case Layers.ForegroundTiles:
                    foreach (Tile tile in visibleForegroundTiles)
                    {
                        if (mapArea.Intersects(tile.AbsoluteBounds))
                            result.Add(tile);
                    }
                    break;
                case Layers.BackgroundTiles:
                    foreach (Tile tile in visibleBackgroundTiles)
                    {
                        if (mapArea.Intersects(tile.AbsoluteBounds))
                            result.Add(tile);
                    }
                    break;
                case Layers.Entities:
                    foreach (Entity entity in visibleEntities)
                    {
                        if (mapArea.Intersects(entity.AbsoluteBounds))
                            result.Add(entity);
                    }
                    break;
                case Layers.Triggers:
                    foreach (Trigger trigger in visibleTriggers)
                    {
                        if (mapArea.Intersects(trigger.AbsoluteBounds))
                            result.Add(trigger);
                    }
                    break;
                case Layers.PlayerSpawns:
                    foreach (PlayerSpawn spawn in visiblePlayerSpawns)
                    {
                        if (mapArea.Intersects(spawn.AbsoluteBounds))
                            result.Add(spawn);
                    }
                    break;
                case Layers.ForegroundDecals:
                    foreach (Decal decal in visibleForegroundDecals)
                    {
                        if (mapArea.Intersects(decal.AbsoluteBounds))
                            result.Add(decal);
                    }
                    break;
                case Layers.BackgroundDecals:
                    foreach (Decal decal in visibleBackgroundDecals)
                    {
                        if (mapArea.Intersects(decal.AbsoluteBounds))
                            result.Add(decal);
                    }
                    break;
            }
            return result;
        }

        public void Render(GameTime time, SpriteBatch spriteBatch)
        {
            if (CurrentMap == null)
                return;

            Layers renderingLayers = LayerSelection.GetLayers(LayerType.Rendering);

            if (renderingLayers.HasFlag(Layers.BackgroundTiles))
            {
                foreach (Tile tile in visibleBackgroundTiles)
                    tile.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.BackgroundDecals))
            {
                foreach (Decal decal in visibleBackgroundDecals)
                    decal.Render(spriteBatch, Camera);

                foreach (Spinner.BackgroundSpinner bgSpinner in visibleBackgroundSpinners)
                    bgSpinner.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.PlayerSpawns))
            {
                foreach (PlayerSpawn playerSpawn in visiblePlayerSpawns)
                    playerSpawn.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.Entities))
            {
                foreach (Entity entity in visibleEntities)
                    entity.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.ForegroundTiles))
            {
                foreach (Tile tile in visibleForegroundTiles)
                    tile.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.ForegroundDecals))
            {
                foreach (Decal decal in visibleForegroundDecals)
                    decal.Render(spriteBatch, Camera);
            }

            if (renderingLayers.HasFlag(Layers.Triggers))
            {
                foreach (Trigger trigger in visibleTriggers)
                    trigger.Render(spriteBatch, Camera);
            }

            Selection.Render(time, spriteBatch);

            // TODO Draw filler tiles

            DebugLayers renderingDebugLayers = LayerSelection.GetDebugLayers();

            if (renderingDebugLayers.HasFlag(DebugLayers.LevelBounds))
            {
                foreach (Level level in visibleLevels)
                    level.RenderDebug(spriteBatch, Camera);
            }

            if (renderingDebugLayers.HasFlag(DebugLayers.PlayerSpawns))
            {
                foreach (PlayerSpawn spawn in visiblePlayerSpawns)
                    spawn.RenderDebug(spriteBatch, Camera);
            }

            if (renderingDebugLayers.HasFlag(DebugLayers.EntityBounds))
            {
                foreach (Entity entity in visibleEntities)
                    entity.RenderDebug(spriteBatch, Camera);
            }

            if (renderingDebugLayers.HasFlag(DebugLayers.FillerBounds))
            {
                foreach (Rectangle filler in visibleFillers)
                    RenderDebugFiller(spriteBatch, Camera, filler);
            }
        }

        public void RenderDebugFiller(SpriteBatch spriteBatch, Camera camera, Rectangle filler)
            => spriteBatch.DrawRectangle(
                new RectangleF(Camera.MapPositionToWindow(filler.Location.ToVector2()), filler.Size.Mul(Camera.Zoom)),
                Color.Brown,
                Math.Max(camera.Zoom, 1f)
            );

        private bool showImGuiDemoWindow = false;
        public void RenderDebug(GameTime time)
        {
            MouseStateExtended mouseState = MouseExtended.GetState();
            Point mousePosition = mouseState.Position;
            Point mouseMapPosition = Camera.WindowPositionToMap(mousePosition);
            Level hoveredLevel = GetLevelAt(mouseMapPosition.ToVector2());

            ImGui.Begin($"{nameof(MapViewer)} debug", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing);
            ImGui.Checkbox("Debug mode", ref Session.Config.DebugMode);

            ImGui.Separator();

            ImGui.Text($"Mouse window position: {mousePosition}");
            ImGui.Text($"Mouse map position: {mouseMapPosition}");
            ImGui.Text("Mouse level position: " + (hoveredLevel != null ? (mouseMapPosition - hoveredLevel.Position).ToString() : "N/A"));

            ImGui.Separator();

            if (ImGui.Button("Reset##0"))
                Camera.MoveToDefault();
            ImGui.SameLine();
            ImGui.Text($"Camera position: {Camera.Position}");

            if (ImGui.Button("Reset##1"))
                Camera.ZoomToDefault();
            ImGui.SameLine();
            ImGui.Text($"Camera zoom factor: {Camera.Zoom}");

            ImGui.Separator();

            if (!Selection.Empty() && ImGui.TreeNode("Selected objects"))
            {
                for (int i = 0; i < Selection.Count; i++)
                {
                    if (Selection.Count == 1 || ImGui.TreeNode($"{i}"))
                    {
                        MapObject obj = Selection[i];
                        ImGui.Text($"Type: {obj.GetType()}");
                        ImGui.Text($"Level position: {obj.Position}");
                        ImGui.Text($"Absolute position: {obj.AbsolutePosition}");
                        ImGui.Text($"Relative position: {obj.AbsolutePosition - Camera.Position}");
                        ImGui.Text($"Size: {obj.Size}");

                        if (Selection.Count > 1)
                            ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Visible levels: {visibleLevels.Count}");
            ImGui.Text($"Visible fillers: {visibleFillers.Count}");
            ImGui.Text($"Visible entities: {visibleEntities.Count}");
            ImGui.Text($"Visible player spawns: {visiblePlayerSpawns.Count}");
            ImGui.Text($"Visible background spinners: {visibleBackgroundSpinners.Count}");
            ImGui.Text($"Visible background decals: {visibleBackgroundDecals.Count}");
            ImGui.Text($"Visible foreground decals: {visibleForegroundDecals.Count}");
            ImGui.Text($"Visible background tiles: {visibleBackgroundTiles.Count}");
            ImGui.Text($"Visible foreground tiles: {visibleForegroundTiles.Count}");
            ImGui.Text($"Visible objects: {visibleObjects.Count}");
            if (ImGui.TreeNode($"Visible objects list"))
            {
                for (int i = 0; i < visibleObjects.Count; i++)
                {
                    MapObject obj = visibleObjects[i];
                    if (ImGui.TreeNode($"{i}. {obj}"))
                    {
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            if (Camera.Zoom == 3f)
                                Camera.MoveTo(obj.Center);
                            else
                                Camera.ZoomTo(Camera.MapPositionToWindow(obj.Center), 3f);
                            Selection.SelectOnly(obj);
                        }

                        obj.DebugInfo();

                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Active coroutines: {Coroutine.RunningCount}");
            ImGui.Text($"Total time: {time.TotalGameTime}");
            if (ImGui.Button("Test logging"))
                Logger.Test();

            ImGui.Checkbox("Show ImGui demo window", ref showImGuiDemoWindow);
            if (showImGuiDemoWindow)
                ImGui.ShowDemoWindow();

            if (ImGui.Button("Use Application.DebugString"))
                App.DebugString("Debug message");

            ImGui.End();
        }

        public static Point ToTilePosition(Point position) => position.Div(Tileset.TileSize);

        public static Vector2 ToTilePosition(Vector2 position) => position / Tileset.TileSize;

        public static Point ToPixelPosition(Point position) => position.Mul(Tileset.TileSize);

        public static Vector2 ToPixelPosition(Vector2 position) => position * Tileset.TileSize;
    }
}
