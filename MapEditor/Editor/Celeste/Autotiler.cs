using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Editor.Celeste
{
    public partial class Autotiler
    {
        /// <summary>
        /// THe object representation of a tileset's XML data.
        /// </summary>
        private class TerrainType
        {
            public char ID;
            public HashSet<char> Ignores = new();
            public List<MaskedTile> Masked = new();
            public Tile Padded = new();
            public Tile Center = new();

            public TerrainType(char id) => ID = id;

            public Texture GetFromPosition(char[,] around)
            {
                return Calc.Random.Choose(Center.Textures);
            }

            public bool Links(char c)
            {
                if (ID == c)
                    return true;
                return Ignores.Contains(c) || Ignores.Contains('*');
            }
        }

        private class MaskedTile
        {
            public byte[] Mask = new byte[9];
            public Tile Tile = new();
        }

        private class Tile
        {
            public List<Texture> Textures = new();
            public List<string> OverlapSprites = new();
            public bool HasOverlays;
        }

        public static Autotiler ForegroundTiles { get; private set; }
        public static Autotiler BackgroundTiles { get; private set; }
        public static Autotiler AnimatedTiles { get; private set; }

        private readonly Dictionary<char, TerrainType> lookup = new();

        public Autotiler(string filename)
        {
            Dictionary<char, XmlElement> loadedTilesets = new();
            foreach (XmlElement tilesetXml in Calc.LoadContentXML(filename).GetElementsByTagName("Tileset"))
            {
                char id = tilesetXml.AttrChar("id");
                Tileset tileset = new(Atlas.Gameplay["tilesets/" + tilesetXml.Attr("path")]);
                TerrainType data = new(id);

                ReadInto(data, tileset, tilesetXml);

                if (tilesetXml.HasAttr("copy"))
                {
                    char copyId = tilesetXml.AttrChar("copy");
                    if (!loadedTilesets.ContainsKey(copyId))
                        throw new Exception("Copied tilesets must be defined before the tilesets that copy them!");
                    ReadInto(data, tileset, loadedTilesets[copyId]);
                }

                if (tilesetXml.HasAttr("ignores"))
                {
                    string ignores = tilesetXml.Attr("ignores");
                    foreach (string ignore in ignores.Split(','))
                    {
                        if (ignore.Length > 0)
                            data.Ignores.Add(ignore[0]);
                    }
                }

                loadedTilesets.Add(id, tilesetXml);
                lookup.Add(id, data);
            }
        }

        private void ReadInto(TerrainType data, Tileset tileset, XmlElement xml)
        {
            foreach (object tilesetXmlObj in (XmlNode) xml)
            {
                if (tilesetXmlObj is not XmlComment)
                {
                    XmlElement tilesetXml = tilesetXmlObj as XmlElement;
                    string mask = tilesetXml.Attr("mask");
                    Tile tiles;

                    if (mask == "center")
                        tiles = data.Center;
                    else if (mask == "padding")
                        tiles = data.Padded;
                    else
                    {
                        MaskedTile masked = new();
                        tiles = masked.Tile;

                        int maskIndex = 0;
                        for (int i = 0; i < mask.Length; i++)
                        {
                            if (mask[i] == '0')
                                masked.Mask[maskIndex++] = 0;
                            else if (mask[i] == '1')
                                masked.Mask[maskIndex++] = 1;
                            else if (mask[i] is 'x' or 'X')
                                masked.Mask[maskIndex++] = 2;
                        }
                        data.Masked.Add(masked);
                    }

                    string tileOffsets = tilesetXml.Attr("tiles");
                    foreach (string tileOffset in tileOffsets.Split(';'))
                    {
                        string[] tileOffsetArray = tileOffset.Split(',');
                        int tileX = int.Parse(tileOffsetArray[0]);
                        int tileY = int.Parse(tileOffsetArray[1]);
                        tiles.Textures.Add(tileset[tileX, tileY]);
                    }

                    if (tilesetXml.HasAttr("sprites"))
                    {
                        string sprites = tilesetXml.Attr("sprites");
                        foreach (string sprite in sprites.Split(','))
                            tiles.OverlapSprites.Add(sprite);
                        tiles.HasOverlays = true;
                    }
                }
            }

            data.Masked.Sort((a, b) =>
            {
                // Number of any ('x' or 'X') masks
                int aAnyMasks = 0;
                int bAnyMasks = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (a.Mask[i] == 2)
                        aAnyMasks++;
                    if (b.Mask[i] == 2)
                        bAnyMasks++;
                }
                return aAnyMasks - bAnyMasks;
            });
        }

        /// <summary>
        /// Generates the tilemap for a level of a set size.
        /// </summary>
        /// <param name="tilesX">The width of the level in tiles.</param>
        /// <param name="tilesY">The height of the level in tiles.</param>
        /// <param name="data">The level tilemap data string.</param>
        /// <returns>A TileGrid with all its tiles set.</returns>
        public TileGrid GenerateLevel(
            int tilesX,
            int tilesY,
            string data)
        {
            Regex lineSeparator = LineSeparatorRegex();

            TileGrid result = new(tilesX, tilesY);

            // Read tilemap data into a char array
            string[] tileLines = lineSeparator.Split(data);
            char[,] tiles = new char[tilesX, tilesY];
            for (int y = 0; y < tileLines.Length; y++)
            {
                for (int x = 0; x < tileLines[y].Length; x++)
                    tiles[x, y] = tileLines[y][x];
            }

            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    char tile = tiles[x, y];
                    if (IsEmpty(tile))
                        continue;

                    char[,] around = new char[3, 3];
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            int tileX = Math.Clamp(x + i, 0, tiles.GetLength(0) - 1);
                            int tileY = Math.Clamp(y + j, 0, tiles.GetLength(1) - 1);

                            around[i + 1, j + 1] = tiles[tileX, tileY];
                        }
                    }

                    result.Tiles[x, y] = lookup[tile].GetFromPosition(around);
                }
            }

            return result;
        }

        private static bool IsEmpty(char id) => id is '0' or char.MinValue;

        public static void LoadAutotilers(string celesteGraphicsDirectory)
        {
            ForegroundTiles = new(Path.Combine(celesteGraphicsDirectory, "ForegroundTiles.xml"));
            BackgroundTiles = new(Path.Combine(celesteGraphicsDirectory, "BackgroundTiles.xml"));
            AnimatedTiles = new(Path.Combine(celesteGraphicsDirectory, "AnimatedTiles.xml"));
        }

        [GeneratedRegex("\\n|\\r|\\r\\n|\\n\\r")]
        private static partial Regex LineSeparatorRegex();
    }
}
