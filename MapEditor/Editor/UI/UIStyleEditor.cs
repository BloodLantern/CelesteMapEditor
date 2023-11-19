using Editor.Saved;
using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;

namespace Editor.UI
{
    public class UIStyleEditor : UIComponent, ICloseable
    {
        private bool wasOpen = false;
        public bool WindowOpen { get; set; }
        public string KeyboardShortcut { get; set; }

        public UIStyleEditor()
            : base(RenderingCall.StateEditor)
        {
        }

        public override void Render()
        {
            if (!wasOpen && WindowOpen)
                ImGui.SetNextWindowFocus();

            if (WindowOpen)
            {
                bool open = WindowOpen;
                ImGui.Begin("UI Style Editor", ref open);
                WindowOpen = open;

                if (ImGui.BeginTabBar("uiStyleEditorTabBar"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();

                    if (ImGui.BeginTabItem("Layout"))
                    {
                        foreach (PropertyInfo property in style.GetType().GetProperties())
                        {
                            Type type = property.PropertyType;

                            if (type == typeof(bool).MakeByRefType())
                            {
                                bool value = (bool) property.GetValue(style);
                                ImGui.Checkbox(property.Name, ref value);
                                property.SetValue(style, value);
                            }
                            else if (type == typeof(float).MakeByRefType())
                            {
                                float value = (float) property.GetValue(style);
                                ImGui.InputFloat(property.Name, ref value);
                                property.SetValue(style, value);
                            }
                            else if (type == typeof(Vector2).MakeByRefType())
                            {
                                Vector2 value = (Vector2) property.GetValue(style);
                                ImGui.InputFloat2(property.Name, ref value);
                                property.SetValue(style, value);
                            }
                            else if (type.IsEnum)
                            {
                                if (ImGui.BeginCombo(property.Name, property.GetValue(style).ToString()))
                                {
                                    foreach (object enumValue in Enum.GetValues(type))
                                    {
                                        if (ImGui.Selectable(enumValue.ToString()))
                                            property.SetValue(style, enumValue);
                                    }
                                    ImGui.EndCombo();
                                }
                            }
                        }

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Colors"))
                    {
                        RangeAccessor<Vector4> colors = style.Colors;

                        for (int i = 0; i < (int) ImGuiCol.COUNT; i++)
                            ImGui.ColorEdit4(((ImGuiCol) i).ToString(), ref colors[i]);

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
                ImGui.End();
            }

            wasOpen = WindowOpen;
        }
    }
}
