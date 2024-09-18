using ImGuiNET;

namespace Editor.UI.Components
{
    public class ModExplorer : UiComponent
    {
        private Application app;

        public ModExplorer(Application app) : base(RenderingCall.StateEditor) => this.app = app;

        public override void Render()
        {
            ImGui.Begin("Mod Explorer");
            ImGui.Text("Not implemented yet");
            ImGui.End();
        }
    }
}
