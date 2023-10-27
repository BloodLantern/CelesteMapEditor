using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;

namespace Editor.Saved
{
    [Serializable]
    public class Keybind
    {
        private Keys? keyboard;
        private MouseButton? mouse;

        public bool IsKeyboard => keyboard.HasValue;
        public bool IsMouse => mouse.HasValue;

        public Keybind() {}
        public Keybind(Keys key) => keyboard = key;
        public Keybind(MouseButton button) => mouse = button;

        public static implicit operator Keys(Keybind keybind) => keybind.keyboard.Value;
        public static implicit operator MouseButton(Keybind keybind) => keybind.mouse.Value;

        public static implicit operator Keybind(Keys key) => new(key);
        public static implicit operator Keybind(MouseButton button) => new(button);
    }
}
