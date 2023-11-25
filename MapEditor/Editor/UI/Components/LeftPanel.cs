using Editor.Extensions;
using Editor.Utils;
using ImGuiNET;
using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace Editor.UI.Components
{
    public class LeftPanel : UIComponent
    {
        private const float DefaultWidth = 200f;
        private const ImGuiWindowFlags WindowFlags =
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoSavedSettings;
        private const float EndingX = 0f;
        private const float MoveInDuration = 0.5f;
        private const string Title = "leftPanel";

        private MenuBar menuBar;

        public LevelList LevelList;
        public ModExplorer ModExplorer;

        public float CurrentX { get; private set; } = -DefaultWidth;
        public bool Visible = false;

        public Vector2 Size { get; private set; } = new(DefaultWidth, 1f);

        public LeftPanel(Application app)
            : base(RenderingCall.StateEditor)
        {
            LevelList = new(app.MapViewer);
            ModExplorer = new(app);
        }

        internal override void Initialize(UIManager manager)
        {
            menuBar = manager.GetComponent<MenuBar>();

            base.Initialize(manager);
        }

        public override void Render()
        {
            if (!Visible)
                return;

            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            float windowHeight = viewport.Size.Y - menuBar.CurrentY - menuBar.Size.Y;
            ImGui.SetNextWindowSizeConstraints(new(DefaultWidth, windowHeight), new(float.PositiveInfinity, windowHeight));
            ImGui.SetNextWindowPos(new(CurrentX, menuBar.Size.Y + menuBar.CurrentY));

            ImGui.Begin(Title, WindowFlags);
            Size = ImGui.GetWindowSize();

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

        public void StartMoveInRoutine() => Coroutine.Start(MoveInRoutine(-Size.X, EndingX, MoveInDuration));

        private IEnumerator MoveInRoutine(float startingX, float endingX, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                CurrentX = Calc.EaseLerp(startingX, endingX, timer, duration, Ease.CubeOut);

                yield return null;
            }

            CurrentX = endingX;
        }
    }
}
