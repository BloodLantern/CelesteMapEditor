using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigurationEditorAttribute : Attribute
    {
        public readonly string Tooltip;

        public ConfigurationEditorAttribute(string tooltip) => Tooltip = tooltip;
    }
}
