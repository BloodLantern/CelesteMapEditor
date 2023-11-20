using Editor.Saved.Attributes;

namespace Editor.Saved
{
    public class GraphicsConfig
    {
        /// <summary>
        /// Refresh rate in milliseconds. Only effective if Vsync is off and value is not 0.
        /// </summary>
        [RequiresGraphicsReload]
        public int RefreshRate = 16;

        /// <summary>
        /// Whether to enable vertical synchronization. If enabled, this bypasses the refresh rate of MapViewerRefreshRate
        /// </summary>
        [RequiresGraphicsReload]
        public bool Vsync = false;
    }
}
