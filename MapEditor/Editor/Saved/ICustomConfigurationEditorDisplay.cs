using System.Reflection;

namespace Editor.Saved
{
    /// <summary>
    /// Any class implementing this interface is able to implement the <see cref="Render(FieldInfo, object)"/>
    /// method used to customize how a type is rendered in the configuration editor.
    /// </summary>
    public interface ICustomConfigurationEditorDisplay
    {
        /// <summary>
        /// Calls the necessary ImGui functions to display the custom configuration
        /// editor widget used to change the <paramref name="instance"/>'s <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The <paramref name="instance"/>'s field to display and change.</param>
        /// <param name="instance">The object instance containing the <paramref name="field"/> to display and change.</param>
        /// <returns>Whether the field value changed.</returns>
        bool Render(FieldInfo field, object instance);
    }
}
