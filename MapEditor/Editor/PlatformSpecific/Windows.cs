using System.Runtime.InteropServices;

namespace Editor.PlatformSpecific;

public static class Windows
{
#if WINDOWS
    [LibraryImport("UXTheme.dll", EntryPoint = "#138", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShouldSystemUseDarkMode();
#endif
}
