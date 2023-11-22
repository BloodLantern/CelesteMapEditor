using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresRestartAttribute : ConfigurationEditorAttribute
    {
        public RequiresRestartAttribute() => Tooltip = "Requires restart";
    }
}
