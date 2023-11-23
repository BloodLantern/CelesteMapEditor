namespace Editor.UI
{
    internal interface ICloseable
    {
        bool WindowOpen { get; set; }

        string KeyboardShortcut { get; set; }
    }
}
