using ImGuiNET;

namespace Editor.Utils
{
    public static class ImGuiUtils
    {
        public static bool ButtonCenteredOnLine(string label, float alignment = 0.5f)
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            float size = ImGui.CalcTextSize(label).X + style.FramePadding.X * 2.0f;
            float avail = ImGui.GetContentRegionAvail().X;

            float off = (avail - size) * alignment;
            if (off > 0.0f)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);

            return ImGui.Button(label);
        }

        private static bool errorPopupOpen = false;
        public static void ErrorInfoDialog(string message)
        {
            ImGui.OpenPopup("Error");
            
            bool errorPopupOpen = true;
            if (ImGui.BeginPopupModal("Error", ref errorPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"An unexpected error occured: {message}");
                if (ButtonCenteredOnLine("Close"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
        }
    }
}
