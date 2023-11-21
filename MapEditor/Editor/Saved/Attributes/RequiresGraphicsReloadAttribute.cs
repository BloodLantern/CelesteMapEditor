using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresGraphicsReloadAttribute : ConfigurationEditorAttribute
    {
        RequiresGraphicsReloadAttribute() => Tooltip = "Requires graphics to be reloaded";
    }
}
