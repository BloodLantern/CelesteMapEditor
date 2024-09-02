using Editor.Saved.Attributes;

namespace Editor.Saved
{
    public class GraphicsConfig : ConfigBase
    {
        /// <summary>
        /// Refresh rate in milliseconds. Only effective if Vsync is off and value is not 0.
        /// </summary>
        [RequiresReload(ReloadType.Graphics)]
        public readonly int RefreshRate = 16;

        /// <summary>
        /// Whether to enable vertical synchronization. If enabled, this bypasses the refresh rate of MapViewerRefreshRate
        /// </summary>
        [RequiresReload(ReloadType.Graphics)]
        public readonly bool Vsync = false;

        public override object Clone() => Clone<GraphicsConfig>();
    }
}
