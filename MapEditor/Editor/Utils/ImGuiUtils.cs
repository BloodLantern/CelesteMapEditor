using Editor.Saved;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;

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

        public static Keybind KeybindPicker(string label, Keybind keybind)
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button(keybind.ToString()))
                ImGui.OpenPopupOnItemClick(label, ImGuiPopupFlags.MouseButtonLeft);

            Keybind result = keybind;
            if (ImGui.BeginPopupModal(label))
            {
                ImGui.Text("Press any keyboard or mouse input");
                ImGui.Separator();

                if (ImGui.Button("Cancel (Esc)"))
                {
                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();

                    if (pressedKeys.Length > 0)
                    {
                        Keys key = pressedKeys[0];
                        if (key != Keys.Escape)
                            result = key;
                        ImGui.CloseCurrentPopup();
                    }
                    else
                    {
                        MouseStateExtended mouse = MouseExtended.GetState();

                        // Only accept the left mouse button as the new input if not hovering the cancel button.
                        if (mouse.LeftButton == ButtonState.Pressed && !ImGui.IsItemHovered())
                        {
                            result = MouseButton.Left;
                            ImGui.CloseCurrentPopup();
                        }

                        MouseButton[] buttons = Enum.GetValues<MouseButton>();
                        for (int i = (int) MouseButton.Left + 1; i < buttons.Length; i++)
                        {
                            MouseButton button = buttons[i];
                            if (mouse.IsButtonDown(button))
                            {
                                result = button;
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                }
                ImGui.EndPopup();
            }

            return result;
        }
    }
}
