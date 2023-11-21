using System;

namespace Editor.Saved.Keybinds
{
    [Serializable]
    public class KeybindsConfig : ConfigBase
    {
        /// <summary>
        /// MapViewer keybinds.
        /// </summary>
        public MapViewerKeybindsConfig MapViewer = new();

        public override object Clone() => Clone<KeybindsConfig>();
    }
}
