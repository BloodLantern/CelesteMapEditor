﻿using Editor.Logging;
using Editor.Saved;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoGame.ImGuiNet;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Editor
{
    public class Session
    {
        public static Session Current { get; private set; }

        private static readonly string OlympusConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Olympus", "config.json");
        
        public Application App { get; }
        public Config Config { get; }

        public string CelesteDirectory { get; private set; }
        public string CelesteContentDirectory { get; private set; }
        public string CelesteGraphicsDirectory { get; private set; }
        public string CelesteModsDirectory { get; private set; }

        public Version CelesteVersion { get; private set; }
        public Version EverestVersion { get; private set; }

        public string ContentDirectory { get; private set; }

        /// <summary>
        /// Mainly used as a debugging font.
        /// </summary>
        public SpriteFont ConsolasFont { get; private set; }
        public SpriteFont UbuntuRegularFont { get; private set; }
        public SpriteFont UbuntuLightFont { get; private set; }
        /// <summary>
        /// Mainly used to display trigger names.
        /// </summary>
        public SpriteFont PixelatedFont { get; private set; }

        public readonly List<CelesteMod> CelesteMods = [];

        public Session(Application app)
        {
            if (Current != null)
                throw new InvalidOperationException("The Session class can only be instantiated once.");
            Current = this;

            App = app;

            ContentDirectory = App.Content.RootDirectory;

            Config = Config.Load();

            SetupImGuiContext(App.ImGuiRenderer);
        }

        private void SetupImGuiContext(ImGuiRenderer renderer)
        {
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGuiStyles.Setup(Config.Ui.Style);
            ImGuiStyles.SetupFont(this, renderer);
        }

        public void LoadContent(ContentManager content)
        {
            ConsolasFont = content.Load<SpriteFont>("Fonts/consolas");
            UbuntuRegularFont = content.Load<SpriteFont>("Fonts/ubuntu-regular");
            UbuntuLightFont = content.Load<SpriteFont>("Fonts/ubuntu-light");
            PixelatedFont = content.Load<SpriteFont>("Fonts/pixelated");
        }

        public bool TryLoad()
        {
            if (!TryGetActiveCelesteDirectory(out string celesteDir))
            {
                Logger.Log("Could not get the active Celeste directory using the Olympus config file. Stopping program.", LogLevel.Fatal);
                Logger.Log($"Was looking for {OlympusConfigFilePath}", LogLevel.Fatal);
                return false;
            }
            CelesteDirectory = celesteDir;

            if (!TryGetActiveCelesteVersion(out Version celesteVersion, out Version everestVersion))
            {
                Logger.Log("Could not get the active Celeste version. Stopping program.", LogLevel.Fatal);
                return false;
            }
            CelesteVersion = celesteVersion;
            EverestVersion = everestVersion;

            if (EverestVersion == null)
                Logger.Log("Everest version not found. Celeste not modded.", LogLevel.Warning);

            Logger.Log($"Detected active Celeste version: {CelesteVersion}");
            Logger.Log($"Detected active Everest version: {EverestVersion}");

            CelesteContentDirectory = Path.Combine(CelesteDirectory, "Content");
            CelesteGraphicsDirectory = Path.Combine(CelesteContentDirectory, "Graphics");
            CelesteModsDirectory = Path.Combine(CelesteDirectory, "Mods");

            return true;
        }

        /// <summary>
        /// Reads the Olympus config file to find the current Celeste directory.
        /// </summary>
        /// <returns>The path to the active Celeste directory.</returns>
        private bool TryGetActiveCelesteDirectory(out string activeCelesteDirectory)
        {
            activeCelesteDirectory = string.Empty;

            JsonDocument doc = JsonDocument.Parse(File.OpenRead(OlympusConfigFilePath), new() { CommentHandling = JsonCommentHandling.Skip });

            List<string> celesteInstalls = [];

            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("install", out JsonElement install))
            {
                Logger.Log($"Error while reading Olympus config file: {nameof(JsonElement)} 'install' does not exist.", LogLevel.Error);
                return false;
            }

            if (!install.TryGetInt32(out int celesteInstallId))
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

            activeCelesteDirectory = celesteInstalls[Math.Clamp(celesteInstallId - 1, 0, celesteInstalls.Count)];
            return true;
        }

        /// <summary>
        /// Function from Olympus: https://github.com/EverestAPI/Olympus/blob/9d5c3cbfc322aaf52e2557f715073b94966eda2a/sharp/CmdGetVersionString.cs#L35.
        /// </summary>
        /// <param name="activeCelesteVersion"></param>
        /// <param name="activeEverestVersion"></param>
        /// <returns></returns>
        public bool TryGetActiveCelesteVersion(out Version activeCelesteVersion, out Version activeEverestVersion)
        {
            activeCelesteVersion = activeEverestVersion = null;

            try
            {
                string gamePath = Path.Combine(CelesteDirectory, "Celeste.exe");

                // Use Celeste.dll if Celeste.exe is not a managed assembly
                try
                {
                    _ = AssemblyName.GetAssemblyName(gamePath);
                }
                catch (FileNotFoundException)
                {
                    gamePath = Path.Combine(CelesteDirectory, "Celeste.dll");
                }
                catch (BadImageFormatException)
                {
                    gamePath = Path.Combine(CelesteDirectory, "Celeste.dll");
                }

                using ModuleDefinition game = ModuleDefinition.ReadModule(gamePath);
                TypeDefinition tCeleste = game.GetType("Celeste.Celeste");
                if (tCeleste == null)
                    return false;

                // Find Celeste .ctor (luckily only has one)

                string versionString = null;
                int[] versionInts = null;

                MethodDefinition cCeleste =
                    tCeleste.FindMethod("System.Void orig_ctor_Celeste()") ??
                    tCeleste.FindMethod("System.Void .ctor()");

                if (cCeleste is { HasBody: true })
                {
                    Mono.Collections.Generic.Collection<Instruction> instrs = cCeleste.Body.Instructions;
                    for (int instri = 0; instri < instrs.Count; instri++)
                    {
                        Instruction instr = instrs[instri];
                        MethodReference cVersion = instr.Operand as MethodReference;
                        if (instr.OpCode != OpCodes.Newobj || cVersion?.DeclaringType?.FullName != "System.Version")
                            continue;

                        // We're constructing a System.Version - check if all parameters are of type int.
                        bool cVersionIntsOnly = cVersion.Parameters.Cast<ParameterReference>().All(param => param.ParameterType.MetadataType == MetadataType.Int32);

                        if (cVersionIntsOnly)
                        {
                            // Assume that ldc.i4* instructions are right before the newobj.
                            versionInts = new int[cVersion.Parameters.Count];
                            for (int i = -versionInts.Length; i < 0; i++)
                                versionInts[i + versionInts.Length] = instrs[i + instri].GetInt();
                        }

                        if (cVersion.Parameters.Count == 1 && cVersion.Parameters[0].ParameterType.MetadataType == MetadataType.String)
                        {
                            // Assume that a ldstr is right before the newobj.
                            versionString = instrs[instri - 1].Operand as string;
                        }

                        // Don't check any other instructions.
                        break;
                    }
                }

                // Construct the version from our gathered data.
                Version version = new();
                if (versionString != null)
                    version = new(versionString);
                
                if (versionInts == null || versionInts.Length == 0)
                {
                    version = new();
                }
                else
                {
                    version = versionInts.Length switch
                    {
                        2 => new(versionInts[0], versionInts[1]),
                        3 => new(versionInts[0], versionInts[1], versionInts[2]),
                        4 => new(versionInts[0], versionInts[1], versionInts[2], versionInts[3]),
                        _ => version
                    };
                }

                TypeDefinition tEverest = game.GetType("Celeste.Mod.Everest");
                if (tEverest != null)
                {
                    // The first operation in .cctor is ldstr with the version string.
                    string versionModStr = (string) tEverest.FindMethod("System.Void .cctor()").Body.Instructions[0].Operand;
                    int versionSplitIndex = versionModStr.IndexOf('-');
                    if (versionSplitIndex != -1 && Version.TryParse(versionModStr.AsSpan(0, versionSplitIndex), out Version versionMod))
                    {
                        activeCelesteVersion = version;
                        activeEverestVersion = versionMod;
                        return true;
                    }
                }

                activeCelesteVersion = version;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Exit()
        {
            Config.Save();
        }
    }
}
