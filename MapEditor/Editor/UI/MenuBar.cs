﻿using Editor.Extensions;
using Editor.Utils;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Editor.UI
{
    public class MenuBar
    {
        private const float MoveInDuration = 0.5f;
        private const float DefaultHeight = 30f;
        private const string Title = "menuBar";

        private readonly Application app;
        public Vector2 Size { get; private set; } = Vector2.One;
        public float CurrentY { get; private set; } = -DefaultHeight;

        public bool Visible = false;

        private readonly ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.MenuBar;

        public MenuBar(Application app)
        {
            this.app = app;
        }

        public void Render(Session session)
        {
            if (!Visible)
                return;

            ImGui.SetNextWindowPos(new(0, CurrentY));
            ImGui.SetNextWindowSize(new(ImGui.GetMainViewport().Size.X, ImGui.GetFrameHeight()));

            ImGui.Begin(Title, flags);
            Size = ImGui.GetWindowSize();

            ImGui.BeginMenuBar();
            FileMenu(session.Config);
            EditMenu();
            ViewMenu();
            ModMenu();
            MapMenu();
            RoomMenu();
            HelpMenu();
            ImGui.EndMenuBar();

            ImGui.End();
        }

        private void FileMenu(Config config)
        {
            if (ImGui.BeginMenu("File"))
            {
                FileMenuNew();
                FileMenuOpen();
                if (config.LastEditedFiles.Count > 0)
                    FileMenuOpenRecent(config);

                ImGui.Separator();

                FileMenuSave();
                FileMenuSaveAll();
                FileMenuSaveAs();

                ImGui.Separator();

                FileMenuRestart();
                FileMenuExit();

                ImGui.EndMenu();
            }
        }

        private void FileMenuNew()
        {
            if (ImGui.MenuItem("New"))
            {
            }
        }

        private static readonly IEnumerable<NfdFilter> OpenFileFilters = new List<NfdFilter>()
        {
            new() { Description = "Celeste Map Binary File", Specification = "bin" },
            new() { Description = "Celeste Mod Compressed Folder", Specification = "zip" }
        };

        private void FileMenuOpen()
        {
            if (ImGui.MenuItem("Open"))
            {
                NfdDialogResult result = Nfd.FileOpen(OpenFileFilters);
                switch (result.Status)
                {
                    case NfdStatus.Ok:
                        switch (Path.GetExtension(result.Path).ToLower())
                        {
                            case ".bin":
                                app.LoadMap(result.Path);
                                break;
                            case ".zip":
                                app.LoadModZip(result.Path);
                                break;
                        }
                        break;

                    case NfdStatus.Error:
                        // TODO notify the user an error occurred
                        //errorMessage = result.Error;
                        break;
                }
            }
        }

        private void FileMenuOpenRecent(Config config)
        {
            if (ImGui.BeginMenu("Open Recent"))
            {
                string mapToLoad = string.Empty;
                foreach (string file in config.LastEditedFiles)
                {
                    if (ImGui.MenuItem(file))
                        mapToLoad = file;
                }

                if (mapToLoad != string.Empty)
                    app.LoadMap(mapToLoad);

                ImGui.EndMenu();
            }
        }

        private void FileMenuSave()
        {
            if (ImGui.MenuItem("Save"))
            {
            }
        }

        private void FileMenuSaveAll()
        {
            if (ImGui.MenuItem("Save All"))
            {
            }
        }

        private void FileMenuSaveAs()
        {
            if (ImGui.MenuItem("Save As"))
            {
            }
        }

        private void FileMenuRestart()
        {
            if (ImGui.MenuItem("Restart"))
            {
                app.Restart();
            }
        }

        private void FileMenuExit()
        {
            if (ImGui.MenuItem("Exit"))
            {
                app.Exit();
            }
        }

        private void EditMenu()
        {
            if (ImGui.BeginMenu("Edit"))
            {
                EditMenuUndo();
                EditMenuRedo();

                ImGui.Separator();

                EditMenuCut();
                EditMenuCopy();
                EditMenuPaste();

                ImGui.Separator();

                EditMenuSelectAll();

                ImGui.Separator();

                EditMenuSettings();

                ImGui.EndMenu();
            }
        }

        private void EditMenuUndo()
        {
            if (ImGui.MenuItem("Undo"))
            {
            }
        }

        private void EditMenuRedo()
        {
            if (ImGui.MenuItem("Redo"))
            {
            }
        }

        private void EditMenuCut()
        {
            if (ImGui.MenuItem("Cut"))
            {
            }
        }

        private void EditMenuCopy()
        {
            if (ImGui.MenuItem("Copy"))
            {
            }
        }

        private void EditMenuPaste()
        {
            if (ImGui.MenuItem("Paste"))
            {
            }
        }

        private void EditMenuSelectAll()
        {
            if (ImGui.MenuItem("Select All"))
            {
            }
        }

        private void EditMenuSettings()
        {
            if (ImGui.MenuItem("Settings"))
            {
            }
        }

        private void ViewMenu()
        {
            if (ImGui.BeginMenu("View"))
            {
                ImGui.EndMenu();
            }
        }

        private void ModMenu()
        {
            if (ImGui.BeginMenu("Mod"))
            {
                ModMenuShowDependencies();

                ImGui.EndMenu();
            }
        }

        private void ModMenuShowDependencies()
        {
            if (ImGui.MenuItem("Show dependencies"))
                app.ModDependencies.Open = true;
        }

        private void MapMenu()
        {
            if (ImGui.BeginMenu("Map"))
            {
                MapMenuStylegrounds();
                MapMenuMetadata();

                ImGui.Separator();

                MapMenuSaveMapImage();

                ImGui.EndMenu();
            }
        }

        private void MapMenuStylegrounds()
        {
            if (ImGui.MenuItem("Stylegrounds"))
            {
            }
        }

        private void MapMenuMetadata()
        {
            if (ImGui.MenuItem("Metadata"))
            {
            }
        }

        private void MapMenuSaveMapImage()
        {
            if (ImGui.MenuItem("Save map image"))
            {
            }
        }

        private void RoomMenu()
        {
            if (ImGui.BeginMenu("Room"))
            {
                RoomMenuAddRoom();
                RoomMenuAddFiller();
                RoomMenuAddFillerRoom();

                ImGui.Separator();

                RoomMenuEdit();

                ImGui.Separator();

                RoomMenuDelete();

                ImGui.EndMenu();
            }
        }

        private void RoomMenuAddRoom()
        {
            if (ImGui.MenuItem("Add room"))
            {
            }
        }

        private void RoomMenuAddFiller()
        {
            if (ImGui.MenuItem("Add filler"))
            {
            }
        }

        private void RoomMenuAddFillerRoom()
        {
            if (ImGui.MenuItem("Add filler room"))
            {
            }
        }

        private void RoomMenuEdit()
        {
            if (ImGui.MenuItem("Edit"))
            {
            }
        }

        private void RoomMenuDelete()
        {
            if (ImGui.MenuItem("Delete"))
            {
            }
        }

        private void HelpMenu()
        {
            if (ImGui.BeginMenu("Help"))
            {
                HelpMenuCheckForUpdates();
                HelpMenuAbout();

                ImGui.EndMenu();
            }
        }

        private void HelpMenuCheckForUpdates()
        {
            if (ImGui.MenuItem("Check for updates"))
            {
            }
        }

        private void HelpMenuAbout()
        {
            if (ImGui.MenuItem("About"))
            {
            }
        }

        public void StartMoveInRoutine() => Coroutine.Start(MoveInRoutine(-DefaultHeight, 0f, MoveInDuration));

        private IEnumerator MoveInRoutine(float startingY, float endingY, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                CurrentY = Calc.EaseLerp(startingY, endingY, timer, duration, Ease.CubeOut);

                yield return null;
            }

            CurrentY = endingY;
        }
    }
}
