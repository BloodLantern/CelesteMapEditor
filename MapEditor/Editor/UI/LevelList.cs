using ImGuiNET;
using System.Collections.Generic;

namespace Editor.UI
{
    public class LevelList
    {
        public Application App;
        public MapViewer MapViewer;

        public LevelList(Application app)
        {
            App = app;
            MapViewer = App.MapViewer;
        }

        private bool roomsCheckbox = true;
        private bool fillersCheckbox = true;
        private bool fillerRoomsCheckbox = true;

        public void Render()
        {
            if (MapViewer.CurrentMap == null)
                return;

            List<Level> levels = GetLevels(false);
            List<Filler> fillers = GetFillers();
            List<Level> fillerLevels = GetLevels(true);

            if (levels.Count > 0)
                ImGui.Checkbox("Rooms", ref roomsCheckbox);
            if (fillers.Count > 0)
            {
                if (levels.Count > 0)
                    ImGui.SameLine();
                ImGui.Checkbox("Fillers", ref fillersCheckbox);
            }
            if (fillerLevels.Count > 0)
            {
                if (levels.Count > 0 || fillers.Count > 0)
                    ImGui.SameLine();
                ImGui.Checkbox("Filler Rooms", ref fillerRoomsCheckbox);
            }

            List<object> list = new();

            if (roomsCheckbox)
                list.AddRange(levels);

            if (fillersCheckbox)
                list.AddRange(fillers);

            if (fillerRoomsCheckbox)
                list.AddRange(fillerLevels);

            list.Sort(
                (a, b) =>
                {
                    if (a is Level levelA)
                    {
                        if (b is Level levelB)
                            return levelA.Name.CompareTo(levelB.Name);
                        else if (b is Filler fillerB)
                            return levelA.Name.CompareTo(fillerB.DisplayName);
                    }
                    else if (a is Filler fillerA)
                    {
                        if (b is Level levelB)
                            return fillerA.DisplayName.CompareTo(levelB.Name);
                        else if (b is Filler fillerB)
                            return fillerA.DisplayName.CompareTo(fillerB.DisplayName);
                    }

                    return 1;
                }
            );

            foreach (object o in list)
            {
                if (o is Level level)
                {
                    ImGui.Selectable(level.Name, ref level.Selected, ImGuiSelectableFlags.AllowDoubleClick);
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        MapViewer.Camera.MoveTo(level.Center);
                }
                else if (o is Filler filler)
                {
                    ImGui.Selectable(filler.DisplayName, ref filler.Selected, ImGuiSelectableFlags.AllowDoubleClick);
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        MapViewer.Camera.MoveTo(filler.Center.ToVector2());
                }
            }
        }

        private List<Level> GetLevels(bool filler) => MapViewer.CurrentMap.Levels.FindAll(l => l.Filler == filler);
        private List<Filler> GetFillers() => MapViewer.CurrentMap.Fillers;
    }
}
