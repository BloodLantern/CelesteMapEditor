using Editor.Saved;
using Editor.Saved.Attributes;
using ImGuiNET;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Editor.UI.Components
{
    public class ConfigurationEditor : UIComponent, ICloseable
    {
        private class ReloadDataDifference
        {
            public string Name;

            public List<ReloadType> ReloadTypes = new();
        }

        private class ReloadData
        {
            public ReloadType HighestReloadType;

            public List<ReloadDataDifference> Differences = new();
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

                ImGui.BeginChild("configFields", ImGui.GetContentRegionAvail() - System.Numerics.Vector2.UnitY * ImGui.GetItemRectSize().Y, ImGuiChildFlags.Border);

                bool configValueChanged = false;
                foreach (FieldInfo field in config.GetType().GetFields())
                {
                    if (field.IsLiteral)
                        continue;

                    configValueChanged |= RenderField(field, newConfig);
                }

                ImGui.EndChild();

                bool openPopup = false;
                if (ImGui.Button("OK"))
                {
                    if (configChanged)
                        openPopup = CheckAndApplyChanges();
                    WindowOpen = false;
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    if (configChanged)
                        CancelChanges();
                    WindowOpen = false;
                }

                if (!configChanged)
                {
                    if (configValueChanged)
                    {
                        configChanged = true;
                        // Using a goto here avoids another 'if (!configChanged)'
                        goto afterBeginDisabled;
                    }

                    // Disable the Cancel and Apply buttons if the config hasn't changed
                    ImGui.BeginDisabled();
                }
afterBeginDisabled:

                ImGui.SameLine();

                if (ImGui.Button("Apply"))
                    openPopup = CheckAndApplyChanges();

                // We need to use a bool here because of how ImGui popup modals work
                if (openPopup)
                    ImGui.OpenPopup("Warning");

                if (ImGui.BeginPopupModal("Warning"))
                {
                    ImGui.Text("Some changes require a reload to take effect");

                    foreach (ReloadDataDifference difference in currentReloadData.Differences)
                    {
                        if (difference.ReloadTypes.Count == 0)
                            continue;

                        ImGui.Text($"{difference.Name}: {string.Join(", ", difference.ReloadTypes)}");
                    }

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

        public bool CheckAndApplyChanges()
        {
            currentReloadData = new();

            foreach (Difference diff in fullComparer.Compare(config, newConfig).Differences)
            {
                FieldInfo field = diff.ParentObject1.GetType().GetField(diff.PropertyName[(diff.ParentPropertyName.Length + 1)..]);

                if (field == null)
                    continue;

                ReloadDataDifference reloadDiff = new() { Name = Calc.HumanizeString(field.Name) };

                foreach (Attribute attribute in field.GetCustomAttributes())
                {
                    switch (attribute)
                    {
                        case RequiresReloadAttribute requiresReloadAttribute:
                            ReloadType attributeReloadType = requiresReloadAttribute.Type;

                            reloadDiff.ReloadTypes.Add(attributeReloadType);

                            if (attributeReloadType > currentReloadData.HighestReloadType)
                                currentReloadData.HighestReloadType = attributeReloadType;

                            break;
                    }
                }

                currentReloadData.Differences.Add(reloadDiff);
            }

            if (currentReloadData.HighestReloadType > ReloadType.None)
                return true;
            else
                ApplyChanges();

            return false;
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
            switch (currentReloadData.HighestReloadType)
            {
                case ReloadType.Graphics:
                    app.ReloadGraphics(config.Graphics);
                    break;
                case ReloadType.FullEditor:
                    app.Restart();
                    break;
            }
        }

        /// <summary>
        /// Renders a single field in the configuration editor or all of its children if it is a class or struct type.
        /// </summary>
        /// <param name="field">The field to render.</param>
        /// <param name="instance">The object instance owning the field.</param>
        /// <returns>Whether the field or one of its children's value changed.</returns>
        private bool RenderField(FieldInfo field, object instance)
        {
            if (field.GetCustomAttribute<NotDisplayedOnEditorAttribute>() != null)
                return false;

            Type type = field.FieldType;
            string displayName = Calc.HumanizeString(field.Name);

            bool valueChanged = false;

            // The code here is terrible, there is certainly a better way to do this
            if (type == typeof(bool))
            {
                bool oldValue = (bool) field.GetValue(instance);
                bool newValue = oldValue;

                ImGui.Checkbox(displayName, ref newValue);
                field.SetValue(instance, newValue);

                valueChanged = oldValue != newValue;
            }
            else if (type == typeof(int))
            {
                int oldValue = (int) field.GetValue(instance);
                int newValue = oldValue;

                ImGui.InputInt(displayName, ref newValue);
                field.SetValue(instance, newValue);

                valueChanged = oldValue != newValue;
            }
            else if (type == typeof(float))
            {
                float oldValue = (float) field.GetValue(instance);
                float newValue = oldValue;

                ImGui.InputFloat(displayName, ref newValue);
                field.SetValue(instance, newValue);

                valueChanged = oldValue != newValue;
            }
            else if (type.IsEnum)
            {
                if (ImGui.BeginCombo(displayName, field.GetValue(instance).ToString()))
                {
                    foreach (object enumValue in Enum.GetValues(type))
                    {
                        if (ImGui.Selectable(enumValue.ToString()))
                        {
                            field.SetValue(instance, enumValue);
                            valueChanged = true;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else if (type == typeof(Color))
            {
                Color oldValue = (Color) field.GetValue(instance);
                System.Numerics.Vector4 value = oldValue.ToVector4().ToNumerics();

                ImGui.ColorEdit4(displayName, ref value);

                Color newValue = new Color(value);
                field.SetValue(instance, newValue);

                valueChanged = oldValue != newValue;
            }
            else if (type == typeof(string))
            {
                string oldValue = (string) field.GetValue(instance);

                byte[] data = Encoding.Unicode.GetBytes(oldValue);
                data = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, data);

                ImGui.InputText(displayName, data, 0xFF);

                data = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, data);
                string newValue = Encoding.Unicode.GetString(data).Trim();

                field.SetValue(instance, newValue);

                valueChanged = oldValue != newValue;
            }
            else if (type.IsValueType || type.IsClass)
            {
                object typeValue = field.GetValue(instance);
                if (typeValue is ICustomConfigurationEditorDisplay customDisplay)
                {
                    valueChanged = customDisplay.Render(field, instance);
                }
                else if (ImGui.TreeNodeEx(displayName, ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.FramePadding))
                {
                    foreach (FieldInfo f in type.GetFields())
                        valueChanged |= RenderField(f, typeValue);
                    ImGui.TreePop();
                }
            }

            foreach (Attribute attribute in field.GetCustomAttributes())
            {
                if (attribute is ConfigurationEditorAttribute configurationEditorAttribute)
                    ImGui.SetItemTooltip(configurationEditorAttribute.Tooltip);
            }

            return valueChanged;
        }
    }
}
