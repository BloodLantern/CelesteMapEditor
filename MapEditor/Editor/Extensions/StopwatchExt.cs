using System.Diagnostics;

namespace Editor.Extensions
{
    public static class StopwatchExt
    {
        public static float GetElapsedSeconds(this Stopwatch self) => self.ElapsedMilliseconds / 1000f;
    }
}
