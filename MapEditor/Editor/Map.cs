using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Editor
{
    public class Map
    {
        public MapData MapData { get; private set; }
        public readonly List<Level> Levels = new();

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
        public (int, int) Render(RectangleF cameraBounds, Image<Rgba32> image)
        {
            int levelCount = 0, entityCount = 0;
            foreach (Level level in Levels)
            {
                if (!cameraBounds.IntersectsWith(level.Bounds))
                    continue;

                entityCount += level.Render(cameraBounds, image);
                levelCount++;
            }
            return (levelCount, entityCount);
        }
    }
}
