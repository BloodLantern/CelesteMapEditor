namespace Editor.UI
{
    internal interface ICloseable
    {
        public bool WindowOpen { get; set; }

        public string KeyboardShortcut { get; set; }
    }
}
