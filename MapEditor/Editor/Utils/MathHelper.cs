using System;

namespace Editor.Utils
{
    public static class MathHelper
    {
        public static float Sqrt(float x)
        {
            return (float) Math.Sqrt(x);
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
