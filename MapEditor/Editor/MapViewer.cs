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
            FillerBounds    = 0b00000010,
            EntityBounds    = 0b00000100,
            Unused1         = 0b00001000,
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

        private Point2? cameraStartPosition;
        private Point? clickStartPosition;
        private bool Dragging => cameraStartPosition.HasValue && clickStartPosition.HasValue;
        private Vector2 dragDelta;

        private List<Level> visibleLevels = new();
        private List<Filler> visibleFillers = new();
        private readonly List<Entity> visibleEntities = new();
        private readonly List<Trigger> visibleTriggers = new();
        private readonly List<Vector2> visiblePlayerSpawns = new();

        public static readonly Point PlayerSpawnSize = new(13, 17);
        public static readonly Vector2 PlayerSpawnOffset = new(-PlayerSpawnSize.X / 2, -PlayerSpawnSize.Y);
        private readonly Texture playerSpawnSprite;

        private Entity selectedEntity;

        private Layers renderedLayers = Layers.All;
        private DebugLayers renderedDebugLayers = DebugLayers.LevelBounds | DebugLayers.FillerBounds;

        public MapViewer(Application app)
        {
            App = app;
            Session = app.Session;

            playerSpawnSprite = new(Atlas.Gameplay["characters/player/fallPose10"], 8, 15, PlayerSpawnSize.X, PlayerSpawnSize.Y);
        }

        public void InitializeCamera()
        {
            Camera = new(Vector2.Zero, 1e-6f);
            Camera.ZoomToDefault(Camera.DefaultZoomDuration * 3);
        }

        public void Update(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            if (CurrentMap == null)
                return;

            visibleLevels = CurrentMap.GetVisibleLevels(Camera.Bounds);
            visibleFillers = CurrentMap.GetVisibleFillers(Camera.Bounds);
            visibleEntities.Clear();
            visibleTriggers.Clear();
            visiblePlayerSpawns.Clear();
            foreach (Level level in visibleLevels)
            {
                visibleEntities.AddRange(level.GetVisibleEntities(Camera.Bounds));
                visibleTriggers.AddRange(level.GetVisibleTriggers(Camera.Bounds));
                visiblePlayerSpawns.AddRange(level.GetVisiblePlayerSpawns(Camera.Bounds));
            }

            ImGuiIOPtr imGuiIO = ImGui.GetIO();
            if (imGuiIO.WantCaptureMouse || imGuiIO.WantCaptureKeyboard)
                return;

            HandleInputs(mouse, keyboard);
        }

        private void HandleInputs(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            if (mouse.IsButtonUp(Session.Config.CameraMoveButton))
            {
                cameraStartPosition = null;
                clickStartPosition = null;
            }
            else if (!Dragging)
            {
                cameraStartPosition = Camera.Position;
                clickStartPosition = mouse.Position;
            }
            
            // If the click started inside the window
            if (clickStartPosition.HasValue && new RectangleF(Point.Zero, App.WindowSize).Contains(clickStartPosition.Value))
            {
                if (Dragging)
                    dragDelta = (mouse.Position - clickStartPosition.Value).ToVector2() / Camera.Zoom;

                if (cameraStartPosition.HasValue)
                    Camera.Position = cameraStartPosition.Value - dragDelta;
            }

            if (mouse.WasButtonJustDown(Session.Config.SelectButton))
                selectedEntity = GetEntityAt(Camera.WindowToMap(mouse.Position).ToVector2());

            if (mouse.DeltaScrollWheelValue != 0)
                Camera.ZoomTo(mouse.Position.ToVector2(), Camera.TargetZoom * MathF.Pow(Session.Config.ZoomFactor, -mouse.DeltaScrollWheelValue / 120));
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

        public Entity GetEntityAt(Vector2 mapPosition)
        {
            foreach (Entity entity in visibleEntities)
            {
                if (entity.Bounds.Contains(mapPosition))
                    return entity;
            }
            return null;
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
                foreach (Vector2 playerSpawn in visiblePlayerSpawns)
                RenderPlayerSpawn(spriteBatch, Camera, playerSpawn);
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

            if (selectedEntity != null)
                spriteBatch.DrawRectangle(Camera.MapToWindow(selectedEntity.Bounds), Color.Lerp(Session.Config.EntitySelectionBoundsColorMin, Session.Config.EntitySelectionBoundsColorMax, Calc.YoYo((float) time.TotalGameTime.TotalSeconds % 1f)), Camera.GetLineThickness());

            // TODO Draw filler tiles

            if (renderedDebugLayers.HasFlag(DebugLayers.LevelBounds))
            {
                foreach (Level level in visibleLevels)
                    level.RenderDebug(spriteBatch, Camera);
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

        public void RenderPlayerSpawn(SpriteBatch spriteBatch, Camera camera, Vector2 position)
            => playerSpawnSprite.Render(spriteBatch, camera, position + PlayerSpawnOffset);

        public void RenderDebugPlayerSpawn(SpriteBatch spriteBatch, Camera camera, Vector2 position)
            => spriteBatch.DrawRectangle(
                new RectangleF(camera.MapToWindow(position + PlayerSpawnOffset), PlayerSpawnSize.Mul(camera.Zoom)),
                Color.DarkGreen,
                Math.Max(camera.Zoom, 1f)
            );

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
            Entity hoveredEntity = GetEntityAt(mouseMapPosition.ToVector2());
            Level hoveredLevel = GetLevelAt(mouseMapPosition.ToVector2());

            ImGui.Begin($"{nameof(MapViewer)} debug", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing);
            ImGui.Checkbox("Debug mode", ref Session.Config.DebugMode);

            ImGui.Separator();

            if (ImGui.TreeNode("Layers"))
            {
                int renderedLayersInt = (int) renderedLayers;
                ImGui.CheckboxFlags("Level background", ref renderedLayersInt, (int) Layers.LevelBackground);
                ImGui.CheckboxFlags("Level foreground", ref renderedLayersInt, (int) Layers.LevelForeground);
                ImGui.CheckboxFlags("Fillers", ref renderedLayersInt, (int) Layers.Fillers);
                ImGui.CheckboxFlags("Entities", ref renderedLayersInt, (int) Layers.Entities);
                ImGui.CheckboxFlags("Triggers", ref renderedLayersInt, (int) Layers.Triggers);
                ImGui.CheckboxFlags("Player spawns", ref renderedLayersInt, (int) Layers.PlayerSpawns);
                renderedLayers = (Layers) renderedLayersInt;

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Debug layers"))
            {
                int renderedDebugLayersInt = (int) renderedDebugLayers;
                ImGui.CheckboxFlags("Level bounds", ref renderedDebugLayersInt, (int) DebugLayers.LevelBounds);
                ImGui.CheckboxFlags("Filler bounds", ref renderedDebugLayersInt, (int) DebugLayers.FillerBounds);
                ImGui.CheckboxFlags("Entity bounds", ref renderedDebugLayersInt, (int) DebugLayers.EntityBounds);
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

            if (selectedEntity != null && ImGui.TreeNode("Selected entity"))
            {
                ImGui.Text($"Name: {selectedEntity.Name}");
                ImGui.Text($"Level position: {selectedEntity.Position}");
                ImGui.Text($"Absolute position: {selectedEntity.AbsolutePosition}");
                ImGui.Text($"Relative position: {selectedEntity.AbsolutePosition - Camera.Position}");
                ImGui.Text($"Origin: {selectedEntity.EntityData.Origin}");
                ImGui.Text($"Size: {selectedEntity.Size}");

                ImGui.TreePop();
            }

            ImGui.Text($"Visible levels: {visibleLevels.Count}");
            ImGui.Text($"Visible fillers: {visibleFillers.Count}");
            ImGui.Text($"Visible entities: {visibleEntities.Count}");
            if (ImGui.TreeNode($"Visible entities list"))
            {
                for (int i = 0; i < visibleEntities.Count; i++)
                {
                    Entity entity = visibleEntities[i];
                    if (ImGui.TreeNode($"{i}. {entity}"))
                    {
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            if (Camera.Zoom == 3f)
                                Camera.MoveTo(entity.Center);
                            else
                                Camera.ZoomTo(Camera.MapToWindow(entity.Center), 3f);
                            selectedEntity = entity;
                        }

                        entity.DebugInfo();

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
