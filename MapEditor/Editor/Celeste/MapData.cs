using Microsoft.Xna.Framework;
using Editor.Logging;
using Editor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Editor.Celeste
{
    public class MapData
    {
        public AreaKey Area = AreaKey.None;
        public AreaData Metadata;
        public List<LevelData> Levels = new();
        public List<Rectangle> Fillers = new();
        public Color BackgroundColor = Color.Black;
        public BinaryPacker.Element Foreground;
        public BinaryPacker.Element Background;
        public string FilePath;

        public MapData(string filePath)
        {
            FilePath = filePath;
        }

        public int LevelCount
        {
            get
            {
                int levelCount = 0;
                foreach (LevelData level in Levels)
                {
                    if (!level.Dummy)
                        ++levelCount;
                }
                return levelCount;
            }
        }

        /// <summary>
        /// Loads the map data using the current <see cref="FilePath"/>.
        /// </summary>
        public bool Load()
        {
            Logger.Log($"Loading map: {FilePath}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!File.Exists(FilePath))
            {
                Logger.Log("Map file does not exist.", LogLevel.Error);
                return false;
            }

            Area.TryLoadFromFileName(Path.GetFileNameWithoutExtension(FilePath));

            BinaryPacker.Element element;
            try
            {
                element = BinaryPacker.FromBinary(FilePath);
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occured while trying to read map: {FilePath}", LogLevel.Error);
                Logger.Log($"Error message: {ex.Message}", LogLevel.Error);
                return false;
            }

            foreach (BinaryPacker.Element data in element.Children)
            {
                switch (data.Name)
                {
                    case "levels":
                        Levels = new List<LevelData>();
                        foreach (BinaryPacker.Element level in data.Children)
                            Levels.Add(new(level));
                        break;

                    case "Filler":
                        Fillers = new List<Rectangle>();
                        if (data.Children != null)
                        {
                            foreach (BinaryPacker.Element filler in data.Children)
                                Fillers.Add(
                                    new Rectangle(
                                        (int) filler.Attributes["x"] * Tileset.TileSize,
                                        (int) filler.Attributes["y"] * Tileset.TileSize,
                                        (int) filler.Attributes["w"] * Tileset.TileSize,
                                        (int) filler.Attributes["h"] * Tileset.TileSize
                                    )
                                );
                        }
                        break;

                    case "Style":
                        if (data.HasAttr("color"))
                            BackgroundColor = Calc.HexToColor(data.Attr("color"));

                        if (data.Children != null)
                        {
                            foreach (BinaryPacker.Element styleground in data.Children)
                            {
                                if (styleground.Name == "Backgrounds")
                                    Background = styleground;
                                else if (styleground.Name == "Foregrounds")
                                    Foreground = styleground;
                            }
                        }
                        break;

                    case "meta":
                        // If we hit this case we are loading a community map as all the base Celeste maps have hardcoded metadatas
                        Metadata = new(data);
                        Area.Campaign = "Uncategorized";
                        break;
                }
            }

            Logger.Log($"Finished loading map. Took {stopwatch.ElapsedMilliseconds}ms");

            return true;
        }

        /// <summary>
        /// Gets the number of strawberries. Outputs the total strawberry count through the parameter.
        /// </summary>
        /// <param name="total">The total strawberry count.</param>
        /// <returns>An array containing the strawberry count for each checkpoint.</returns>
        public int[] GetStrawberries(out int total)
        {
            total = 0;
            int[] strawberries = new int[10];
            foreach (LevelData level in Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "strawberry")
                    {
                        ++total;
                        ++strawberries[entity.Int("checkpointID")];
                    }
                }
            }
            return strawberries;
        }

        /// <summary>
        /// Get the level at position (0, 0).
        /// </summary>
        /// <returns></returns>
        public LevelData StartLevel() => GetAt(Vector2.Zero);

        /// <summary>
        /// Get the level at the given position.
        /// </summary>
        /// <param name="at">The position to get a level at.</param>
        /// <returns>The level at the given position.</returns>
        public LevelData GetAt(Vector2 at)
        {
            foreach (LevelData level in Levels)
            {
                if (level.Check(at))
                    return level;
            }
            return null;
        }

        /// <summary>
        /// Get the level with the given name.
        /// </summary>
        /// <param name="levelName">The name of the level to get.</param>
        /// <returns>The level with the given name.</returns>
        public LevelData Get(string levelName)
        {
            foreach (LevelData level in Levels)
            {
                if (level.Name.Equals(levelName))
                    return level;
            }
            return null;
        }
    }
}
