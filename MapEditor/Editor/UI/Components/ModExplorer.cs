﻿using ImGuiNET;

namespace Editor.UI.Components
{
    public class ModExplorer : UiComponent
    {
        private Application app;

        public ModExplorer(Application app) : base(RenderingCall.None) => this.app = app;

        public override void Render()
        {
            ImGui.Text("Not implemented yet");
        }
    }
}
