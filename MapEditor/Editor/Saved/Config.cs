using System;
using System.Collections.Generic;
using System.IO;
using Editor.Utils;
using Editor.Logging;
using Editor.Saved.Keybinds;

namespace Editor.Saved
{
    [Serializable]
    public class Config
    {
        public const string ConfigFile = "config.xml";

        #region Serialized Fields
        /// <summary>
        /// Max recent edited files saved.
        /// </summary>
        public int MaxRecentEditedFiles = 5;

        /// <summary>
        /// List of last edited files paths. Should have a maximum of 'MaxRecentEditedFiles' entries.
        /// </summary>
        public List<string> RecentEditedFiles = new();

        /// <summary>
        /// Refresh rate in milliseconds. Only effective if Vsync is off and value is not 0.
        /// </summary>
        public int RefreshRate = 16;
       
        /// <summary>
        /// Whether to enable vertical synchronization. If enabled, this bypasses the refresh rate of MapViewerRefreshRate
        /// </summary>
        public bool Vsync = false;

        /// <summary>
        /// Whether to enable the debug window and features.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// Enables logging to files. Does not change whether the Logger class is enabled.
        /// </summary>
        public bool EnableLogging = false;

        /// <summary>
        /// Lowest log level for log files.
        /// </summary>
        public LogLevel LogLevel = LogLevel.Error;

        /// <summary>
        /// Whether to show the average FPS count.
        /// </summary>
        public bool ShowAverageFps = false;

        /// <summary>
        /// Whether to show the debug console window.
        /// </summary>
        public bool ShowDebugConsoleWindow = false;

        /// <summary>
        /// Whether to show the layer selection window.
        /// </summary>
        public bool ShowLayerSelectionWindow = true;

        /// <summary>
        /// Whether to load the last edited map at startup.
        /// </summary>
        public bool AutoLoadLastEditedMap = true;

        /// <summary>
        /// Whether to preload all mods at startup.
        /// </summary>
        public bool AlwaysPreLoadAllMods = false;

        /// <summary>
        /// Whether to warp to a room when selecting it in the list.
        /// </summary>
        public bool RoomSelectionWarp = true;

        /// <summary>
        /// Global keybinds.
        /// </summary>
        public GeneralKeybindsConfig GeneralKeybindsConfig = new();

        /// <summary>
        /// MapViewer configuration.
        /// </summary>
        public MapViewerConfig MapViewerConfig = new();

        /// <summary>
        /// Light or dark theme.
        /// </summary>
        public ImGuiStyles.Style UiStyle = ImGuiStyles.DefaultStyle;
        #endregion

        #region Properties
        public string RecentEditedFile => RecentEditedFiles.Count > 0 ? RecentEditedFiles[0] : null;
        #endregion

        #region Constructors
        public Config()
        {
        }
        #endregion

        #region Methods
        private void FirstTimeSetup()
        {
#if WINDOWS
            if (!Calc.ShouldSystemUseDarkMode())
                UiStyle = ImGuiStyles.Style.Light;
#endif
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
            if (RecentEditedFiles.Contains(filePath))
            {
                // Move 'filePath' to the top
                RecentEditedFiles.Remove(filePath);
                RecentEditedFiles.Insert(0, filePath);
                return false;
            }
            else
            {
                RecentEditedFiles.Insert(0, filePath);

                if (RecentEditedFiles.Count > MaxRecentEditedFiles)
                    RecentEditedFiles.RemoveAt(RecentEditedFiles.Count - 1);
                return true;
            }
        }

        public void Save()
        {
            //XmlSerializer serializer = new(typeof(Config));
            //FileStream output = new(ConfigFile, FileMode.Create, FileAccess.Write);
            //serializer.Serialize(output, this);

            File.WriteAllText(ConfigFile, this.GetXml(true));
        }

        public static Config Load()
        {
            if (!File.Exists(ConfigFile))
            {
                Config config = new();
                config.FirstTimeSetup();
                return config;
            }

            //XmlSerializer serializer = new(typeof(Config));
            //FileStream output = new(ConfigFile, FileMode.Open, FileAccess.Read);
            //return (Config)serializer.Deserialize(output);
            return File.ReadAllText(ConfigFile).LoadFromXml<Config>();
        }
        #endregion
    }
}
