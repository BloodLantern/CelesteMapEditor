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
        public string LastEditedFile => LastEditedFiles[0];

        public int MapViewerRefreshRate = 60;

        public bool ShowDebugInfo = false;

        public bool AutoLoadLastEditedMap = true;

        public Config()
        {
            if (!File.Exists(ConfigFile))
                FirstTimeSetup();
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
            FileStream output = new(ConfigFile, FileMode.OpenOrCreate, FileAccess.Write);
            serializer.Serialize(output, this);
        }

        public static Config Load()
        {
            XmlSerializer serializer = new(typeof(Config));
            FileStream output = new(ConfigFile, FileMode.Open, FileAccess.Read);
            return (Config) serializer.Deserialize(output);
        }
    }
}
