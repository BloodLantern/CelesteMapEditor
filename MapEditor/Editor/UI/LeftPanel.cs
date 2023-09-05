using ImGuiNET;

namespace Editor.UI
{
    public class LeftPanel
    {
        public const float DefaultWidth = 200f;
        public const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse;

        public MapEditor MapEditor;

        public LevelList LevelList;
        public ModExplorer ModExplorer;

        public float Width { get; private set; } = DefaultWidth;

        public LeftPanel(MapEditor mapEditor)
        {
            MapEditor = mapEditor;
            LevelList = new(mapEditor);
            ModExplorer = new(mapEditor);
        }

        public void Render()
        {
            float windowHeight = MapEditor.WindowSize.Y - ImGui.GetFrameHeight();
            ImGui.SetNextWindowPos(new(0f, ImGui.GetFrameHeight()));
            ImGui.SetNextWindowSizeConstraints(new(DefaultWidth, windowHeight), new(float.PositiveInfinity, windowHeight));

            ImGui.Begin("#leftPanel", WindowFlags);
            Width = ImGui.GetWindowWidth();

            if (ImGui.BeginTabBar("#leftPanelTabBar"))
            {
                if (ImGui.BeginTabItem("Room List"))
                {
                    LevelList.Render();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Mod Explorer"))
                {
                    ModExplorer.Render();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
        }
    }
}
