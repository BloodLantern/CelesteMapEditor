using Editor.Saved;
using System;

namespace Editor.Extensions
{
    public static class ReloadTypeExt
    {
        public static string GetTooltip(this ReloadType type)
        {
            return type switch
            {
                ReloadType.Graphics => "Requires graphics to be reloaded",
                ReloadType.FullEditor => "Requires a full restart",
                _ => throw new ArgumentException($"Cannot get the tooltip message from this {nameof(ReloadType)}.", type.ToString())
            } + " to take effect";
        }
    }
}
