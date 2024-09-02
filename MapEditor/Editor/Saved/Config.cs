using System;
using System.Collections.Generic;
using System.IO;
using Editor.Utils;
using Editor.Logging;
using Editor.Saved.Keybinds;
using Editor.Saved.Attributes;
using System.Reflection;
using Editor.UI.Components;

namespace Editor.Saved
{
    [Serializable]
    public class Config : ConfigBase
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
        [NotDisplayedOnEditor]
        public List<string> RecentEditedFiles = [];

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
        [NotDisplayedOnEditor]
        public bool ShowDebugConsoleWindow = false;

        /// <summary>
        /// Whether to show the layer selection window.
        /// </summary>
        [NotDisplayedOnEditor]
        public bool ShowLayerSelectionWindow = false;

        public LayerSelection.Layers[] LayerSelectionLayers = [LayerSelection.Layers.All, LayerSelection.Layers.Entities];

        public LayerSelection.DebugLayers LayerSelectionDebugLayers = LayerSelection.DebugLayers.LevelBounds | LayerSelection.DebugLayers.FillerBounds;

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
        /// UI configuration.
        /// </summary>
        public UiConfig Ui = new();

        /// <summary>
        /// Graphics configuration.
        /// </summary>
        public GraphicsConfig Graphics = new();

        /// <summary>
        /// Keybinds.
        /// </summary>
        public KeybindsConfig Keybinds = new();

        /// <summary>
        /// MapViewer configuration.
        /// </summary>
        public MapViewerConfig MapViewer = new();
        #endregion

        #region Properties
        [ComparisonIgnore]
        public string LastEditedFile => RecentEditedFiles.Count > 0 ? RecentEditedFiles[0] : null;
        #endregion

        #region Constructors
        public Config()
        {
        }
        #endregion

        #region Methods
        internal override void FirstTimeSetup()
        {
            foreach (FieldInfo field in typeof(Config).GetFields())
            {
                if (field.GetValue(this) is ConfigBase configBase)
                    configBase.FirstTimeSetup();
            }
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

        public void Save() => File.WriteAllText(ConfigFile, this.GetXml(true));

        public static Config Load()
        {
            if (!File.Exists(ConfigFile))
            {
                Config config = new();
                config.FirstTimeSetup();
                return config;
            }

            return File.ReadAllText(ConfigFile).LoadFromXml<Config>();
        }

        public override object Clone() => Clone<Config>();
        #endregion
    }
}
