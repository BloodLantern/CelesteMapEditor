using Editor.Extensions;
using Editor.Utils;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace Editor.Saved.Keybinds
{
    [Serializable]
    public class MapViewerKeybindsConfig
    {
        [XmlAnyElement("CameraMoveXmlComment")] public XmlComment CameraMoveXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Keybind used to move the camera.")]
        public Keybind CameraMove = MouseButton.Middle;

        [XmlAnyElement("SelectXmlComment")] public XmlComment SelectXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Keybind used to select an object.")]
        public Keybind Select = MouseButton.Left;

        [XmlAnyElement("DeselectXmlComment")] public XmlComment DeselectXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Keybind used to deselect all selected object.")]
        public Keybind Deselect = Keys.Escape;

        [XmlAnyElement("DeleteXmlComment")] public XmlComment DeleteXmlComment { get { return GetType().GetXmlComment(); } set { } }
        [XmlComment("Keybind used to delete the selected objects.")]
        public Keybind Delete = Keys.Delete;
    }
}
