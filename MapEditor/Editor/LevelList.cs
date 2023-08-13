using ImGuiNET;

namespace Editor
{
    public class LevelList
    {
        public const float DefaultWidth = 200f;

        public MapEditor MapEditor;
        public MapViewer MapViewer;

        public float Width { get; private set; } = DefaultWidth;

        public LevelList(MapEditor mapEditor)
        {
            MapEditor = mapEditor;
            MapViewer = MapEditor.MapViewer;
        }

        public void Render()
        {
            float windowHeight = MapEditor.WindowSize.Y - ImGui.GetFrameHeight();
            ImGui.SetNextWindowPos(new(0f, ImGui.GetFrameHeight()));
            ImGui.SetNextWindowSizeConstraints(new(DefaultWidth, windowHeight), new(float.PositiveInfinity, windowHeight));

            ImGui.Begin("Room List", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse);
            Width = ImGui.GetWindowWidth();

            foreach (Level level in MapViewer.CurrentMap.Levels)
            {
                if (ImGui.MenuItem(level.Name))
                {
                    MapViewer.Camera.MoveTo(level.Center);
                }
            }

            ImGui.End();
        }
    }
}
