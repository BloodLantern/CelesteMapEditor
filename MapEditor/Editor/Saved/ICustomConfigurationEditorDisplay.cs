using System.Reflection;

namespace Editor.Saved
{
    public interface ICustomConfigurationEditorDisplay
    {
        void Render(FieldInfo field, object instance);
    }
}
