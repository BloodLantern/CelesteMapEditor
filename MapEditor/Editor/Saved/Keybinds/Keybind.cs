using Editor.Utils;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Editor.Saved
{
    public struct Keybind : IXmlSerializable, ICustomConfigurationEditorDisplay
    {
        private Keys? keyboard = null;
        private MouseButton? mouse = null;

        public readonly bool IsKeyboard => keyboard.HasValue;
        public readonly bool IsMouse => mouse.HasValue;

        public Keybind(Keys key) => keyboard = key;
        public Keybind(MouseButton button) => mouse = button;

        public static implicit operator Keys(Keybind keybind) => keybind.keyboard.Value;
        public static implicit operator MouseButton(Keybind keybind) => keybind.mouse.Value;

        public static implicit operator Keybind(Keys key) => new(key);
        public static implicit operator Keybind(MouseButton button) => new(button);

        public readonly XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            if (reader.AttributeCount != 1)
                throw new XmlException($"Wrong attribute count in XML representation of {nameof(Keybind)}");

            if (!reader.MoveToFirstAttribute())
                throw new XmlException($"An error occured while trying to read XML attribute value of type {nameof(Keybind)}");

            if (reader.Name == string.Empty)
                return;

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

        public override string ToString()
        {
            return IsKeyboard ? "Keyboard " + keyboard.ToString() : "Mouse " + mouse.ToString();
        }

        public readonly void Render(FieldInfo field, object instance)
        {
            Keybind value = (Keybind) field.GetValue(instance);
            Keybind newValue = ImGuiUtils.KeybindPicker(field.Name, value);
            field.SetValue(instance, newValue);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is Keybind keybind &&
                   keyboard == keybind.keyboard &&
                   mouse == keybind.mouse;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(keyboard, mouse, IsKeyboard, IsMouse);
        }

        public static bool operator ==(Keybind left, Keybind right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Keybind left, Keybind right)
        {
            return !(left == right);
        }
    }
}
