using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using System;

namespace Editor.Utils
{
    public class ImRenderer : ImGuiRenderer
    {
        public IntPtr Context;

        public ImRenderer(Game game)
            : base(game) => Context = ImGui.GetCurrentContext();

        public override void BeforeLayout(GameTime gameTime)
        {
            ImGui.SetCurrentContext(Context);
            base.BeforeLayout(gameTime);
        }
    }
}
