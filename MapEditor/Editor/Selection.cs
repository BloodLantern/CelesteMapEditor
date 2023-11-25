using Editor.Extensions;
using Editor.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System.Collections;
using System.Collections.Generic;

namespace Editor
{
    public class Selection : IEnumerable<MapObject>
    {
        private RectangleF area;
        private Vector2 clickStart;
        private readonly List<MapObject> areaList = new();
        private readonly List<MapObject> list = new();
        private List<Vector2> clickStartPositions = new();

        private readonly MapViewer mapViewer;
        private readonly Camera camera;

        public int Count => list.Count;

        private float selectionClickDuration = 0f;

        public Selection(MapViewer mapViewer)
        {
            this.mapViewer = mapViewer;
            camera = mapViewer.Camera;
        }

        public void HandleInputs(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            Vector2 mousePos = mouse.Position.ToVector2();
            MapObject objectUnderMouse = mapViewer.GetObjectAt(camera.WindowPositionToMap(mouse.Position).ToVector2());

            if (mouse.WasButtonJustDown(mapViewer.Keybinds.Select))
            {
                // If the click was on nothing, clear the selection
                if (objectUnderMouse == null)
                    DeselectAll();

                selectionClickDuration = 0f;
                clickStart = mousePos;
                clickStartPositions = new();
                foreach (MapObject mapObject in list)
                    clickStartPositions.Add(mapObject.Position);

                if (!keyboard.IsShiftDown() && keyboard.IsControlDown())
                    Select(objectUnderMouse);
            }

            Vector2 mouseDragDelta = mousePos - clickStart;

            if (mouse.IsButtonDown(mapViewer.Keybinds.Select))
            {
                selectionClickDuration += time.GetElapsedSeconds();

                if (keyboard.IsShiftDown())
                {
                    if (area.IsEmpty)
                        area = new(mouse.Position, new());
                    else
                        area = new(Vector2.Min(clickStart, mousePos), Calc.Abs(mouseDragDelta));

                    DeselectRange(areaList);

                    List<MapObject> objectsInRectangle = mapViewer.GetObjectsInArea(camera.WindowAreaToMap(area));

                    foreach (MapObject obj in objectsInRectangle)
                    {
                        if (!areaList.Contains(obj))
                            areaList.Add(obj);
                    }

                    for (int i = 0; i < areaList.Count; )
                    {
                        MapObject obj = areaList[i];
                        if (!objectsInRectangle.Contains(obj))
                            areaList.Remove(obj);
                        else
                            i++;
                    }

                    SelectRange(areaList);
                }
                else
                {
                    Vector2 offset = camera.WindowOffsetToMap(mouseDragDelta);
                    for (int i = 0; i < clickStartPositions.Count; i++)
                        list[i].Position = clickStartPositions[i] + offset;
                }
            }

            if (mouse.WasButtonJustUp(mapViewer.Keybinds.Select))
            {
                area = new();
                clickStartPositions.Clear();

                // If we clicked for a short amount of time and we didn't shift, select only the object under the mouse
                if (selectionClickDuration < 0.35f && !keyboard.IsShiftDown())
                    SelectOnly(mapViewer.GetObjectAt(camera.WindowPositionToMap(mousePos)));
            }

            if (keyboard.WasKeyJustUp(mapViewer.Keybinds.Deselect))
                DeselectAll();

            if (keyboard.WasKeyJustUp(mapViewer.Keybinds.Delete))
            {
                foreach (MapObject mapObject in list)
                    mapObject.RemoveFromMap();
            }
        }

        public void Render(GameTime time, SpriteBatch spriteBatch)
        {
            Color color = Color.Lerp(mapViewer.Config.EntitySelectionBoundsColorMin, mapViewer.Config.EntitySelectionBoundsColorMax, Calc.YoYo((float) time.TotalGameTime.TotalSeconds % 1f));

            foreach (MapObject obj in list)
                spriteBatch.DrawRectangle(camera.MapAreaToWindow(obj.AbsoluteBounds), color, camera.GetLineThickness());

            if (!area.IsEmpty)
            {
                spriteBatch.FillRectangle(area, mapViewer.Config.EntitySelectionBoundsColorMin * 0.5f);
                spriteBatch.DrawRectangle(area, mapViewer.Config.EntitySelectionBoundsColorMax * 0.5f, 2f);
            }
        }

        public void Select(MapObject obj)
        {
            if (obj != null && !list.Contains(obj))
                list.Add(obj);
        }

        public void SelectRange(IEnumerable<MapObject> entities)
        {
            foreach (MapObject obj in entities)
                Select(obj);
        }

        public void Deselect(MapObject obj)
        {
            if (obj != null)
                list.Remove(obj);
        }

        public void DeselectRange(IEnumerable<MapObject> entities)
        {
            foreach (MapObject obj in entities)
                Deselect(obj);
        }

        public void SelectOnly(MapObject obj)
        {
            list.Clear();
            Select(obj);
        }

        public void SelectAll() => SelectRange(mapViewer.ObjectsEnumerator);

        public void DeselectAll() => list.Clear();

        public bool Empty() => list.Empty();

        public IEnumerator<MapObject> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public MapObject this[int index] => list[index];
    }
}
