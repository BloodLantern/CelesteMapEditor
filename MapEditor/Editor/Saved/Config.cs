using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Editor.Utils;
using Editor.Extensions;
using Editor.Logging;
using Editor.Saved.Keybinds;
using YamlDotNet.Serialization;
using System.Data.SqlTypes;

namespace Editor.Saved
{
    [Serializable]
    public class Config
    {
        public const string ConfigFile = "config.xml";

        #region Serialized Fields
        [XmlAnyElement("MaxRecentEditedFilesXmlComment")] public XmlComment MaxRecentEditedFilesXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Max recent edited files saved.")]
        public int MaxRecentEditedFiles = 5;

        [XmlAnyElement("RecentEditedFilesXmlComment")] public XmlComment RecentEditedFilesXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("List of last edited files paths. Should have a maximum of 'MaxRecentEditedFiles' entries.")]
        public List<string> RecentEditedFiles = new();

        [XmlAnyElement("RefreshRateXmlComment")] public XmlComment RefreshRateXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Refresh rate in milliseconds. Only effective if Vsync is off and value is not 0.")]
        public int RefreshRate = 16;

        [XmlAnyElement("VsyncXmlComment")] public XmlComment VSyncXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to enable vertical synchronization. If enabled, this bypasses the refresh rate of MapViewerRefreshRate")]
        public bool Vsync = false;

        [XmlAnyElement("DebugModeXmlComment")] public XmlComment DebugModeXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to enable the debug window and features.")]
        public bool DebugMode = false;

        [XmlAnyElement("EnableLoggingXmlComment")] public XmlComment EnableLoggingXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Enables logging to files. Does not change whether the Logger class is enabled.")]
        public bool EnableLogging = false;

        [XmlAnyElement("LogLevelXmlComment")] public XmlComment LogLevelXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Lowest log level for log files.")]
        public LogLevel LogLevel = LogLevel.Error;

        [XmlAnyElement("ShowAverageFpsXmlComment")] public XmlComment ShowAverageFpsXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to show the average FPS count.")]
        public bool ShowAverageFps = false;

        [XmlAnyElement("ShowDebugConsoleWindowXmlComment")] public XmlComment ShowDebugConsoleWindowXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to show the debug console window.")]
        public bool ShowDebugConsoleWindow = false;

        [XmlAnyElement("ShowLayerSelectionWindowXmlComment")] public XmlComment ShowLayerSelectionWindowXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to show the layer selection window.")]
        public bool ShowLayerSelectionWindow = true;

        [XmlAnyElement("AutoLoadLastEditedMapXmlComment")] public XmlComment AutoLoadLastEditedMapXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to load the last edited map at startup.")]
        public bool AutoLoadLastEditedMap = true;

        [XmlAnyElement("AlwaysPreLoadAllModsXmlComment")] public XmlComment AlwaysPreLoadAllModsXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to preload all mods at startup.")]
        public bool AlwaysPreLoadAllMods = false;

        [XmlAnyElement("RoomSelectionWarpXmlComment")] public XmlComment RoomSelectionWarpXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Whether to warp to a room when selecting it in the list.")]
        public bool RoomSelectionWarp = true;

        [XmlAnyElement("GeneralKeybindsConfigXmlComment")] public XmlComment GeneralKeybindsConfigXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Global keybinds.")]
        public GeneralKeybindsConfig GeneralKeybindsConfig = new();

        [XmlAnyElement("MapViewerConfigXmlComment")] public XmlComment MapViewerConfigXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("MapViewer configuration.")]
        public MapViewerConfig MapViewerConfig = new();

        [XmlAnyElement("UiStyleXmlComment")] public XmlComment UiStyleXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Light of dark theme.")]
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
