using Editor.Extensions;
using Editor.Logging;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;

namespace Editor.UI
{
    public class DebugConsole
    {
        public LogLevel LogLevel = LogLevel.Debug;
        public Session Session;

        public DebugConsole(Session session) => Session = session;

        public void Render()
        {
            ImGui.Begin("Debug console", ImGuiWindowFlags.NoFocusOnAppearing);

            if (ImGui.BeginCombo("Log level", LogLevel.ToString()))
            {
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    if (ImGui.Selectable(level.ToString()))
                        LogLevel = level;
                }
                ImGui.EndCombo();
            }

            int scroll = 0;
            if (ImGui.Button("Scroll up"))
                scroll = -1;
            ImGui.SameLine();
            if (ImGui.Button("Scroll down") || Logger.LoggedLastFrame || ImGui.IsWindowAppearing())
                scroll = 1;

            ImGui.BeginChild("Logs", new(-1), false, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.AlwaysAutoResize);

            if (scroll == -1)
                ImGui.SetScrollHereY(0f);

            // Show the current logs
            Color color = Color.White;
            foreach (string log in Logger.LogEntries.ToArray())
            {
                LogLevel level = Logger.GetLevel(log);
                if ((byte) level < (byte) LogLevel)
                    continue;

                color = level switch
                {
                    LogLevel.Debug => Color.LightBlue,
                    LogLevel.Info => Color.LightGreen,
                    LogLevel.Warning => Color.Orange,
                    LogLevel.Error => Color.Red,
                    LogLevel.Fatal => Color.DarkRed,
                    _ => throw new InvalidOperationException("Invalid log level")
                };

                if (Session.Config.UiStyle == ImGuiStyles.Style.Light)
                    color = color.Inverse();

                ImGui.TextColored(color.ToVector4().ToNumerics(), log);
            }

            if (scroll == 1)
                ImGui.SetScrollHereY(1f);

            ImGui.EndChild();

            ImGui.End();
        }
    }
}
