using Editor.Celeste;
using Editor.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Editor
{
    public class Autotiler
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
            public Tileset Tileset;

            public TerrainType(char id, Tileset tileset)
            {
                ID = id;
                Tileset = tileset;
            }

            public Texture GetTileFromMask(byte[] mask)
            {
                Tile result = null;

                for (int i = 0; i < 3 * 3; i++)
                {
                    // The tile is neither padded nor centered if its 3x3 mask has at least one empty tile
                    if (mask[i + (3 + i / 3) * 2] == 0)
                    {
                        for (int j = 0; j < Masked.Count; j++)
                        {
                            bool same = true;
                            for (int k = 0; k < 3 * 3; k++)
                            {
                                byte currentMask = Masked[j].Mask[k];
                                int maskIndex = k + (3 + k / 3) * 2;
                                if (currentMask == 2 || mask[maskIndex] == 2)
                                    continue;

                                if (currentMask != mask[maskIndex])
                                {
                                    same = false;
                                    break;
                                }
                            }

                            if (same)
                                result = Masked[j].Tile;
                        }
                        break;
                    }
                }

                // If it is considered as a center tile, check if it is instead a padded one
                if (result == null)
                {
                    if (mask[2] == 0
                        || mask[10] == 0
                        || mask[14] == 0
                        || mask[22] == 0)
                        result = Padded;
                }

                result ??= Center;

                return Calc.Random.Choose(result.Textures);
            }

            public byte GetMask(char otherId)
            {
                if (otherId is '0' or char.MinValue)
                    return 0;

                if (otherId == ID)
                    return 1;

                if (Ignores.Contains('*') || Ignores.Contains(otherId))
                    return 0;

                return 1;
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
                TerrainType data = new(id, tileset);

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

        private static void ReadInto(TerrainType data, Tileset tileset, XmlElement xml)
        {
            foreach (object tilesetXmlObj in (XmlNode)xml)
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
                            if (mask[i] == '0')
                                maskedTile.Mask[maskIndex++] = 0;
                            else if (mask[i] == '1')
                                maskedTile.Mask[maskIndex++] = 1;
                            else if (mask[i] is 'x' or 'X')
                                maskedTile.Mask[maskIndex++] = 2;
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
            Regex lineSeparator = new("\\n|\\r|\\r\\n|\\n\\r");

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

                    TerrainType terrainType = lookup[tile];

                    byte[] mask = new byte[5 * 5];
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int tileX = Math.Clamp(x + j - 2, 0, tiles.GetLength(0) - 1);
                            int tileY = Math.Clamp(y + i - 2, 0, tiles.GetLength(1) - 1);

                            mask[i * 5 + j] = terrainType.GetMask(tiles[tileX, tileY]);
                        }
                    }

                    result.Tiles[x, y] = terrainType.GetTileFromMask(mask);
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
    }
}
