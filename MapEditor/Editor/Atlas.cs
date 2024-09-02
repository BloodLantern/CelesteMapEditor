using Microsoft.Xna.Framework;
using Editor.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Editor.Celeste
{
    public class Atlas
    {
        /// <summary>
        /// The vanilla gameplay Atlas.
        /// </summary>
        public static Atlas Gameplay { get; private set; }

        public readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);

        public static void LoadVanillaAtlases(string celesteGraphicsDirectory, Loading loading, float progressFactor)
        {
            Logger.Log("Loading atlases");
            Stopwatch stopwatch = Stopwatch.StartNew();

            Texture.AllocateBuffers();
            Gameplay = LoadAtlasWithMeta(Path.Combine(celesteGraphicsDirectory, "Atlases", "Gameplay"), loading, progressFactor);
            loading.CurrentSubText = string.Empty;
            Texture.DeallocateBuffers();

            Logger.Log($"Finished loading atlases. Took {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Load an Atlas using a meta file.
        /// </summary>
        /// <param name="path">The path to the meta file.</param>
        /// <returns>The loaded Atlas.</returns>
        private static Atlas LoadAtlasWithMeta(string path, Loading loading, float progressFactor)
        {
            string filePath = path + ".meta";
            loading.CurrentSubText = filePath;
            using FileStream fileStream = File.OpenRead(filePath);
            BinaryReader reader = new(fileStream);

            reader.ReadInt32();
            reader.ReadString();
            reader.ReadInt32();

            Atlas result = new();
            short atlasCount = reader.ReadInt16();
            float atlasCountInverse = 1f / atlasCount;
            for (int i = 0; i < atlasCount; i++)
            {
                string dataFileName = reader.ReadString();
                loading.CurrentSubText = dataFileName;
                Texture parentTexture = new(Session.Current.CelesteContentDirectory, Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, dataFileName + ".data"));

                short childCount = reader.ReadInt16();
                float childCountInverse = 1f / childCount;
                for (int j = 0; j < childCount; j++)
                {
                    string texturePath = reader.ReadString().Replace('\\', '/');
                    loading.CurrentSubText = texturePath;

                    Point clipPosition = new(reader.ReadInt16(), reader.ReadInt16());
                    Point clipSize = new(reader.ReadInt16(), reader.ReadInt16());

                    Point offset = new(-reader.ReadInt16(), -reader.ReadInt16());
                    Point textureSize = new(reader.ReadInt16(), reader.ReadInt16());

                    result.Textures[texturePath] = new(parentTexture, new(clipPosition, clipSize), offset, textureSize);
                    loading.Progress += progressFactor * atlasCountInverse * childCountInverse;
                }
            }

            return result;
        }

        /// <summary>
        /// Loads an Atlas from a folder.
        /// </summary>
        /// <param name="path">The path to the Atlas' root folder.</param>
        /// <returns>The loaded Atlas.</returns>
        public static Atlas LoadAtlas(string path)
        {
            Atlas result = new();

            foreach (string file in Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories))
            {
                result.Textures[Path.ChangeExtension(file, string.Empty)] = new(Path.GetDirectoryName(path), file);
            }

            return result;
        }

        public Texture this[string key] => Textures[key];

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
