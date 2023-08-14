using ImGuiNET;

namespace Editor.UI
{
    public class ModDependencies
    {
        public Session Session;

        public ModDependencies(Session session) => Session = session;

        public void Render()
        {
            if (ImGui.Begin("Mod Dependencies"))
            {
                ImGui.Columns(2);
                ImGui.Text("Mod Name");
                ImGui.NextColumn();
                ImGui.Text("Version");
                ImGui.NextColumn();
                ImGui.Separator();
                foreach (CelesteMod mod in Session.CelesteMods)
                {
                    ImGui.Text(mod.Name);
                    ImGui.NextColumn();
                    ImGui.Text(mod.VersionString);
                    ImGui.NextColumn();
                    ImGui.Separator();
                }
                ImGui.End();
            }
        }
    }
}
