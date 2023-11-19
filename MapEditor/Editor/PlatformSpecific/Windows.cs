using System.Runtime.InteropServices;

namespace Editor.PlatformSpecific
{
    public class Windows
    {
#if WINDOWS
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();
#endif
    }
}
