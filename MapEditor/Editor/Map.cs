using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace Editor
{
    public class Map
    {
        public MapData MapData { get; private set; }
        public readonly List<Level> Levels = new();
        public List<Rectangle> Fillers => MapData.Fillers;

        public Map(MapData data)
        {
            MapData = data;

            foreach (LevelData levelData in MapData.Levels)
                Levels.Add(new(levelData));
        }

        /// <summary>
        /// See <see cref="Level.Render(RectangleF, Image{Rgba32})"/>.
        /// </summary>
        /// <returns>
        /// A tuple cotaining the number of levels and entities that were rendererd.
        /// </returns>
        public void Render(RectangleF cameraBounds, Image<Rgba32> image)
        {
            foreach (Level level in Levels)
            {
                if (!cameraBounds.IntersectsWith(level.Bounds))
                    continue;

                level.Render(cameraBounds, image);
            }
            foreach (Rectangle filler in Fillers)
            {
                if (!cameraBounds.IntersectsWith(filler))
                    continue;

                image.Mutate(o => o.DrawPolygon(Color.Brown, 1, filler.ToPointFArray()));
            }
        }
    }
}
