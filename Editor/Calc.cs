using Microsoft.Xna.Framework;

namespace Editor
{
    public static class Calc
    {
        private static readonly string Hex = "0123456789ABCDEF";

        public static Color HexToColor(string hexColor)
        {
            int hashtagOffset = 0;
            if (hexColor[0] == '#')
                hashtagOffset = 1;

            string rStr = hexColor.Substring(hashtagOffset, 2);
            string gStr = hexColor.Substring(hashtagOffset + 2, 2);
            string bStr = hexColor.Substring(hashtagOffset + 4, 2);

            byte r = (byte) (Hex.IndexOf(rStr[0]) * 0x10 + Hex.IndexOf(rStr[1]));
            byte g = (byte) (Hex.IndexOf(gStr[0]) * 0x10 + Hex.IndexOf(gStr[1]));
            byte b = (byte) (Hex.IndexOf(bStr[0]) * 0x10 + Hex.IndexOf(bStr[1]));

            byte a = 0xFF;
            if (hexColor.Length > hashtagOffset + 6)
            {
                string aStr = hexColor.Length > hashtagOffset + 6 ? hexColor.Substring(hashtagOffset + 6, 2) : "FF";
                a = (byte) (Hex.IndexOf(aStr[0]) * 0x10 + Hex.IndexOf(aStr[1]));
            }

            return new Color(r, g, b, a);
        }
    }
}
