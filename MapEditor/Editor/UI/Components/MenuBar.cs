using Editor.Extensions;
using Editor.Saved;
using Editor.Utils;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Editor.UI.Components
{
    public class MenuBar : UiComponent
    {
        private const string Title = "Menu Bar";

        private Application App { get; }
        private MapViewer MapViewer { get; }
        private ModDependencies ModDependencies { get; set; }
        private LayerSelection LayerSelection { get; set; }

        public MenuBar(Application app)
            : base(RenderingCall.First)
        {
            App = app;
            MapViewer = app.MapViewer;
        }

        internal override void Initialize(UiManager manager)
        {
            ModDependencies = manager.GetComponent<ModDependencies>();
            LayerSelection = manager.GetComponent<LayerSelection>();

            base.Initialize(manager);
        }

        public override void Render()
        {
            if (!ImGui.BeginMainMenuBar())
                return;
            
            FileMenu(App.Session.Config);
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
            if (!ImGui.BeginMenu("File"))
                return;
            
            FileMenuNew();
            FileMenuOpen();
            if (config.RecentEditedFiles.Count > 0)
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

        private void FileMenuNew()
        {
            if (!ImGui.MenuItem("New"))
                return;
        }

        private static readonly IEnumerable<NfdFilter> OpenFileFilters = new List<NfdFilter>()
        {
            new() { Description = "Celeste Map Binary File", Specification = "bin" },
            new() { Description = "Celeste Mod Compressed Folder", Specification = "zip" }
        };

        private void FileMenuOpen()
        {
            if (!ImGui.MenuItem("Open"))
                return;
            
            NfdDialogResult result = Nfd.FileOpen(OpenFileFilters);
            switch (result.Status)
            {
                case NfdStatus.Ok:
                    switch (Path.GetExtension(result.Path).ToLower())
                    {
                        case ".bin":
                            App.LoadMap(result.Path);
                            break;
                        case ".zip":
                            App.LoadModZip(result.Path);
                            break;
                    }
                    break;

                case NfdStatus.Error:
                    // TODO notify the user an error occurred
                    //errorMessage = result.Error;
                    break;
            }
        }

        private void FileMenuOpenRecent(Config config)
        {
            if (!ImGui.BeginMenu("Open Recent"))
                return;
            
            string mapToLoad = string.Empty;
            foreach (string file in config.RecentEditedFiles)
            {
                if (ImGui.MenuItem(file))
                    mapToLoad = file;
            }

            if (mapToLoad != string.Empty)
                App.LoadMap(mapToLoad);

            ImGui.EndMenu();
        }

        private void FileMenuSave()
        {
            if (!ImGui.MenuItem("Save"))
                return;
        }

        private void FileMenuSaveAll()
        {
            if (!ImGui.MenuItem("Save All"))
                return;
        }

        private void FileMenuSaveAs()
        {
            if (!ImGui.MenuItem("Save As"))
                return;
        }

        private void FileMenuRestart()
        {
            if (!ImGui.MenuItem("Restart"))
                return;
            
            App.Restart();
        }

        private void FileMenuExit()
        {
            if (!ImGui.MenuItem("Exit"))
                return;
            
            App.Exit();
        }

        private void EditMenu()
        {
            if (!ImGui.BeginMenu("Edit"))
                return;
            
            EditMenuUndo();
            EditMenuRedo();

            ImGui.Separator();

            EditMenuCut();
            EditMenuCopy();
            EditMenuPaste();

            ImGui.Separator();

            EditMenuSelectAll();

            ImGui.Separator();

            EditMenuConfiguration();

            ImGui.EndMenu();
        }

        private void EditMenuUndo()
        {
            if (!ImGui.MenuItem("Undo"))
                return;
        }

        private void EditMenuRedo()
        {
            if (!ImGui.MenuItem("Redo"))
                return;
        }

        private void EditMenuCut()
        {
            if (!ImGui.MenuItem("Cut"))
                return;
        }

        private void EditMenuCopy()
        {
            if (!ImGui.MenuItem("Copy"))
                return;
        }

        private void EditMenuPaste()
        {
            if (!ImGui.MenuItem("Paste"))
                return;
        }

        private void EditMenuSelectAll()
        {
            if (!ImGui.MenuItem("Select All"))
                return;
            
            MapViewer.Selection.SelectAll();
        }

        private void EditMenuConfiguration()
        {
            if (!ImGui.MenuItem("Configuration"))
                return;
        }

        private void ViewMenu()
        {
            if (!ImGui.BeginMenu("View"))
                return;
            
            foreach (UiComponent component in App.UiManager)
            {
                if (component is ICloseable closeable)
                {
                    bool open = closeable.WindowOpen;
                    ImGui.MenuItem(Calc.HumanizeString(component.GetType().Name), closeable.KeyboardShortcut != string.Empty ? closeable.KeyboardShortcut : null, ref open);
                    closeable.WindowOpen = open;
                }
            }

            ImGui.EndMenu();
        }

        private void ModMenu()
        {
            if (!ImGui.BeginMenu("Mod"))
                return;
            
            ModMenuShowDependencies();

            ImGui.EndMenu();
        }

        private void ModMenuShowDependencies()
        {
            if (!ImGui.MenuItem("Show dependencies"))
                return;
            
            ModDependencies.WindowOpen = true;
        }

        private void MapMenu()
        {
            if (!ImGui.BeginMenu("Map"))
                return;
            
            MapMenuStylegrounds();
            MapMenuMetadata();

            ImGui.Separator();

            MapMenuSaveMapImage();

            ImGui.EndMenu();
        }

        private void MapMenuStylegrounds()
        {
            if (!ImGui.MenuItem("Stylegrounds"))
                return;
        }

        private void MapMenuMetadata()
        {
            if (!ImGui.MenuItem("Metadata"))
                return;
        }

        private void MapMenuSaveMapImage()
        {
            if (!ImGui.MenuItem("Save map image"))
                return;
        }

        private void RoomMenu()
        {
            if (!ImGui.BeginMenu("Room"))
                return;
            
            RoomMenuAddRoom();
            RoomMenuAddFiller();
            RoomMenuAddFillerRoom();

            ImGui.Separator();

            RoomMenuEdit();

            ImGui.Separator();

            RoomMenuDelete();

            ImGui.EndMenu();
        }

        private void RoomMenuAddRoom()
        {
            if (!ImGui.MenuItem("Add room"))
                return;
        }

        private void RoomMenuAddFiller()
        {
            if (!ImGui.MenuItem("Add filler"))
                return;
        }

        private void RoomMenuAddFillerRoom()
        {
            if (!ImGui.MenuItem("Add filler room"))
                return;
        }

        private void RoomMenuEdit()
        {
            if (!ImGui.MenuItem("Edit"))
                return;
        }

        private void RoomMenuDelete()
        {
            if (!ImGui.MenuItem("Delete"))
                return;
        }

        private void HelpMenu()
        {
            if (!ImGui.BeginMenu("Help"))
                return;
            
            HelpMenuCheckForUpdates();
            HelpMenuAbout();

            ImGui.EndMenu();
        }

        private void HelpMenuCheckForUpdates()
        {
            if (!ImGui.MenuItem("Check for updates"))
                return;
        }

        private void HelpMenuAbout()
        {
            if (!ImGui.MenuItem("About"))
                return;
        }
    }
}
