﻿using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;

namespace Editor
{
    public class Map
    {
        public MapData MapData { get; }
        public readonly List<Level> Levels = new();
        public List<Filler> Fillers => MapData.Fillers;
        public string FilePath => MapData.FilePath;

        public Map(MapData data)
        {
            MapData = data;

            foreach (LevelData levelData in MapData.Levels)
                Levels.Add(new(levelData));
        }

        public List<Level> GetVisibleLevels(RectangleF cameraBounds) => Levels.FindAll(level => cameraBounds.Intersects(level.Bounds));

        public List<Filler> GetVisibleFillers(RectangleF cameraBounds) => Fillers.FindAll(filler => cameraBounds.Intersects(filler));
    }
}
