using Editor.Saved;
using Editor.Saved.Attributes;
using Editor.Utils;
using ImGuiNET;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace Editor.UI.Components
{
    public class ConfigurationEditor : UIComponent, ICloseable
    {
        private bool wasOpen = false;
        public bool WindowOpen { get; set; }
        public string KeyboardShortcut { get; set; }

        private Config config, oldConfig;
        private bool configChanged = false;

        public ConfigurationEditor()
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
                ImGui.Begin("Configuration Editor", ref open);
                WindowOpen = open;

                if (!wasOpen)
                    oldConfig = (Config)config.Clone();

                ImGui.BeginChild("configFields", new(-1), true, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.AlwaysAutoResize);

                foreach (FieldInfo field in config.GetType().GetFields())
                {
                    if (field.IsLiteral)
                        continue;

                    RenderField(field, config);
                }

                ImGui.EndChild();

                if (configChanged)
                {
                    if (ImGui.Button("Cancel"))
                    {

                    }

                    if (ImGui.Button("Apply"))
                    {
                        oldConfig = null;
                        configChanged = false;
                    }
                }
                else
                {
                    if (ImGuiUtils.ReleasedClickOnSomething() && !config.Equals(oldConfig))
                        configChanged = true;
                }

                ImGui.End();
            }

            wasOpen = WindowOpen;
        }

        public override void Load(Config config)
        {
            base.Load(config);

            this.config = config;
        }

        private void RenderField(FieldInfo field, object instance)
        {
            if (field.GetCustomAttribute<NotDisplayedOnEditorAttribute>() != null)
                return;

            Type type = field.FieldType;

            if (type == typeof(bool))
            {
                bool value = (bool)field.GetValue(instance);
                ImGui.Checkbox(field.Name, ref value);
                field.SetValue(instance, value);
            }
            else if (type == typeof(int))
            {
                int value = (int)field.GetValue(instance);
                ImGui.InputInt(field.Name, ref value);
                field.SetValue(instance, value);
            }
            else if (type == typeof(float))
            {
                float value = (float)field.GetValue(instance);
                ImGui.InputFloat(field.Name, ref value);
                field.SetValue(instance, value);
            }
            else if (type.IsEnum)
            {
                if (ImGui.BeginCombo(field.Name, field.GetValue(instance).ToString()))
                {
                    foreach (object enumValue in Enum.GetValues(type))
                    {
                        if (ImGui.Selectable(enumValue.ToString()))
                            field.SetValue(instance, enumValue);
                    }
                    ImGui.EndCombo();
                }
            }
            else if (type == typeof(Color))
            {
                System.Numerics.Vector4 value = ((Color)field.GetValue(instance)).ToVector4().ToNumerics();
                ImGui.ColorEdit4(field.Name, ref value);
                field.SetValue(instance, new Color(value));
            }
            else if (type.IsValueType || type.IsClass)
            {
                object typeValue = field.GetValue(instance);
                if (typeValue is ICustomConfigurationEditorDisplay customDisplay)
                {
                    customDisplay.Render(field, instance);
                }
                else if (ImGui.TreeNodeEx(field.Name, ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.FramePadding))
                {
                    foreach (FieldInfo f in type.GetFields())
                        RenderField(f, typeValue);
                    ImGui.TreePop();
                }
            }
        }
    }
}
