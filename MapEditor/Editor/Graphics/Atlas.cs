using Editor.Logging;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Editor.Graphics
{
    public class Atlas
    {
        public const int ByteArraySize = 0x80000;
        public const int ByteArrayCheckSize = ByteArraySize - 0x20;

        private static Atlas gameplay = new();
        /// <summary>
        /// The vanilla gameplay Atlas.
        /// </summary>
        public static Atlas Gameplay { get { return gameplay; } }

        public readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);

        public static void LoadAtlases(string celesteContentDirectory)
        {
            byte[] buffer = new byte[0x04_00_00_00];
            byte[] buffer2 = new byte[ByteArraySize];

            Logger.Log("Loading atlases");
            Stopwatch stopwatch = Stopwatch.StartNew();

            LoadAtlas(ref gameplay, celesteContentDirectory, Path.Combine(celesteContentDirectory, "Graphics", "Atlases", "Gameplay"), ref buffer, ref buffer2);

            Logger.Log($"Finished loading atlases. Took {stopwatch.ElapsedMilliseconds}ms");
        }

        private static void LoadAtlas(ref Atlas atlas, string celesteContentDirectory, string path, ref byte[] buffer, ref byte[] buffer2)
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
                Texture parentTexture = new(celesteContentDirectory, Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, dataFileName + ".data"), ref buffer, ref buffer2);

                short childCount = reader.ReadInt16();
                for (int j = 0; j < childCount; j++)
                {
                    string texturePath = reader.ReadString().Replace('\\', '/');

                    Point clipPosition = new(reader.ReadInt16(), reader.ReadInt16());
                    Size clipSize = new(reader.ReadInt16(), reader.ReadInt16());

                    Point offset = new(-reader.ReadInt16(), -reader.ReadInt16());
                    Size textureSize = new(reader.ReadInt16(), reader.ReadInt16());

                    atlas.Textures[texturePath] = new Texture(parentTexture, new Rectangle(clipPosition, clipSize), offset, textureSize);
                }
            }
        }

        public Texture this[string key]
        {
            get => Textures[key];
        }
    }
}
