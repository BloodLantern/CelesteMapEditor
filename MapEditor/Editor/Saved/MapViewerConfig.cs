using System;
using Editor.Saved.Keybinds;
using Microsoft.Xna.Framework;

namespace Editor.Saved
{
    [Serializable]
    public class MapViewerConfig
    {
        /// <summary>
        /// Whether to show entities, solids and player spawns hitboxes.
        /// </summary>
        public bool ShowHitboxes = false;

        /// <summary>
        /// Factor by which the camera zooms in and out.
        /// </summary>
        public float ZoomFactor = 1.25f;

        /// <summary>
        /// Minimum color for the selection rectangle around an entity.
        /// </summary>
        public Color EntitySelectionBoundsColorMin = Color.DarkMagenta;

        /// <summary>
        /// Maximum color for the selection rectangle around an entity.
        /// </summary>
        public Color EntitySelectionBoundsColorMax = Color.Magenta;

        /// <summary>
        /// MapViewer keybinds.
        /// </summary>
        public MapViewerKeybindsConfig Keybinds = new();
    }
}
