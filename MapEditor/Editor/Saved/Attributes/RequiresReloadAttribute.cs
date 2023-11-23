using Editor.Extensions;

namespace Editor.Saved.Attributes
{
    public class RequiresReloadAttribute : ConfigurationEditorAttribute
    {
        public ReloadType Type;

        public RequiresReloadAttribute(ReloadType type) : base(type.GetTooltip()) => Type = type;
    }
}
