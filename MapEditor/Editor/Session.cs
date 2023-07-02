using Editor.Celeste;
using Editor.Graphics;
using Editor.Logging;
using Newtonsoft.Json;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Editor
{
    public class Session
    {
        public static Session CurrentSession { get; private set; }

        public static readonly string OlympusConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Olympus", "config.json");
        
        private static readonly string DefaultFontDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        private const string DebugTextFontName = "consola.ttf";

        public MapData CurrentMap;
        public Config Config;

        public string CelesteDirectory;
        public string CelesteContentDirectory;

        public readonly Font DebugTextFont;
        public readonly FontCollection FontCollection = new();

        public Session()
        {
            if (CurrentSession != null)
                throw new InvalidOperationException("The Session class can only be instantiated once.");
            CurrentSession = this;

            Config = File.Exists(Config.ConfigFile) ? Config.Load() : new();

            CelesteDirectory = GetActiveCelesteDirectory();
            CelesteContentDirectory = Path.Combine(CelesteDirectory, "Content");

            FontFamily fontFamily = FontCollection.Add(Path.Combine(DefaultFontDirectory, DebugTextFontName));
            DebugTextFont = fontFamily.CreateFont(13, FontStyle.Regular);

            Atlas.LoadAtlases(CelesteContentDirectory);
        }

        /// <summary>
        /// Reads the Olympus config file to find the current Celeste directory.
        /// </summary>
        /// <returns>The path to the active Celeste directory.</returns>
        private string GetActiveCelesteDirectory()
        {
            JsonTextReader reader = new(new StringReader(File.ReadAllText(OlympusConfigFile)));

            int installID = -1;
            List<string> installs = new();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.Value != null)
                {
                    // Look for the 'install' and 'installs' properties
                    switch (reader.Value.ToString())
                    {
                        case "install":
                            installID = reader.ReadAsInt32().GetValueOrDefault();
                            break;

                        case "installs":
                            // Read the array start
                            reader.Read();

                            // Check that the value is the expected array
                            if (reader.Value != null || reader.TokenType != JsonToken.StartArray)
                            {
                                Logger.Log($"Invalid Olympus Celeste installs array type: {reader.TokenType}. Expected an array start", LogLevel.Warning);
                                return string.Empty;
                            }

                            // Read all the array values
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                // Check that the value is the expected JSON object start
                                if (reader.Value != null || reader.TokenType != JsonToken.StartObject)
                                {
                                    Logger.Log($"Invalid Olympus Celeste install type: {reader.TokenType}. Expected a JSON object start", LogLevel.Warning);
                                    return string.Empty;
                                }

                                // Read all the install values
                                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                                {
                                    if (reader.TokenType == JsonToken.PropertyName && reader.Value != null && reader.Value as string == "path")
                                    {
                                        installs.Add(reader.ReadAsString());
                                        break;
                                    }
                                }

                                // Read the array end
                                reader.Read();

                                // Check that the value is the expected JSON object end
                                if (reader.Value != null || reader.TokenType != JsonToken.EndObject)
                                {
                                    Logger.Log($"Invalid Olympus Celeste install type: {reader.TokenType}. Expected a JSON object end", LogLevel.Warning);
                                    return string.Empty;
                                }
                            }
                            break;
                    }
                }
            }

            return installs[installID - 1];
        }

        public void Exit()
        {
            Config.Save();
        }
    }
}
