namespace Editor.UI
{
    public interface ICloseable
    {
        bool WindowOpen { get; set; }

        string KeyboardShortcut { get; }
    }
}
