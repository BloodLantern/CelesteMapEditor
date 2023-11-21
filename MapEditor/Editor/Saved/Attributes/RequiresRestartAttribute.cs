using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresRestartAttribute : ConfigurationEditorAttribute
    {
        RequiresRestartAttribute() => Tooltip = "Requires restart";
    }
}
