using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Editor.Saved
{
    public struct Keybind : IXmlSerializable
    {
        private Keys? keyboard = null;
        private MouseButton? mouse = null;

        public bool IsKeyboard => keyboard.HasValue;
        public bool IsMouse => mouse.HasValue;

        public Keybind(Keys key) => keyboard = key;
        public Keybind(MouseButton button) => mouse = button;

        public static implicit operator Keys(Keybind keybind) => keybind.keyboard.Value;
        public static implicit operator MouseButton(Keybind keybind) => keybind.mouse.Value;

        public static implicit operator Keybind(Keys key) => new(key);
        public static implicit operator Keybind(MouseButton button) => new(button);

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            if (reader.AttributeCount != 1)
                throw new XmlException($"Wrong attribute count in XML representation of {nameof(Keybind)}");

            if (!reader.MoveToFirstAttribute())
                throw new XmlException($"An error occured while trying to read XML attribute value of type {nameof(Keybind)}");

            if (reader.Name == "keyboard")
                keyboard = Enum.Parse<Keys>(reader.Value);
            else
                mouse = Enum.Parse<MouseButton>(reader.Value);
        }

        public void WriteXml(XmlWriter writer)
        {
            if (IsKeyboard)
                writer.WriteAttributeString("keyboard", keyboard.ToString());
            else
                writer.WriteAttributeString("mouse", mouse.ToString());
        }
    }
}
