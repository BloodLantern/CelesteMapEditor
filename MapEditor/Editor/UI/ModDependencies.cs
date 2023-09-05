using ImGuiNET;

namespace Editor.UI
{
    public class ModDependencies
    {
        public Session Session;
        public bool Open = false;
        private bool wasOpen = false;

        private readonly bool[] enabledMods;
        private string searchText = string.Empty;

        public ModDependencies(Session session)
        {
            Session = session;
            enabledMods = new bool[Session.CelesteMods.Count];
            for (int i = 0; i < enabledMods.Length; i++)
                enabledMods[i] = Session.CelesteMods[i].Enabled;
        }

        public void Render()
        {
            if (!wasOpen && Open)
                ImGui.SetNextWindowFocus();

            if (Open)
            {
                bool modified = IsModified();
                ImGui.Begin("Mod Dependencies", ref Open);

                ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
                ImGui.InputTextWithHint("##search", "Search...", ref searchText, 30);

                float itemHeight = ImGui.GetItemRectSize().Y;
                System.Numerics.Vector2 childSize = new(-1, ImGui.GetWindowHeight() - itemHeight * 2 - ImGui.GetStyle().ItemSpacing.Y - ImGui.GetFrameHeightWithSpacing());
                if (modified)
                    childSize.Y -= itemHeight;

                ImGui.BeginChild("List", childSize, false, ImGuiWindowFlags.AlwaysAutoResize);
                foreach (CelesteMod mod in Session.CelesteMods)
                {
                    if (searchText == string.Empty || mod.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase))
                        ModEntry(mod);
                }
                ImGui.EndChild();

                if (modified)
                {
                    if (ImGui.Button("Cancel"))
                    {
                        for (int i = 0; i < enabledMods.Length; i++)
                            enabledMods[i] = Session.CelesteMods[i].Enabled;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Apply"))
                    {
                        for (int i = 0; i < enabledMods.Length; i++)
                            Session.CelesteMods[i].Enabled = enabledMods[i];
                        CelesteMod.UpdateAll(Session);
                    }
                }

                ImGui.End();
            }

            wasOpen = Open;
        }

        private void ModEntry(CelesteMod mod)
        {
            if (mod.ForceEnabled)
                ImGui.BeginDisabled();

            ImGui.Checkbox($"##{mod.Index}", ref enabledMods[mod.Index]);

            if (mod.ForceEnabled)
                ImGui.EndDisabled();

            ImGui.SameLine();

            if (mod.DLL == null && mod.Dependencies.Count == 0 && mod.OptionalDependencies.Count == 0)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 27);
                ImGui.Text(mod.NameAndVersion);
                return;
            }

            if (ImGui.TreeNode(mod.NameAndVersion))
            {
                if (mod.DLL != null)
                    ImGui.Text($"DLL path: {mod.DLL}");

                if (mod.Dependencies.Count > 0 && ImGui.TreeNode($"Dependencies: {mod.Dependencies.Count}"))
                {
                    foreach (CelesteMod dependency in mod.Dependencies)
                        ModEntry(dependency);

                    ImGui.TreePop();
                }

                if (mod.OptionalDependencies.Count > 0 && ImGui.TreeNode($"Optional dependencies: {mod.OptionalDependencies.Count}"))
                {
                    foreach (CelesteMod dependency in mod.OptionalDependencies)
                        ModEntry(dependency);

                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }
        }

        public bool IsModified()
        {
            for (int i = 0; i < enabledMods.Length; i++)
                if (enabledMods[i] != Session.CelesteMods[i].Enabled)
                    return true;
            return false;
        }
    }
}
