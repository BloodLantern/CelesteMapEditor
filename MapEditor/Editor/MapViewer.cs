using Editor.Celeste;
using Editor.Extensions;
using Editor.Logging;
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
        private enum Layers : byte
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
        private enum DebugLayers : byte
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

        public Session Session;
        public Application App;
        public Map CurrentMap;
        public Camera Camera;

        private List<Level> visibleLevels = new();
        private List<Filler> visibleFillers = new();
        private readonly List<MapObject> visibleObjects = new();
        private readonly List<Entity> visibleEntities = new();
        private readonly List<Trigger> visibleTriggers = new();
        private readonly List<PlayerSpawn> visiblePlayerSpawns = new();

        private Selection selection;

        private Layers renderedLayers = Layers.All;
        private DebugLayers renderedDebugLayers = DebugLayers.LevelBounds | DebugLayers.FillerBounds;

        public MapViewer(Application app)
        {
            App = app;
            Session = app.Session;

            PlayerSpawn.SetupSprite();
        }

        public void InitializeCamera()
        {
            Camera = new(App, Vector2.Zero, 1e-6f);
            Camera.ZoomToDefault(Camera.DefaultZoomDuration * 3);

            selection = new(this);
        }

        public void Update(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            if (CurrentMap == null)
                return;

            visibleLevels = CurrentMap.GetVisibleLevels(Camera.Bounds);
            visibleFillers = CurrentMap.GetVisibleFillers(Camera.Bounds);

            visibleObjects.Clear();
            visibleEntities.Clear();
            visibleTriggers.Clear();
            visiblePlayerSpawns.Clear();

            foreach (Level level in visibleLevels)
            {
                visibleEntities.AddRange(level.GetVisibleEntities(Camera.Bounds));
                visibleTriggers.AddRange(level.GetVisibleTriggers(Camera.Bounds));
                visiblePlayerSpawns.AddRange(level.GetVisiblePlayerSpawns(Camera.Bounds));
            }

            visibleObjects.AddRange(visibleEntities);
            visibleObjects.AddRange(visibleTriggers);
            visibleObjects.AddRange(visiblePlayerSpawns);

            ImGuiIOPtr imGuiIO = ImGui.GetIO();
            if (imGuiIO.WantCaptureMouse || imGuiIO.WantCaptureKeyboard)
                return;

            HandleInputs(mouse, keyboard);
        }

        private void HandleInputs(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            Camera.HandleInputs(mouse, keyboard);

            selection.HandleInputs(mouse, keyboard);
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

            selection.Render(time, spriteBatch);

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
                new RectangleF(Camera.MapToWindow(filler.Location.ToVector2()), filler.Size.Mul(Camera.Zoom)),
                Color.Brown,
                Math.Max(camera.Zoom, 1f)
            );

        public void RenderDebug()
        {
            MouseStateExtended mouseState = MouseExtended.GetState();
            Point mousePosition = mouseState.Position;
            Point mouseMapPosition = Camera.WindowToMap(mousePosition);
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

            if (!selection.Empty() && ImGui.TreeNode("Selected objects"))
            {
                for (int i = 0; i < selection.Count; i++)
                {
                    if (selection.Count == 1 || ImGui.TreeNode($"{i}"))
                    {
                        MapObject obj = selection[i];
                        ImGui.Text($"Type: {obj.GetType()}");
                        ImGui.Text($"Level position: {obj.Position}");
                        ImGui.Text($"Absolute position: {obj.AbsolutePosition}");
                        ImGui.Text($"Relative position: {obj.AbsolutePosition - Camera.Position}");
                        ImGui.Text($"Size: {obj.Size}");

                        if (selection.Count > 1)
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
                                Camera.ZoomTo(Camera.MapToWindow(obj.Center), 3f);
                            selection.SelectOnly(obj);
                        }

                        obj.DebugInfo();

                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Active coroutines: {Coroutine.RunningCount}");
            ImGui.Checkbox("Show debug console", ref Session.Config.ShowDebugConsole);
            ImGui.Checkbox("Show average fps", ref Session.Config.ShowAverageFps);
            if (ImGui.Button("Test logging"))
                Logger.Test();

            if (ImGui.BeginCombo("UI Style", Session.Config.UiStyle.ToString()))
            {
                foreach (ImGuiStyles.Style style in Enum.GetValues(typeof(ImGuiStyles.Style)))
                {
                    if (ImGui.Selectable(style.ToString()))
                        ImGuiStyles.Setup(Session.Config.UiStyle = style);
                }
                ImGui.EndCombo();
            }

            ImGui.End();
        }

        public static Point ToTilePosition(Point position) => position.Div(Tileset.TileSize);

        public static Vector2 ToTilePosition(Vector2 position) => position / Tileset.TileSize;

        public static Point ToPixelPosition(Point position) => position.Mul(Tileset.TileSize);

        public static Vector2 ToPixelPosition(Vector2 position) => position * Tileset.TileSize;
    }
}
