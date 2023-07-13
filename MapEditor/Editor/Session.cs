using Editor.Celeste;
using Editor.Logging;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace Editor
{
    public class Session
    {
        public static Session CurrentSession { get; private set; }

        private static readonly string OlympusConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Olympus", "config.json");
        
        private static readonly string DefaultFontDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        private const string DebugTextFontName = "consola.ttf";
        private const int DebugTextFontSize = 13;

        public Map CurrentMap;
        public Config Config;

        public string CelesteDirectory;
        public string CelesteContentDirectory;
        public string CelesteGraphicsDirectory;

        public readonly Font DebugTextFont;
        public readonly FontCollection FontCollection = new();

        public Session()
        {
            if (CurrentSession != null)
                throw new InvalidOperationException("The Session class can only be instantiated once.");
            CurrentSession = this;

            Config = File.Exists(Config.ConfigFile) ? Config.Load() : new();

            if (!TryGetActiveCelesteDirectory(out CelesteDirectory))
            {
                Logger.Log("Could not get the active Celeste directory using the Olympus config file. Stopping program.", LogLevel.Fatal);
                CurrentSession = null;
                return;
            }

            CelesteContentDirectory = Path.Combine(CelesteDirectory, "Content");
            CelesteGraphicsDirectory = Path.Combine(CelesteContentDirectory, "Graphics");

            FontFamily fontFamily = FontCollection.Add(Path.Combine(DefaultFontDirectory, DebugTextFontName));
            DebugTextFont = fontFamily.CreateFont(DebugTextFontSize, FontStyle.Regular);

            Atlas.LoadAtlases(CelesteGraphicsDirectory);
            Autotiler.LoadAutotilers(CelesteGraphicsDirectory);
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
