using Editor.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    public class Selection : IEnumerable<MapObject>
    {
        private RectangleF area;
        private Vector2 clickStart;
        private readonly List<MapObject> areaList = new();
        private readonly List<MapObject> list = new();

        private readonly MapViewer mapViewer;
        private readonly Config config;
        private readonly Camera camera;

        public int Count => list.Count;

        public Selection(MapViewer mapViewer)
        {
            this.mapViewer = mapViewer;
            config = mapViewer.Session.Config;
            camera = mapViewer.Camera;
        }

        public void HandleInputs(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            Vector2 mousePos = mouse.Position.ToVector2();

            if (mouse.WasButtonJustDown(config.SelectButton))
            {
                clickStart = mouse.Position.ToVector2();

                if (!keyboard.IsShiftDown())
                {
                    MapObject obj = mapViewer.GetObjectAt(camera.WindowToMap(mouse.Position).ToVector2());

                    if (keyboard.IsControlDown())
                        Select(obj);
                    else
                        SelectOnly(obj);
                }
            }

            if (mouse.IsButtonDown(config.SelectButton))
            {
                if (keyboard.IsShiftDown())
                {
                    if (area.IsEmpty)
                        area = new(mouse.Position, new());
                    else
                        area = new(Vector2.Min(clickStart, mousePos), Calc.Abs(clickStart - mousePos));

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
                else
                {
                    area = new();
                    areaList.Clear();
                }
            }

            if (mouse.WasButtonJustUp(config.SelectButton))
                area = new();

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

        public IEnumerator<MapObject> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public MapObject this[int index] => list[index];
    }
}
