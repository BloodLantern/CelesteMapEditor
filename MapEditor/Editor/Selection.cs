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

                    mapViewer.GetEntitiesIn(camera.WindowToMap(area)).ForEach(e => { if (!list.Contains(e)) list.Add((T) e); });
                }
                else
                {
                    area = new();
                }
            }

            if (mouse.WasButtonJustUp(config.SelectButton))
                area = new();
        }

        public void Render(GameTime time, SpriteBatch spriteBatch)
        {
            foreach (T entity in list)
                spriteBatch.DrawRectangle(
                    camera.MapToWindow(entity.AbsoluteBounds),
                    Color.Lerp(config.EntitySelectionBoundsColorMin, config.EntitySelectionBoundsColorMax, Calc.YoYo((float) time.TotalGameTime.TotalSeconds % 1f)),
                    camera.GetLineThickness()
                );

            if (!area.IsEmpty)
            {
                spriteBatch.FillRectangle(area, config.EntitySelectionBoundsColorMin * 0.5f);
                spriteBatch.DrawRectangle(area, config.EntitySelectionBoundsColorMax * 0.5f, camera.GetLineThickness());
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
