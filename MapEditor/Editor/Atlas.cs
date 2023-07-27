using Microsoft.Xna.Framework;
using Editor.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Celeste
{
    public class Atlas
    {
        private static Atlas gameplay = new();
        /// <summary>
        /// The vanilla gameplay Atlas.
        /// </summary>
        public static Atlas Gameplay { get { return gameplay; } }

        public readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);

        public static void LoadAtlases(string celesteGraphicsDirectory)
        {
            Logger.Log("Loading atlases");
            Stopwatch stopwatch = Stopwatch.StartNew();

            Texture.AllocateBuffers();
            LoadAtlas(ref gameplay, Path.Combine(celesteGraphicsDirectory, "Atlases", "Gameplay"));
            Texture.DeallocateBuffers();

            Logger.Log($"Finished loading atlases. Took {stopwatch.ElapsedMilliseconds}ms");
        }

        private static void LoadAtlas(ref Atlas atlas, string path)
        {
            using FileStream fileStream = File.OpenRead(path + ".meta");
            BinaryReader reader = new(fileStream);

            reader.ReadInt32();
            reader.ReadString();
            reader.ReadInt32();

            short atlasCount = reader.ReadInt16();
            for (int i = 0; i < atlasCount; i++)
            {
                string dataFileName = reader.ReadString();
                Texture parentTexture = new(Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, dataFileName + ".data"));

                short childCount = reader.ReadInt16();
                for (int j = 0; j < childCount; j++)
                {
                    string texturePath = reader.ReadString().Replace('\\', '/');

                    Point clipPosition = new(reader.ReadInt16(), reader.ReadInt16());
                    Point clipSize = new(reader.ReadInt16(), reader.ReadInt16());

                    Point offset = new(-reader.ReadInt16(), -reader.ReadInt16());
                    Point textureSize = new(reader.ReadInt16(), reader.ReadInt16());

                    atlas.Textures[texturePath] = new Texture(parentTexture, new Rectangle(clipPosition, clipSize), offset, textureSize);
                }
            }
        }

        public Texture this[string key]
        {
            get => Textures[key];
        }

        public List<Texture> GetAtlasSubtextures(string key)
        {
            List<Texture> subtextures = new();
            for (int i = 0; ; i++)
            {
                Texture subtexture = GetAtlasSubtextureFromAtlasAt(key, i);

                if (subtexture == null)
                    break;

                subtextures.Add(subtexture);
            }
            return subtextures;
        }

        private Texture GetAtlasSubtextureFromAtlasAt(string key, int index)
        {
            if (index == 0 && Textures.ContainsKey(key))
                return Textures[key];

            string indexStr = index.ToString();
            for (int length = indexStr.Length; indexStr.Length < length + 6; indexStr = "0" + indexStr)
            {
                if (Textures.TryGetValue(key + indexStr, out Texture subtextureFromAtlasAt))
                    return subtextureFromAtlasAt;
            }

            return null;
        }
    }
}
