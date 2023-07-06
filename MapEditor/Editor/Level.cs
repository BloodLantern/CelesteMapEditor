using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Numerics;

namespace Editor
{
    public class Level
    {
        public LevelData LevelData { get; private set; }
        public readonly List<Entity> Entities = new();

        public Rectangle Bounds => LevelData.Bounds;
        public Vector2 Position => LevelData.Position;

        public Level(LevelData data)
        {
            LevelData = data;

            foreach (EntityData entityData in LevelData.Entities)
                Entities.Add(new(entityData, this));
        }

        /// <summary>
        /// Renders this Level to the passed Image, only rendering the entities
        /// located inside the given RectangleF.
        /// </summary>
        /// <param name="cameraBounds">
        /// The RectangleF representing the camera bounds. The entities are only rendered
        /// if they are located inside this rectangle.
        /// </param>
        /// <param name="image">The image to render to.</param>
        /// <returns>The number of entities that were rendered.</returns>
        public int Render(RectangleF cameraBounds, Image<Rgba32> image)
        {
            PointF relativePosition = new(Position.X - cameraBounds.X, Position.Y - cameraBounds.Y);

            if (Session.CurrentSession.Config.ShowDebugInfo)
                image.Mutate(o => o.DrawPolygon(Color.Magenta, 2,
                    new PointF(relativePosition.X, relativePosition.Y),
                    new PointF(relativePosition.X + Bounds.Width, relativePosition.Y),
                    new PointF(relativePosition.X + Bounds.Width, relativePosition.Y + Bounds.Height),
                    new PointF(relativePosition.X, relativePosition.Y + Bounds.Height)));

            int entityCount = 0;
            foreach (Entity entity in Entities)
            {
                if (entity.AbsolutePosition.X >= cameraBounds.Right
                    || entity.AbsolutePosition.Y >= cameraBounds.Bottom
                    || entity.AbsolutePosition.X + entity.Size.Width < cameraBounds.Left
                    || entity.AbsolutePosition.Y + entity.Size.Height < cameraBounds.Top)
                    continue;

                entity.Render(cameraBounds, image);
                entityCount++;
            }
            return entityCount;
        }
    }
}
