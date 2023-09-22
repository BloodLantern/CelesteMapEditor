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
    public class Selection<T> : IEnumerable<T>
        where T : Entity
    {
        private RectangleF area;
        private Vector2 clickStart;
        private readonly List<T> areaList = new();
        private readonly List<T> list = new();

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
                    T entity = (T) mapViewer.GetEntityAt(camera.WindowToMap(mouse.Position).ToVector2());

                    if (keyboard.IsControlDown())
                        Select(entity);
                    else
                        SelectOnly(entity);
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

                    List<Entity> entitiesInRectangle = mapViewer.GetEntitiesInArea(camera.WindowToMap(area));
                    entitiesInRectangle.ForEach(e => { if (!areaList.Contains(e)) areaList.Add((T) e); });
                    for (int i = 0; i < areaList.Count; )
                    {
                        T e = areaList[i];
                        if (!entitiesInRectangle.Contains(e))
                            areaList.Remove(e);
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

            foreach (T entity in list)
                spriteBatch.DrawRectangle(camera.MapToWindow(entity.AbsoluteBounds), color, camera.GetLineThickness());

            if (!area.IsEmpty)
            {
                spriteBatch.FillRectangle(area, config.EntitySelectionBoundsColorMin * 0.5f);
                spriteBatch.DrawRectangle(area, config.EntitySelectionBoundsColorMax * 0.5f, 2f);
            }
        }

        public void Select(T entity)
        {
            if (entity != null && !list.Contains(entity))
                list.Add(entity);
        }

        public void Select(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
                Select(entity);
        }

        public void Deselect(T entity)
        {
            if (entity != null)
                list.Remove(entity);
        }

        public void Deselect(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
                Deselect(entity);
        }

        public void SelectOnly(T entity)
        {
            list.Clear();
            Select(entity);
        }

        public void DeselectAll() => list.Clear();

        public bool Empty() => list.Empty();

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public T this[int index] => list[index];
    }
}
