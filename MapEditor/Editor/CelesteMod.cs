using Editor.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Editor
{
    public class CelesteMod
    {
        public static readonly CelesteMod Empty = new();

        public string NameAndVersion => $"{Name} v{(Version != null ? Version : VersionString)}";

        public string Name;
        public System.Version Version;
        public string VersionString;

        public string Dll;

        public readonly List<CelesteMod> Dependencies = [];
        public readonly List<CelesteMod> OptionalDependencies = [];

        /// <summary>
        /// The location of the .zip file or the directory.
        /// </summary>
        public string Location;
        public bool IsDirectory;

        public readonly int Index = -1;

        private bool enabled = false;
        public bool Enabled
        {
            get
            {
                if (ForceEnabled)
                    return true;
                return enabled;
            }
            set => enabled = value;
        }
        public bool Preloaded = false;
        public bool Loaded = false;

        public readonly bool ForceEnabled = false;

        public readonly Dictionary<string, Texture> GameplayAtlasEntries = new();

        public CelesteMod()
        {
        }

        public CelesteMod(int index)
        {
            Index = index;
        }

        public CelesteMod(string name, System.Version version, int index)
        {
            Name = name;
            Version = version;
            VersionString = version.ToString();
            Index = index;
            ForceEnabled = true;
            Loaded = true;
        }

        public void PreLoad(string path)
        {
            Location = path;

            if (File.Exists(path))
            {
                if (Path.GetExtension(path) != ".zip")
                    return;

                Logger.Log($"- Preloading {Path.GetFileName(path)}...");

                PreLoadZip(path);
            }
            else // Directory
            {
                if (Path.GetFileName(path) == "Cache")
                    return;

                Logger.Log($"- Preloading {Path.GetFileName(path)}...");

                PreLoadDirectory(path);
            }

            // If the mod doesn't have a name yet, it doesn't have an everest.yaml file
            // Use the file/folder name as the mod name instead
            if (Name == null)
            {
                Name = Path.GetFileNameWithoutExtension(path);
                Version = new();
                VersionString = "[No everest.yaml provided]";
            }

            Preloaded = true;
        }

        public void PreLoadZip(string path)
        {
            IsDirectory = false;

            using ZipArchive archive = ZipFile.OpenRead(path);

            ZipArchiveEntry everestYaml = archive.GetEntry("everest.yaml") ?? archive.GetEntry("everest.yml");
            if (everestYaml != null)
            {
                using Stream everestYamlStream = everestYaml.Open();
                ReadYaml(everestYamlStream);
            }

            const string GameplayPath = "Graphics/Atlases/Gameplay";
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith(GameplayPath) || Path.GetExtension(entry.Name) != ".png")
                    continue;

                GameplayAtlasEntries.Add(entry.FullName[(GameplayPath.Length + 1)..], null);
            }
        }

        public void PreLoadDirectory(string path)
        {
            IsDirectory = true;

            string everestYaml = Path.Combine(path, "everest.yaml");
            if (File.Exists(everestYaml))
                ReadYaml(File.OpenRead(everestYaml));

            string gameplayAtlasPath = Path.Combine(path, "Graphics", "Atlases", "Gameplay");
            if (!Directory.Exists(gameplayAtlasPath))
                return;

            foreach (string file in Directory.EnumerateFiles(gameplayAtlasPath, "*.png", SearchOption.AllDirectories))
                GameplayAtlasEntries.Add(Path.GetDirectoryName(Path.GetRelativePath(gameplayAtlasPath, file)).Replace('\\', '/') + '/' + Path.GetFileNameWithoutExtension(file), null);
        }

        public void ReadYaml(Stream file)
        {
            YamlStream everestYamlStream = [];
            everestYamlStream.Load(new Parser(new StreamReader(file)));

            YamlMappingNode node = (YamlMappingNode) ((YamlSequenceNode) everestYamlStream.Documents[0].RootNode).Children[0];
            List<CelesteMod> dependenciesList = null;
            foreach (KeyValuePair<YamlNode, YamlNode> entry in node)
            {
                switch ((string) entry.Key)
                {
                    case "Name":
                        Name = (string) entry.Value;
                        break;

                    case "Version":
                        string value = (string) entry.Value;
                        if (!System.Version.TryParse(value, out Version))
                            Logger.Log($"Invalid Celeste mod version: {value}, ignoring", LogLevel.Warning);
                        VersionString = value;
                        break;

                    case "DLL":
                        Dll = (string) entry.Value;
                        break;

                    case "Dependencies":
                        dependenciesList = Dependencies;
                        break;

                    case "OptionalDependencies":
                        dependenciesList = OptionalDependencies;
                        break;
                }

                if (dependenciesList != null)
                {
                    foreach (YamlNode dependency in (YamlSequenceNode) entry.Value)
                    {
                        CelesteMod dep = new();
                        foreach (KeyValuePair<YamlNode, YamlNode> dependencyVal in (YamlMappingNode) dependency)
                        {
                            switch ((string) dependencyVal.Key)
                            {
                                case "Name":
                                    dep.Name = (string) dependencyVal.Value;
                                    break;

                                case "Version":
                                    string value;
                                    if (dependencyVal.Value.NodeType == YamlNodeType.Sequence)
                                        value = (string) ((YamlSequenceNode) dependencyVal.Value).Children[0];
                                    else
                                        value = (string) dependencyVal.Value;

                                    if (!System.Version.TryParse(value, out dep.Version))
                                        Logger.Log($"Invalid Celeste dependency mod version: {value}, ignoring", LogLevel.Warning);
                                    dep.VersionString = value;
                                    break;
                            }
                        }
                        if (dep != Empty)
                        {
                            // If a dependency with the same name already exists, replace it if the new one has a higher version
                            CelesteMod sameDep = dependenciesList.Find(m => m.Name == dep.Name);
                            if (sameDep != null)
                            {
                                if (dep.Version > sameDep.Version)
                                    dependenciesList.Remove(sameDep);
                            }
                            dependenciesList.Add(dep);
                        }
                    }
                    dependenciesList.Sort((a, b) => a.Name.CompareTo(b.Name));
                }
            }
        }

        /// <summary>
        /// Requires the mod to be preloaded.
        /// </summary>
        public void Load()
        {
            if (IsDirectory)
            {
                foreach (string asset in GameplayAtlasEntries.Keys)
                    GameplayAtlasEntries[asset] = new(File.OpenRead(Path.Combine(Location, asset)), ".png", asset);
            }
            else
            {
                using ZipArchive archive = ZipFile.OpenRead(Location);

                foreach (string asset in GameplayAtlasEntries.Keys)
                    GameplayAtlasEntries[asset] = new(archive.GetEntry("Graphics/Atlases/Gameplay/" + asset).Open(), ".png", asset);
            }

            Loaded = true;
        }

        public void Unload()
        {
            foreach (string asset in GameplayAtlasEntries.Keys)
                GameplayAtlasEntries[asset] = null;

            Loaded = false;
        }

        public static void PreLoadAll(Session session, Loading loading, float progressFactor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            float oldProgress = loading.Progress;
            float progressFactorOnTwo = progressFactor / 2f;

            Logger.Log("Preloading Celeste mods...");

            // Getting a list instance here allows us to get the total number of entries
            List<string> entries = [..Directory.EnumerateFileSystemEntries(session.CelesteModsDirectory)];
            float entriesCountInverse = 1f / entries.Count;

            int index = 0;
            session.CelesteMods.Add(new("Celeste", session.CelesteVersion, index++));
            session.CelesteMods.Add(new("Everest", session.EverestVersion, index++));
            session.CelesteMods.Add(new("EverestCore", session.EverestVersion, index++));
            foreach (string path in entries)
            {
                loading.CurrentSubText = Path.GetRelativePath(session.CelesteModsDirectory, path);

                CelesteMod mod = new(index++);
                mod.PreLoad(path);

                if (mod != Empty)
                {
                    session.CelesteMods.Add(mod);
                    loading.Progress += entriesCountInverse * progressFactorOnTwo;
                }
                else
                {
                    index--;
                }
            }

            loading.CurrentSubText = string.Empty;

            Logger.Log($"{session.CelesteMods.Count} Celeste mods preloaded. Took {stopwatch.ElapsedMilliseconds}ms");

            float celesteModsCountInverse = 1f / session.CelesteMods.Count;

            stopwatch = Stopwatch.StartNew();
            Logger.Log("Setting up Celeste mod dependencies...");

            // For each Celeste mod, find all of its dependencies and replace them with the loaded references
            foreach (CelesteMod mod in session.CelesteMods)
            {
                for (int depIndex = 0; depIndex < mod.Dependencies.Count; depIndex++)
                {
                    CelesteMod dependency = mod.Dependencies[depIndex];
                    CelesteMod loadedReference = session.CelesteMods.Find(m => m.Name == dependency.Name);

                    if (loadedReference == null)
                    {
                        Logger.Log($"Celeste mod {mod.Name} requires {dependency.Name}, but it isn't installed.", LogLevel.Warning);
                        continue;
                    }

                    if (loadedReference.Version < dependency.Version)
                    {
                        Logger.Log($"Celeste mod {mod.Name} requires {dependency.Name} {dependency.VersionString} or newer, but {loadedReference.Name} {loadedReference.VersionString} is installed.", LogLevel.Warning);
                        continue;
                    }

                    mod.Dependencies[depIndex] = loadedReference;
                }
                loading.Progress += celesteModsCountInverse * progressFactorOnTwo;
            }

            Logger.Log($"Celeste mod dependencies set up. Took {stopwatch.ElapsedMilliseconds}ms");

            loading.Progress = oldProgress + progressFactor;
        }

        /// <summary>
        /// Requires all mods to be preloaded. Avoid using this method because it is extremely slow.
        /// Only load the necessary mods instead.
        /// </summary>
        /// <param name="session">The Session instance in which the mods are listed.</param>
        public static void LoadAll(Session session)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Logger.Log("Loading Celeste mods...");

            foreach (CelesteMod mod in session.CelesteMods)
            {
                // If mod is Celeste or Everest there's nothing to load here
                if (mod.ForceEnabled)
                    continue;

                mod.Load();
            }

            Logger.Log($"Celeste mods loaded. Took {stopwatch.ElapsedMilliseconds}ms");
        }

        public static void UnloadAll(Session session)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Logger.Log("Unloading Celeste mods...");

            foreach (CelesteMod mod in session.CelesteMods)
            {
                // If mod is Celeste or Everest there's nothing to unload here
                if (mod.ForceEnabled)
                    continue;

                mod.Unload();
            }

            Logger.Log($"Celeste mods unloaded. Took {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Updates all mods status. This checks whether they are enabled
        /// and loaded and loads or unloads them if necessary.
        /// </summary>
        public static void UpdateAll(Session session)
        {
            foreach (CelesteMod mod in session.CelesteMods)
            {
                if (mod.Enabled && !mod.Loaded)
                    mod.Load();
                else if (!mod.Enabled && mod.Loaded)
                    mod.Unload();
            }
            GC.Collect();
        }

        public override string ToString() => Name;

        public override int GetHashCode() => HashCode.Combine(Name, Version, VersionString, Dll, Dependencies, OptionalDependencies, GameplayAtlasEntries);

        public override bool Equals(object obj)
        {
            return obj is CelesteMod mod &&
                   Name == mod.Name &&
                   EqualityComparer<System.Version>.Default.Equals(Version, mod.Version) &&
                   VersionString == mod.VersionString;
        }

        public static bool operator ==(CelesteMod left, CelesteMod right)
        {
            return EqualityComparer<CelesteMod>.Default.Equals(left, right);
        }

        public static bool operator !=(CelesteMod left, CelesteMod right)
        {
            return !(left == right);
        }
    }
}
