using Assimp.Configs;
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

        public string Name;
        public System.Version Version;
        public string VersionString;

        public string DLL;

        public readonly List<CelesteMod> Dependencies = new();
        public readonly List<CelesteMod> OptionalDependencies = new();

        public List<string> GameplayAtlasEntries = new();

        public CelesteMod()
        {
        }

        public CelesteMod(string name, System.Version version)
        {
            Name = name;
            Version = version;
            VersionString = version.ToString();
        }

        public void PreLoad(string path)
        {
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
        }

        public void PreLoadZip(string path)
        {
            using ZipArchive archive = ZipFile.OpenRead(path);

            ZipArchiveEntry everestYaml = archive.GetEntry("everest.yaml") ?? archive.GetEntry("everest.yml");
            if (everestYaml != null)
            {
                using Stream _everestYamlStream = everestYaml.Open();
                ReadYaml(_everestYamlStream);
            }

            const string gameplayPath = "Graphics/Atlases/Gameplay";
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith(gameplayPath))
                    continue;

                GameplayAtlasEntries.Add(entry.FullName[(gameplayPath.Length + 1)..]);
            }
        }

        public void PreLoadDirectory(string path)
        {
            string everestYaml = Path.Combine(path, "everest.yaml");
            if (File.Exists(everestYaml))
                ReadYaml(File.OpenRead(everestYaml));

            string gameplayAtlasPath = Path.Combine(path, "Graphics", "Atlases", "Gameplay");
            if (!Directory.Exists(gameplayAtlasPath))
                return;

            foreach (string file in Directory.EnumerateFiles(gameplayAtlasPath, "*.png", SearchOption.AllDirectories))
                GameplayAtlasEntries.Add(Path.GetDirectoryName(Path.GetRelativePath(gameplayAtlasPath, file)).Replace('\\', '/') + '/' + Path.GetFileNameWithoutExtension(file));
        }

        public void ReadYaml(Stream file)
        {
            YamlStream everestYamlStream = new();
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
                        DLL = (string) entry.Value;
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
                    CelesteMod dep = new();
                    foreach (KeyValuePair<YamlNode, YamlNode> dependency in (YamlMappingNode) ((YamlSequenceNode) entry.Value).Children[0])
                    {

                        switch ((string) dependency.Key)
                        {
                            case "Name":
                                dep.Name = (string) dependency.Value;
                                break;

                            case "Version":
                                string value;
                                if (dependency.Value.NodeType == YamlNodeType.Sequence)
                                    value = (string) ((YamlSequenceNode) dependency.Value).Children[0];
                                else
                                    value = (string) dependency.Value;

                                if (!System.Version.TryParse(value, out dep.Version))
                                    Logger.Log($"Invalid Celeste dependency mod version: {value}, ignoring", LogLevel.Warning);
                                dep.VersionString = value;
                                break;
                        }
                    }

                    if (!dep.Equals(Empty))
                        dependenciesList.Add(dep);
                }
            }
        }

        public static void PreLoadAll(Session session)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Logger.Log("Preloading Celeste mods...");

            session.CelesteMods.Add(new CelesteMod("Everest", session.EverestVersion));
            foreach (string path in Directory.EnumerateFileSystemEntries(session.CelesteModsDirectory))
            {
                CelesteMod mod = new();
                mod.PreLoad(path);

                if (!mod.Equals(Empty))
                    session.CelesteMods.Add(mod);
            }

            Logger.Log($"{session.CelesteMods.Count} Celeste mods preloaded. Took {stopwatch.ElapsedMilliseconds}ms");
            Logger.Log($"Current memory usage: 0x{GC.GetTotalMemory(true):X8}", LogLevel.Debug);

            stopwatch = Stopwatch.StartNew();
            Logger.Log("Setting up Celeste mod dependencies...");

            // For each Celeste mod, find all of its dependencies and replace them with the loaded references
            foreach (CelesteMod mod in session.CelesteMods)
            {
                for (int depIndex = 0; depIndex < mod.Dependencies.Count; depIndex++)
                {
                    CelesteMod dependency = mod.Dependencies[depIndex];
                    CelesteMod celesteMod = session.CelesteMods.Find(m => m.Name == dependency.Name);
                    if (celesteMod == null)
                    {
                        Logger.Log($"Celeste mod {mod.Name} requires {dependency.Name}, but it isn't installed.", LogLevel.Warning);
                        continue;
                    }

                    if (celesteMod.Version < dependency.Version)
                    {
                        Logger.Log($"Celeste mod {mod.Name} requires {dependency.Name} {dependency.VersionString} or newer, but {celesteMod.Name} {celesteMod.VersionString} is installed.", LogLevel.Warning);
                        continue;
                    }
                    mod.Dependencies[depIndex] = celesteMod;
                }
            }

            Logger.Log($"Celeste mod dependencies set up. Took {stopwatch.ElapsedMilliseconds}ms");
            Logger.Log($"Current memory usage: 0x{GC.GetTotalMemory(true):X8}", LogLevel.Debug);
        }

        public override bool Equals(object obj)
        {
            return obj is CelesteMod mod &&
                   Name == mod.Name &&
                   EqualityComparer<System.Version>.Default.Equals(Version, mod.Version) &&
                   VersionString == mod.VersionString;
        }

        public override int GetHashCode() => HashCode.Combine(Name, Version, VersionString, DLL, Dependencies, OptionalDependencies, GameplayAtlasEntries);
    }
}
