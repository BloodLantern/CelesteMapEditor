using Editor.Celeste;
using Editor.Extensions;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Editor
{
    public class MapViewer
    {
        public Session Session;
        public MapEditor MapEditor;
        public Map CurrentMap;
        public Camera Camera;

        private Point2? cameraStartPosition;
        private Point? clickStartPosition;
        private bool Dragging => cameraStartPosition.HasValue && clickStartPosition.HasValue;
        private Vector2 dragDelta;

        private List<Level> visibleLevels;
        private List<Rectangle> visibleFillers;
        private readonly List<Entity> visibleEntities = new();
        private readonly List<Vector2> visiblePlayerSpawns = new();

        public static readonly Point PlayerSpawnSize = new(13, 17);
        public static readonly Vector2 PlayerSpawnOffset = new(-PlayerSpawnSize.X / 2, -PlayerSpawnSize.Y);
        private readonly Texture playerSpawnSprite;

        private Entity selectedEntity;

        public MapViewer(MapEditor mapEditor)
        {
            MapEditor = mapEditor;
            Session = mapEditor.Session;

            Camera = new(Vector2.Zero, 0.00001f);
            Camera.ZoomToDefault(Camera.DefaultZoomDuration * 3);

            playerSpawnSprite = new(Atlas.Gameplay["characters/player/fallPose10"], 8, 15, PlayerSpawnSize.X, PlayerSpawnSize.Y);
        }

        public void Update(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            visibleLevels = CurrentMap.GetVisibleLevels(Camera.Bounds);
            visibleFillers = CurrentMap.GetVisibleFillers(Camera.Bounds);
            visibleEntities.Clear();
            visiblePlayerSpawns.Clear();
            foreach (Level level in visibleLevels)
            {
                visibleEntities.AddRange(level.GetVisibleEntities(Camera.Bounds));
                visiblePlayerSpawns.AddRange(level.GetVisiblePlayerSpawns(Camera.Bounds));
            }

            ImGuiIOPtr imGuiIO = ImGui.GetIO();
            if (imGuiIO.WantCaptureMouse || imGuiIO.WantCaptureKeyboard)
                return;

            HandleInputs(time, mouse, keyboard);
        }

        private void HandleInputs(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
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
            if (clickStartPosition.HasValue && new RectangleF(Point.Zero, MapEditor.WindowSize).Contains(clickStartPosition.Value))
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

        private bool showEntityBounds = false;
        public void Render(SpriteBatch spriteBatch)
        {
            foreach (Level level in visibleLevels)
                level.RenderBackground(spriteBatch, Camera);

            foreach (Vector2 playerSpawn in visiblePlayerSpawns)
                RenderPlayerSpawn(spriteBatch, Camera, playerSpawn);

            foreach (Entity entity in visibleEntities)
                entity.Render(spriteBatch, Camera);

            foreach (Level level in visibleLevels)
                level.RenderForeground(spriteBatch, Camera);

            // TODO Draw filler tiles

            if (Session.Config.DebugMode)
            {
                foreach (Level level in visibleLevels)
                    level.RenderDebug(spriteBatch, Camera);

                if (showEntityBounds)
                {
                    foreach (Entity entity in visibleEntities)
                        entity.RenderDebug(spriteBatch, Camera);
                }

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

        private float cameraZoomSizeEqualTolerance = 1E-03f;
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

            ImGui.Checkbox("Show entity bounds", ref showEntityBounds);

            ImGui.Separator();

            ImGui.Text($"Mouse window position: {mousePosition}");
            ImGui.Text($"Mouse map position: {mouseMapPosition}");
            ImGui.Text("Mouse level position: " + (hoveredLevel != null ? (mouseMapPosition - hoveredLevel.Position).ToString() : "N/A"));

            ImGui.Separator();

            ImGui.Text($"Camera bounds: {Camera.Bounds}");
            if (ImGui.Button("Reset"))
                Camera.ZoomToDefault();
            ImGui.SameLine();
            ImGui.Text($"Camera zoom factor: {Camera.Zoom}");

            Vector2 cameraZoomToSize = Camera.ZoomToSize(Camera.Zoom);
            bool cameraZoomToSizeEqual = cameraZoomToSize.EqualsWithTolerence(Camera.Size, cameraZoomSizeEqualTolerance);
            float cameraSizeToZoom = Camera.SizeToZoom(Camera.Size);
            bool cameraSizeToZoomEqual = cameraSizeToZoom.EqualsWithTolerance(Camera.Zoom, cameraZoomSizeEqualTolerance);
            ImGui.Text($"Camera zoom to size: {cameraZoomToSize}, equal: {cameraZoomToSizeEqual}");
            ImGui.Text($"Camera size to zoom: {cameraSizeToZoom}, equal: {cameraSizeToZoomEqual}");
            ImGui.Text($"Camera size & zoom sync: {cameraZoomToSizeEqual && cameraSizeToZoomEqual}");
            ImGui.InputFloat("Camera size & zoom tolerance", ref cameraZoomSizeEqualTolerance, 0.001f, 0.01f, "%f");

            ImGui.Separator();

            if (selectedEntity != null)
            {
                ImGui.Text($"Selected entity name: {selectedEntity.Name}");
                ImGui.Text($"Selected entity level position: {selectedEntity.Position}");
                ImGui.Text($"Selected entity absolute position: {selectedEntity.AbsolutePosition}");
                ImGui.Text($"Selected entity relative position: {selectedEntity.AbsolutePosition - Camera.Position}");
                ImGui.Text($"Selected entity origin: {selectedEntity.EntityData.Origin}");
                ImGui.Text($"Selected entity size: {selectedEntity.Size}");
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
                        entity.DebugInfo();

                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Active coroutines: {Coroutine.RunningCount}");

            ImGui.End();
        }

        public static Point ToTilePosition(Point position) => position.Div(Tileset.TileSize);

        public static Vector2 ToTilePosition(Vector2 position) => position / Tileset.TileSize;

        public static Point ToPixelPosition(Point position) => position.Mul(Tileset.TileSize);

        public static Vector2 ToPixelPosition(Vector2 position) => position * Tileset.TileSize;
    }
}
