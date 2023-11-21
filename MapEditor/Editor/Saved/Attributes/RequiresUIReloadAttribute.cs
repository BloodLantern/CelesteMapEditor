using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiresUIReloadAttribute : ConfigurationEditorAttribute
    {
        public RequiresUIReloadAttribute() => Tooltip = "Requires UI to be reloaded";
    }
}
