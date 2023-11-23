using Editor.Celeste;
using Editor.Edits;
using Editor.Extensions;
using Editor.Logging;
using Editor.Objects;
using Editor.Saved;
using Editor.Saved.Keybinds;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;

namespace Editor
{
    public class MapViewer
    {
        [Flags]
        public enum Layers : byte
        {
            None            = 0b00000000,
            LevelForeground = 0b00000001,
            LevelBackground = 0b00000010,
            Fillers         = 0b00000100,
            Entities        = 0b00001000,
            Triggers        = 0b00010000,
            PlayerSpawns    = 0b00100000,
            Unused1         = 0b01000000,
            Unused2         = 0b10000000,
            All             = 0b11111111
        }

        [Flags]
        public enum DebugLayers : byte
        {
            None            = 0b00000000,
            LevelBounds     = 0b00000001,
            EntityBounds    = 0b00000010,
            FillerBounds    = 0b00000100,
            PlayerSpawns    = 0b00001000,
            Unused2         = 0b00010000,
            Unused3         = 0b00100000,
            Unused4         = 0b01000000,
            Unused5         = 0b10000000,
            All             = 0b11111111
        }

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
        private readonly List<Tile> visibleForegroundTiles = new();
        private readonly List<Tile> visibleBackgroundTiles = new();

        private readonly List<Edit> edits = new();

        public Selection Selection { get; private set; }

        private Layers renderedLayers = Layers.All;
        private DebugLayers renderedDebugLayers = DebugLayers.LevelBounds | DebugLayers.FillerBounds;

        public MapViewer(Application app)
        {
            App = app;
            Session = app.Session;
            Config = Session.Config.MapViewer;
            Keybinds = Session.Config.Keybinds.MapViewer;

            PlayerSpawn.SetupSprite();
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

            ImGuiIOPtr imGuiIO = ImGui.GetIO();
            if (imGuiIO.WantCaptureMouse || imGuiIO.WantCaptureKeyboard)
                return;

            HandleInputs(time, mouse, keyboard);
        }

        private void ClearObjectLists()
        {
            visibleObjects.Clear();
            visibleEntities.Clear();
            visibleTriggers.Clear();
            visiblePlayerSpawns.Clear();
            visibleForegroundTiles.Clear();
            visibleBackgroundTiles.Clear();
        }

        private void AddLevelObjectsToLists(Level level, RectangleF cameraBounds)
        {
            visibleEntities.AddRange(level.GetVisibleEntities(cameraBounds));
            visibleTriggers.AddRange(level.GetVisibleTriggers(cameraBounds));
            visiblePlayerSpawns.AddRange(level.GetVisiblePlayerSpawns(cameraBounds));
            visibleForegroundTiles.AddRange(level.GetVisibleForegroundTiles(cameraBounds));
            visibleBackgroundTiles.AddRange(level.GetVisibleBackgroundTiles(cameraBounds));
        }

        private void AddObjectsToMainList()
        {
            visibleObjects.AddRange(visibleEntities);
            visibleObjects.AddRange(visibleTriggers);
            visibleObjects.AddRange(visiblePlayerSpawns);
            visibleObjects.AddRange(visibleForegroundTiles);
            visibleObjects.AddRange(visibleBackgroundTiles);
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

        public MapObject GetObjectAt(Vector2 mapPosition)
        {
            foreach (MapObject obj in visibleObjects)
            {
                if (obj.AbsoluteBounds.Contains(mapPosition))
                    return obj;
            }
            return null;
        }

        public List<MapObject> GetObjectsInArea(RectangleF mapArea)
        {
            List<MapObject> result = new();
            foreach (MapObject obj in visibleObjects)
            {
                if (mapArea.Intersects(obj.AbsoluteBounds))
                    result.Add(obj);
            }
            return result;
        }

        public void Render(GameTime time, SpriteBatch spriteBatch)
        {
            if (CurrentMap == null)
                return;

            if (renderedLayers.HasFlag(Layers.LevelBackground))
            {
                foreach (Level level in visibleLevels)
                    level.RenderBackground(spriteBatch, Camera);
            }

            if (renderedLayers.HasFlag(Layers.PlayerSpawns))
            {
                foreach (PlayerSpawn playerSpawn in visiblePlayerSpawns)
                    playerSpawn.Render(spriteBatch, Camera);
            }

            if (renderedLayers.HasFlag(Layers.Entities))
            {
                foreach (Entity entity in visibleEntities)
                    entity.Render(spriteBatch, Camera);
            }

            if (renderedLayers.HasFlag(Layers.LevelForeground))
            {
                foreach (Level level in visibleLevels)
                    level.RenderForeground(spriteBatch, Camera);
            }

            if (renderedLayers.HasFlag(Layers.Triggers))
            {
                foreach (Trigger trigger in visibleTriggers)
                    trigger.Render(spriteBatch, Camera);
            }

            Selection.Render(time, spriteBatch);

            // TODO Draw filler tiles

            if (renderedDebugLayers.HasFlag(DebugLayers.LevelBounds))
            {
                foreach (Level level in visibleLevels)
                    level.RenderDebug(spriteBatch, Camera);
            }

            if (renderedDebugLayers.HasFlag(DebugLayers.PlayerSpawns))
            {
                foreach (PlayerSpawn spawn in visiblePlayerSpawns)
                    spawn.RenderDebug(spriteBatch, Camera);
            }

            if (renderedDebugLayers.HasFlag(DebugLayers.EntityBounds))
            {
                foreach (Entity entity in visibleEntities)
                    entity.RenderDebug(spriteBatch, Camera);
            }

            if (renderedDebugLayers.HasFlag(DebugLayers.FillerBounds))
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

            if (ImGui.TreeNode("Layers"))
            {
                int renderedLayersInt = (int) renderedLayers;
                foreach (Layers layer in Enum.GetValues(typeof(Layers)))
                    ImGui.CheckboxFlags(layer.ToString(), ref renderedLayersInt, (int) layer);
                renderedLayers = (Layers) renderedLayersInt;

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Debug layers"))
            {
                int renderedDebugLayersInt = (int) renderedDebugLayers;
                foreach (DebugLayers layer in Enum.GetValues(typeof(DebugLayers)))
                    ImGui.CheckboxFlags(layer.ToString(), ref renderedDebugLayersInt, (int) layer);
                renderedDebugLayers = (DebugLayers) renderedDebugLayersInt;

                ImGui.TreePop();
            }

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
            ImGui.Checkbox("Show debug console", ref Session.Config.ShowDebugConsoleWindow);
            ImGui.Checkbox("Show average fps", ref Session.Config.ShowAverageFps);
            if (ImGui.Button("Test logging"))
                Logger.Test();

            if (ImGui.BeginCombo("UI Style", Session.Config.UI.Style.ToString()))
            {
                foreach (ImGuiStyles.Style style in Enum.GetValues(typeof(ImGuiStyles.Style)))
                {
                    if (ImGui.Selectable(style.ToString()))
                        ImGuiStyles.Setup(Session.Config.UI.Style = style);
                }
                ImGui.EndCombo();
            }

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
