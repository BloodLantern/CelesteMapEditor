using Editor.Extensions;
using ImGuiNET;
using NativeFileDialogExtendedSharp;

namespace Editor.UI
{
    public class MenuBar
    {
        public MapEditor MapEditor;

        public MenuBar(MapEditor mapEditor)
        {
            MapEditor = mapEditor;
        }

        public void Render(Session session)
        {
            ImGui.BeginMainMenuBar();

            FileMenu(session.Config);
            EditMenu();
            ViewMenu();
            ModMenu();
            MapMenu();
            RoomMenu();
            HelpMenu();

            ImGui.EndMainMenuBar();
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

        private void FileMenuOpen()
        {
            if (ImGui.MenuItem("Open"))
            {
                NfdDialogResult result = Nfd.FileOpen(new NfdFilter() { Description = "Celeste Map Binary File", Specification = "bin" }.Yield());
                switch (result.Status)
                {
                    case NfdStatus.Ok:
                        MapEditor.LoadMap(result.Path);
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
                    MapEditor.LoadMap(mapToLoad);

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
                MapEditor.Restart();
            }
        }

        private void FileMenuExit()
        {
            if (ImGui.MenuItem("Exit"))
            {
                MapEditor.Exit();
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
                MapEditor.ModDependencies.Open = true;
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
    }
}
