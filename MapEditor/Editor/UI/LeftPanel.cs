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
        private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse;
        private const float StartingX = -DefaultWidth;
        private const float EndingX = 0f;
        private const float MoveInDuration = 1f;

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

        public static void StartMoveInRoutine() => Coroutine.Start(MoveInRoutine(StartingX, EndingX, MoveInDuration));

        private static IEnumerator MoveInRoutine(float startingX, float endingX, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                ImGui.SetWindowPos("#leftPanel", new(Calc.EaseLerp(startingX, endingX, timer, duration, Ease.QuadOut), ImGui.GetFrameHeight()));

                yield return null;
            }

            ImGui.SetWindowPos("#leftPanel", new(endingX, ImGui.GetFrameHeight()));
        }
    }
}
