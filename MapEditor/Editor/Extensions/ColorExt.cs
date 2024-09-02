using Microsoft.Xna.Framework;
using System;

namespace Editor.Extensions
{
    public static class ColorExt
    {
        public static Color Inverse(this Color color)
        {
            int r = 255 - color.R;
            int g = 255 - color.G;
            int b = 255 - color.B;

            return new(r, g, b, color.A);
        }

        public static void ToHsv(this Color color, out float hue, out float saturation, out float value)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            hue = 0f;
            if (delta != 0)
            {
                if (max == r)
                    hue = (g - b) / delta;
                else if (max == g)
                    hue = 2f + (b - r) / delta;
                else if (max == b)
                    hue = 4f + (r - g) / delta;

                hue *= 60f;
                if (hue < 0)
                    hue += 360f;
            }

            saturation = max == 0 ? 0 : delta / max;
            value = max;
        }
    }
}
