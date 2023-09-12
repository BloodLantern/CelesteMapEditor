using Editor.Extensions;
using Editor.Utils;
using ImGuiNET;
using System.Collections;
using System.Diagnostics;

namespace Editor.UI
{
    public class LeftPanel
    {
        private const float DefaultWidth = 200f;
        private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
        private const float EndingX = 0f;
        private const float MoveInDuration = 0.5f;
        private const string Title = "leftPanel";

        private readonly MapEditor mapEditor;
        private readonly MenuBar menuBar;

        public LevelList LevelList;
        public ModExplorer ModExplorer;

        public float Width { get; private set; } = DefaultWidth;

        public LeftPanel(MapEditor mapEditor)
        {
            this.mapEditor = mapEditor;
            menuBar = mapEditor.MenuBar;
            LevelList = new(mapEditor);
            ModExplorer = new(mapEditor);
        }

        public void Render()
        {
            float windowHeight = mapEditor.WindowSize.Y - menuBar.Size.Y;
            ImGui.SetNextWindowSizeConstraints(new(DefaultWidth, windowHeight), new(float.PositiveInfinity, windowHeight));

            ImGui.Begin(Title, WindowFlags);
            Width = ImGui.GetWindowWidth();

            if (ImGui.IsWindowAppearing())
                StartMoveInRoutine();

            if (ImGui.Button("Move"))
                StartMoveInRoutine();

            if (ImGui.Button("MoveMenuBar"))
                menuBar.StartMoveInRoutine();

            if (ImGui.BeginTabBar("leftPanelTabBar"))
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

        public void StartMoveInRoutine() => Coroutine.Start(MoveInRoutine(-Width, EndingX, MoveInDuration));

        private IEnumerator MoveInRoutine(float startingX, float endingX, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                ImGui.SetWindowPos(Title, new(Calc.EaseLerp(startingX, endingX, timer, duration, Ease.CubeOut), menuBar.Size.Y));

                yield return null;
            }

            ImGui.SetWindowPos(Title, new(endingX, menuBar.Size.Y));
        }
    }
}
