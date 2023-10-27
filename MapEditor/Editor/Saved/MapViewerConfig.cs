using System;
using Editor.Saved.Keybinds;
using Microsoft.Xna.Framework;

namespace Editor.Saved
{
    [Serializable]
    public class MapViewerConfig
    {
        public bool ShowHitboxes = false;

        public float ZoomFactor = 1.25f;

        public Color EntitySelectionBoundsColorMin = Color.DarkMagenta;

        public Color EntitySelectionBoundsColorMax = Color.Magenta;

        public MapViewerKeybindsConfig Keybinds = new();
    }
}
