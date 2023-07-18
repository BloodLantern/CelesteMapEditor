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
        public TileGrid ForegroundTiles { get; private set; }
        public TileGrid BackgroundTiles { get; private set; }

        public Rectangle Bounds => LevelData.Bounds;
        public Point Position => LevelData.Position;

        public Level(LevelData data)
        {
            LevelData = data;

            foreach (EntityData entityData in LevelData.Entities)
                Entities.Add(new(entityData, this));

            ForegroundTiles = Autotiler.ForegroundTiles.GenerateLevel(data.TileBounds.Width, data.TileBounds.Height, LevelData.ForegroundTiles);
            BackgroundTiles = Autotiler.BackgroundTiles.GenerateLevel(data.TileBounds.Width, data.TileBounds.Height, LevelData.BackgroundTiles);
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
        public void Render(RectangleF cameraBounds, Image<Rgba32> image)
        {
            PointF relativePosition = new(Position.X - cameraBounds.X, Position.Y - cameraBounds.Y);

            if (Session.CurrentSession.Config.ShowDebugInfo)
                image.Mutate(o => o.DrawPolygon(Color.Magenta, 2,
                    new PointF(relativePosition.X, relativePosition.Y),
                    new PointF(relativePosition.X + Bounds.Width, relativePosition.Y),
                    new PointF(relativePosition.X + Bounds.Width, relativePosition.Y + Bounds.Height),
                    new PointF(relativePosition.X, relativePosition.Y + Bounds.Height)));

            foreach (Entity entity in Entities)
            {
                if (entity.AbsolutePosition.X >= cameraBounds.Right
                    || entity.AbsolutePosition.Y >= cameraBounds.Bottom
                    || entity.AbsolutePosition.X + entity.Size.Width < cameraBounds.Left
                    || entity.AbsolutePosition.Y + entity.Size.Height < cameraBounds.Top)
                    continue;

                entity.Render(cameraBounds, image);
            }

            ForegroundTiles.Render(cameraBounds, image, Position);
        }
    }
}
