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

                    list.RemoveAll(x => areaList.Contains(x));

                    List<Entity> entities = mapViewer.GetEntitiesIn(camera.WindowToMap(area));
                    entities.ForEach(e => { if (!areaList.Contains(e)) areaList.Add((T) e); });
                    for (int i = 0; i < areaList.Count; i++)
                    {
                        T e = areaList[i];
                        if (!entities.Contains(e))
                        {
                            areaList.Remove(e);
                            i--;
                        }
                    }

                    list.AddRange(areaList);
                }
                else
                {
                    area = new();
                    areaList.Clear();
                }
            }

            if (mouse.WasButtonJustUp(config.SelectButton))
                area = new();
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
            if (entity != null)
                list.Add(entity);
        }

        public void SelectOnly(T entity)
        {
            list.Clear();
            Select(entity);
        }

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
