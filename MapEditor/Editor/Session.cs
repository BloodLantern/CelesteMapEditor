using Editor.Celeste;
using Editor.Logging;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Xml.Serialization;

namespace Editor
{
    public class Session
    {
        public static Session Current { get; private set; }

        private static readonly string OlympusConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Olympus", "config.json");
        
        public MapEditor MapEditor;
        public Config Config;

        public string CelesteDirectory;
        public string CelesteContentDirectory;
        public string CelesteGraphicsDirectory;

        private bool debugContentLoaded = false;
        public SpriteFont DebugFont;

        public Session(MapEditor mapEditor)
        {
            if (Current != null)
                throw new InvalidOperationException("The Session class can only be instantiated once.");
            Current = this;

            MapEditor = mapEditor;

            Config = Config.Load();

            if (!TryGetActiveCelesteDirectory(out CelesteDirectory))
            {
                Logger.Log("Could not get the active Celeste directory using the Olympus config file. Stopping program.", LogLevel.Fatal);
                Current = null;
                return;
            }

            CelesteContentDirectory = Path.Combine(CelesteDirectory, "Content");
            CelesteGraphicsDirectory = Path.Combine(CelesteContentDirectory, "Graphics");

            Atlas.LoadAtlases(CelesteGraphicsDirectory);
            Autotiler.LoadAutotilers(CelesteGraphicsDirectory);
        }

        public void LoadContent(ContentManager content)
        {
            if (Config.DebugMode)
                LoadDebugContent(content);
        }

        public void LoadDebugContent(ContentManager content)
        {
            if (debugContentLoaded)
                return;

            DebugFont = content.Load<SpriteFont>("Fonts/DebugFont");

            debugContentLoaded = true;
        }

        /// <summary>
        /// Reads the Olympus config file to find the current Celeste directory.
        /// </summary>
        /// <returns>The path to the active Celeste directory.</returns>
        private bool TryGetActiveCelesteDirectory(out string activeCelesteDirectory)
        {
            activeCelesteDirectory = string.Empty;

            JsonDocument doc = JsonDocument.Parse(File.OpenRead(OlympusConfigFilePath), new JsonDocumentOptions() { CommentHandling = JsonCommentHandling.Skip });

            int celesteInstallID = -1;
            List<string> celesteInstalls = new();

            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("install", out JsonElement install))
            {
                Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'install' does not exist.", LogLevel.Error);
                return false;
            }

            if (!install.TryGetInt32(out celesteInstallID))
            {
                Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'install' is not of type {nameof(Int32)}.", LogLevel.Error);
                return false;
            }

            if (!root.TryGetProperty("installs", out JsonElement installs))
            {
                Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'installs' does not exist.", LogLevel.Error);
                return false;
            }

            if (installs.ValueKind != JsonValueKind.Array)
            {
                Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'installs' is not of type {nameof(JsonValueKind.Array)}.", LogLevel.Error);
                return false;
            }

            foreach (JsonElement installEntry in installs.EnumerateArray())
            {
                if (!installEntry.TryGetProperty("path", out JsonElement pathProperty))
                {
                    Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'path' does not exist.", LogLevel.Error);
                    return false;
                }

                string path = pathProperty.GetString();

                if (path == null)
                {
                    Logger.Log($"Error while reading Olympus config file: Celeste install path is not of type string.", LogLevel.Error);
                    return false;
                }

                celesteInstalls.Add(path);
            }

            activeCelesteDirectory = celesteInstalls[Math.Clamp(celesteInstallID - 1, 0, celesteInstalls.Count)];
            return true;
        }

        public void Exit()
        {
            Config.Save();
        }
    }
}
