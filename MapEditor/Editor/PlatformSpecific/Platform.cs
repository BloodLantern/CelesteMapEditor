namespace Editor.PlatformSpecific;

public static class Platform
{
    public static bool ShouldUseDarkMode()
    {
#if WINDOWS
        return Windows.ShouldSystemUseDarkMode();
#else
        return true;
#endif
    }
}
