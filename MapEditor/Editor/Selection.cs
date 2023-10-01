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
        private List<MapObject> clickStartList;

        private readonly MapViewer mapViewer;
        private readonly Config config;
        private readonly Camera camera;

        public int Count => list.Count;

        private float selectionClickDuration = 0f;

        public Selection(MapViewer mapViewer)
        {
            this.mapViewer = mapViewer;
            config = mapViewer.Session.Config;
            camera = mapViewer.Camera;
        }

        public void HandleInputs(GameTime time, MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            Vector2 mousePos = mouse.Position.ToVector2();
            MapObject objectUnderMouse = mapViewer.GetObjectAt(camera.WindowToMap(mouse.Position).ToVector2());
            Vector2 mouseDragDelta = mousePos - clickStart;

            if (mouse.WasButtonJustDown(config.SelectButton))
            {
                selectionClickDuration = 0f;
                clickStart = mousePos;
                clickStartList = new(list);

                if (!keyboard.IsShiftDown())
                {
                    if (keyboard.IsControlDown())
                        Select(objectUnderMouse);
                }
            }

            if (mouse.IsButtonDown(config.SelectButton))
            {
                selectionClickDuration += time.GetElapsedSeconds();

                if (keyboard.IsShiftDown())
                {
                    if (area.IsEmpty)
                        area = new(mouse.Position, new());
                    else
                        area = new(Vector2.Min(clickStart, mousePos), Calc.Abs(mouseDragDelta));

                    Deselect(areaList);

                    List<MapObject> objectsInRectangle = mapViewer.GetObjectsInArea(camera.WindowToMap(area));

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

                    Select(areaList);
                }
                else if (objectUnderMouse != null)
                {
                    for (int i = 0; i < clickStartList.Count; i++)
                        list[i].Position = clickStartList[i].Position + camera.WindowToMap(mouseDragDelta);
                }
                else
                {
                    area = new();
                    areaList.Clear();
                }
            }

            if (mouse.WasButtonJustUp(config.SelectButton))
            {
                area = new();
                clickStartList.Clear();

                // If we clicked for a short amount of time and we didn't shift, select only the object under the mouse
                if (selectionClickDuration < 0.35f && !keyboard.IsShiftDown())
                    SelectOnly(mapViewer.GetObjectAt(camera.WindowToMap(mousePos)));
            }

            if (keyboard.WasKeyJustUp(config.DeselectKey))
                DeselectAll();
        }

        public void Render(GameTime time, SpriteBatch spriteBatch)
        {
            Color color = Color.Lerp(config.EntitySelectionBoundsColorMin, config.EntitySelectionBoundsColorMax, Calc.YoYo((float) time.TotalGameTime.TotalSeconds % 1f));

            foreach (MapObject obj in list)
                spriteBatch.DrawRectangle(camera.MapToWindow(obj.AbsoluteBounds), color, camera.GetLineThickness());

            if (!area.IsEmpty)
            {
                spriteBatch.FillRectangle(area, config.EntitySelectionBoundsColorMin * 0.5f);
                spriteBatch.DrawRectangle(area, config.EntitySelectionBoundsColorMax * 0.5f, 2f);
            }
        }

        public void Select(MapObject obj)
        {
            if (obj != null && !list.Contains(obj))
                list.Add(obj);
        }

        public void Select(IEnumerable<MapObject> entities)
        {
            foreach (MapObject obj in entities)
                Select(obj);
        }

        public void Deselect(MapObject obj)
        {
            if (obj != null)
                list.Remove(obj);
        }

        public void Deselect(IEnumerable<MapObject> entities)
        {
            foreach (MapObject obj in entities)
                Deselect(obj);
        }

        public void SelectOnly(MapObject obj)
        {
            list.Clear();
            Select(obj);
        }

        public void DeselectAll() => list.Clear();

        public bool Empty() => list.Empty();

        public IEnumerator<MapObject> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public MapObject this[int index] => list[index];
    }
}
