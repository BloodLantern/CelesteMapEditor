using Editor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
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

            public Texture GetTileFromMask(byte[] mask)
            {
                Tile result = null;

                for (int i = 0; i < mask.Length; i++)
                {
                    // The tile is neither padded nor centered if its mask has at least one empty tile
                    if (mask[i] is 0 or 2)
                    {
                        for (int j = 0; j < Masked.Count; j++)
                        {
                            bool same = true;
                            for (int k = 0; k < 3 * 3;  k++)
                            {
                                byte currentMask = Masked.ElementAt(j).Mask[k];
                                if (currentMask == 2 || mask[k] == 2)
                                    continue;

                                if (currentMask != mask[k])
                                {
                                    same = false;
                                    break;
                                }
                            }

                            if (same)
                                result = Masked.ElementAt(j).Tile;
                        }
                        break;
                    }
                }

                result ??= Center;

                return Calc.Random.Choose(result.Textures);
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
            public byte[] Mask;
            public Tile Tile;
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
                    Tile tile;

                    if (mask == "center")
                        tile = data.Center;
                    else if (mask == "padding")
                        tile = data.Padded;
                    else
                    {
                        MaskedTile maskedTile = new()
                        {
                            Tile = new(),
                            Mask = new byte[3 * 3]
                        };
                        tile = maskedTile.Tile;

                        int maskIndex = 0;
                        for (int i = 0; i < mask.Length; i++)
                        {
                            if (mask[i] is '0' or '1' or 'x' or 'X' or '*')
                                maskedTile.Mask[maskIndex++] = GetMask(mask[i]);
                        }

                        data.Masked.Add(maskedTile);
                    }

                    string tileOffsets = tilesetXml.Attr("tiles");
                    foreach (string tileOffset in tileOffsets.Split(';'))
                    {
                        string[] tileOffsetArray = tileOffset.Split(',');
                        int tileX = int.Parse(tileOffsetArray[0]);
                        int tileY = int.Parse(tileOffsetArray[1]);
                        tile.Textures.Add(tileset[tileX, tileY]);
                    }

                    if (tilesetXml.HasAttr("sprites"))
                    {
                        string sprites = tilesetXml.Attr("sprites");
                        foreach (string sprite in sprites.Split(','))
                            tile.OverlapSprites.Add(sprite);
                        tile.HasOverlays = true;
                    }
                }
            }
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

                    byte[] mask = new byte[3 * 3];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            // I don't know why 'i' and 'j' should be reversed here but it works
                            int tileX = Math.Clamp(x + j - 1, 0, tiles.GetLength(0) - 1);
                            int tileY = Math.Clamp(y + i - 1, 0, tiles.GetLength(1) - 1);

                            mask[i * 3 + j] = GetMaskFromIds(tile, tiles[tileX, tileY]);
                        }
                    }

                    result.Tiles[x, y] = lookup[tile].GetTileFromMask(mask);
                }
            }

            return result;
        }

        private static bool IsEmpty(char id) => id is '0' or char.MinValue;

        private byte GetMask(char mask) => GetMaskFromIds('1', mask);

        private static byte GetMaskFromIds(char id, char maskId) => (byte) (maskId == '0' ? 0 : (id == maskId ? 1 : 2));

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
