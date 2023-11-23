using Editor.Saved;
using Editor.Saved.Attributes;
using Editor.Utils;
using ImGuiNET;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Editor.UI.Components
{
    public class ConfigurationEditor : UIComponent, ICloseable
    {
        private class ReloadData
        {
            public ReloadType ReloadType;

            public List<Difference> Differences;
        }

        private bool wasOpen = false;
        public bool WindowOpen { get; set; }
        public string KeyboardShortcut { get; set; }

        private readonly Application app;
        private Config config, newConfig;
        private bool configChanged = false;

        private readonly CompareLogic defaultComparer, fullComparer;

        private ReloadData currentReloadData;

        public ConfigurationEditor(Application app)
            : base(RenderingCall.StateEditor)
        {
            this.app = app;

            defaultComparer = new CompareLogic(new ComparisonConfig() { AttributesToIgnore = { typeof(ComparisonIgnoreAttribute) } });
            fullComparer = new CompareLogic(new ComparisonConfig() { AttributesToIgnore = { typeof(ComparisonIgnoreAttribute) }, MaxDifferences = int.MaxValue });
        }

        public override void Render()
        {
            if (!wasOpen && WindowOpen)
                ImGui.SetNextWindowFocus();

            if (WindowOpen)
            {
                bool open = WindowOpen;
                ImGui.SetNextWindowSizeConstraints(new(500f, 300f), new(float.PositiveInfinity));
                ImGui.Begin("Configuration Editor", ref open, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                WindowOpen = open;

                newConfig ??= (Config) config.Clone();

                ImGui.BeginChild(
                    "configFields",
                    ImGui.GetContentRegionAvail() - System.Numerics.Vector2.UnitY * ImGui.GetItemRectSize().Y,
                    true,
                    ImGuiWindowFlags.NoFocusOnAppearing
                );

                foreach (FieldInfo field in config.GetType().GetFields())
                {
                    if (field.IsLiteral)
                        continue;

                    RenderField(field, newConfig);
                }

                ImGui.EndChild();

                if (!configChanged)
                {
                    // FIXME: A lag spike is caused here because we compare the two config instances
                    if (ImGuiUtils.ReleasedClickOnSomething() && !defaultComparer.Compare(config, newConfig).AreEqual)
                    {
                        configChanged = true;
                        goto afterBeginDisabled;
                    }

                    // Disable the Cancel and Apply buttons if the config hasn't changed
                    ImGui.BeginDisabled();
                }
afterBeginDisabled:

                if (ImGui.Button("Cancel"))
                    CancelChanges();

                ImGui.SameLine();

                if (ImGui.Button("Apply"))
                {
                    currentReloadData = new()
                    {
                        Differences = fullComparer.Compare(config, newConfig).Differences
                    };

                    foreach (Difference diff in currentReloadData.Differences)
                    {
                        FieldInfo field = diff.ParentObject1.GetType().GetField(diff.PropertyName[(diff.ParentPropertyName.Length + 1)..]);

                        if (field == null)
                            continue;

                        foreach (Attribute attribute in field.GetCustomAttributes())
                        {
                            switch (attribute)
                            {
                                case RequiresReloadAttribute requiresReloadAttribute:
                                    ReloadType attributeReloadType = requiresReloadAttribute.Type;
                                    if (attributeReloadType > currentReloadData.ReloadType)
                                        currentReloadData.ReloadType = attributeReloadType;
                                    break;
                            }
                        }
                    }

                    if (currentReloadData.ReloadType > ReloadType.None)
                        ImGui.OpenPopup("Warning");
                    else
                        ApplyChanges();
                }

                if (ImGui.BeginPopupModal("Warning"))
                {
                    ImGui.Text($@"Some changes require a {Calc.HumanizeString(currentReloadData.ReloadType.ToString()).ToLower()} reload to take effect");

                    if (ImGui.Button("Cancel"))
                    {
                        CancelChanges();
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Continue without these changes"))
                    {
                        ApplyChanges();
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Apply now"))
                    {
                        ApplyChanges();
                        ApplyReload();
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }

                if (!configChanged)
                    ImGui.EndDisabled();

                ImGui.End();
            }

            wasOpen = WindowOpen;
        }

        public override void Load(Config config)
        {
            base.Load(config);

            this.config = config;
        }

        public void ApplyChanges()
        {
            config.CopyFrom(newConfig);
            CancelChanges();
        }

        public void CancelChanges()
        {
            newConfig = null;
            configChanged = false;
        }

        public void ApplyReload()
        {
            switch (currentReloadData.ReloadType)
            {
                case ReloadType.Graphics:
                    app.ReloadGraphics(config.Graphics);
                    break;
                case ReloadType.FullEditor:
                    app.Restart();
                    break;
            }
        }

        private void RenderField(FieldInfo field, object instance)
        {
            if (field.GetCustomAttribute<NotDisplayedOnEditorAttribute>() != null)
                return;

            Type type = field.FieldType;
            string displayName = Calc.HumanizeString(field.Name);

            if (type == typeof(bool))
            {
                bool value = (bool)field.GetValue(instance);
                ImGui.Checkbox(displayName, ref value);
                field.SetValue(instance, value);
            }
            else if (type == typeof(int))
            {
                int value = (int)field.GetValue(instance);
                ImGui.InputInt(displayName, ref value);
                field.SetValue(instance, value);
            }
            else if (type == typeof(float))
            {
                float value = (float)field.GetValue(instance);
                ImGui.InputFloat(displayName, ref value);
                field.SetValue(instance, value);
            }
            else if (type.IsEnum)
            {
                if (ImGui.BeginCombo(displayName, field.GetValue(instance).ToString()))
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
                ImGui.ColorEdit4(displayName, ref value);
                field.SetValue(instance, new Color(value));
            }
            else if (type.IsValueType || type.IsClass)
            {
                object typeValue = field.GetValue(instance);
                if (typeValue is ICustomConfigurationEditorDisplay customDisplay)
                {
                    customDisplay.Render(field, instance);
                }
                else if (ImGui.TreeNodeEx(displayName, ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.FramePadding))
                {
                    foreach (FieldInfo f in type.GetFields())
                        RenderField(f, typeValue);
                    ImGui.TreePop();
                }
            }

            foreach (Attribute attribute in field.GetCustomAttributes())
            {
                if (attribute is ConfigurationEditorAttribute configurationEditorAttribute)
                    ImGui.SetItemTooltip(configurationEditorAttribute.Tooltip);
            }
        }
    }
}
