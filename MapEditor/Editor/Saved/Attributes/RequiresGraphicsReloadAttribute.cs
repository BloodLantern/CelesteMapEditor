using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresGraphicsReloadAttribute : ConfigurationEditorAttribute
    {
        public RequiresGraphicsReloadAttribute() => Tooltip = "Requires graphics to be reloaded";
    }
}
