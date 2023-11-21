using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigurationEditorAttribute : Attribute
    {
        public string Tooltip;
    }
}
