using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;

namespace Editor.Saved.Keybinds
{
    [Serializable]
    public class MapViewerKeybindsConfig : ConfigBase
    {
        /// <summary>
        /// Keybind used to move the camera.
        /// </summary>
        public Keybind CameraMove = MouseButton.Middle;

        /// <summary>
        /// Keybind used to select an object.
        /// </summary>
        public Keybind Select = MouseButton.Left;

        /// <summary>
        /// Keybind used to deselect all selected object.
        /// </summary>
        public Keybind Deselect = Keys.Escape;

        /// <summary>
        /// Keybind used to delete the selected objects.
        /// </summary>
        public Keybind Delete = Keys.Delete;

        public override object Clone() => CloneFields<MapViewerKeybindsConfig>();
    }
}
