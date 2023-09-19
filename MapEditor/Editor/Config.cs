using Editor.Logging;
using Editor.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Editor
{
    [Serializable]
    public class Config
    {
        public const string ConfigFile = "config.xml";
        
        public int MaxLastEditedFiles = 5;
        public List<string> LastEditedFiles = new();
        public string LastEditedFile => LastEditedFiles.Count > 0 ? LastEditedFiles[0] : null;

        /// <summary>
        /// Only effective if Vsync is off.
        /// </summary>
        public TimeSpan MapViewerRefreshRate = new(0, 0, 0, 0, 16);
        public bool UseMapViewerRefreshRate = true;
        public bool Vsync = false;

        public bool DebugMode = false;
        /// <summary>
        /// Enables logging to files.
        /// </summary>
        public bool EnableLogging = false;
        /// <summary>
        /// Lowest log level for log files.
        /// </summary>
        public LogLevel LogLevel = LogLevel.Error;
        public bool ShowDebugConsole = false;
        public bool ShowAverageFps = false;

        public bool AutoLoadLastEditedMap = true;
        public bool RoomSelectionWarp = true;
        public bool ShowHitboxes = false;

        public float ZoomFactor = 1.25f;

        public MouseButton CameraMoveButton = MouseButton.Middle;
        public MouseButton SelectButton = MouseButton.Left;

        public Color EntitySelectionBoundsColorMin = Color.DarkMagenta;
        public Color EntitySelectionBoundsColorMax = Color.Magenta;

        public ImGuiStyles.Style UiStyle = ImGuiStyles.DefaultStyle;

        public Config()
        {
        }

        private void FirstTimeSetup()
        {
        }

        /// <summary>
        /// Adds a file to the recent ones.
        /// </summary>
        /// <param name="filePath">The file to add to the recent list.</param>
        /// <returns>
        /// Whether the path was successfully added in the list. Returns false
        /// if it was already in the list and was therefore only moved to the top.
        /// </returns>
        public bool AddEditedFile(string filePath)
        {
            if (LastEditedFiles.Contains(filePath))
            {
                // Move 'filePath' to the top
                LastEditedFiles.Remove(filePath);
                LastEditedFiles.Insert(0, filePath);
                return false;
            }
            else
            {
                LastEditedFiles.Insert(0, filePath);

                if (LastEditedFiles.Count > MaxLastEditedFiles)
                    LastEditedFiles.RemoveAt(LastEditedFiles.Count - 1);
                return true;
            }
        }

        public void Save()
        {
            XmlSerializer serializer = new(typeof(Config));
            FileStream output = new(ConfigFile, FileMode.Create, FileAccess.Write);
            serializer.Serialize(output, this);
        }

        public static Config Load()
        {
            if (!File.Exists(ConfigFile))
            {
                Config config = new();
                config.FirstTimeSetup();
                return config;
            }

            XmlSerializer serializer = new(typeof(Config));
            FileStream output = new(ConfigFile, FileMode.Open, FileAccess.Read);
            return (Config) serializer.Deserialize(output);
        }
    }
}
