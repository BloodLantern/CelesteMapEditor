using Editor.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Editor
{
    public static class Calc
    {
        public static Random Random = new();
        public const float DefaultFrameDuration = 1f / 60f;
        private const string Hex = "0123456789ABCDEF";

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static XmlDocument LoadContentXML(string filename)
        {
            XmlDocument xmlDocument = new();
            using (StreamReader inStream = new(Path.Combine(Session.Current.CelesteContentDirectory, filename)))
                xmlDocument.Load(inStream);
            return xmlDocument;
        }

        public static T GiveMe<T>(int index, T a, T b)
        {
            if (index == 0)
                return a;
            if (index != 1)
                throw new Exception("Index was out of range");
            return b;
        }

        public static T GiveMe<T>(int index, T a, T b, T c)
        {
            return index switch
            {
                0 => a,
                1 => b,
                2 => c,
                _ => throw new Exception("Index was out of range!"),
            };
        }

        public static T GiveMe<T>(int index, T a, T b, T c, T d)
        {
            return index switch
            {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                _ => throw new Exception("Index was out of range!"),
            };
        }

        public static T GiveMe<T>(int index, T a, T b, T c, T d, T e)
        {
            return index switch
            {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                4 => e,
                _ => throw new Exception("Index was out of range!"),
            };
        }

        public static T GiveMe<T>(int index, T a, T b, T c, T d, T e, T f)
        {
            return index switch
            {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                4 => e,
                5 => f,
                _ => throw new Exception("Index was out of range!"),
            };
        }

        public static byte HexToByte(char c) => (byte) Hex.IndexOf(char.ToUpper(c));

        public static Color HexToColor(string hex)
        {
            int prefixLength = 0;
            if (hex.Length >= 1 && hex[0] == '#')
                prefixLength = 1;

            if (hex.Length - prefixLength >= 6)
            {
                float r = (HexToByte(hex[prefixLength]) * 16 + HexToByte(hex[prefixLength + 1])) / (float) byte.MaxValue;
                float g = (HexToByte(hex[prefixLength + 2]) * 16 + HexToByte(hex[prefixLength + 3])) / (float) byte.MaxValue;
                float b = (HexToByte(hex[prefixLength + 4]) * 16 + HexToByte(hex[prefixLength + 5])) / (float) byte.MaxValue;

                return new Color(r, g, b);
            }

            return int.TryParse(hex[prefixLength..], out int result) ? HexToColor(result) : Color.White;
        }

        public static Color HexToColor(int hex) => new()
        {
            A = byte.MaxValue,
            R = (byte) (hex >> 16),
            G = (byte) (hex >> 8),
            B = (byte) hex
        };

        public static Color HsvToColor(float hue, float s, float v)
        {
            int angle = (int) (hue * 360f);
            float num2 = s * v;
            float num3 = num2 * (1f - Math.Abs(angle / 60f % 2f - 1f));
            float num4 = v - num2;
            if (angle < 60)
                return new Color(num4 + num2, num4 + num3, num4);
            if (angle < 120)
                return new Color(num4 + num3, num4 + num2, num4);
            if (angle < 180)
                return new Color(num4, num4 + num2, num4 + num3);
            if (angle < 240)
                return new Color(num4, num4 + num3, num4 + num2);
            return angle < 300 ? new Color(num4 + num3, num4, num4 + num2) : new Color(num4 + num2, num4, num4 + num3);
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
            => new(MathHelper.Lerp(a.X, b.X, t), MathHelper.Lerp(a.Y, b.Y, t));

        public static float EaseLerp(float a, float b, float t, float duration, Ease.Easer ease)
            => MathHelper.Lerp(a, b, ease(Math.Min(t, duration) / duration));

        public static float YoYo(float value) => value <= 0.5f ? value * 2f : 1f - (value - 0.5f) * 2f;

        public static Color GetHue(Vector2 position, float time)
            => HsvToColor(0.4f + YoYo((position.Length() + time * 50f) % 280f / 280f) * 0.4f, 0.4f, 0.9f);

        public static bool EqualsWithTolerance(this float value, float other, float tolerance = 1E-05f) => Math.Abs(value - other) <= tolerance;

        public static string HumanizeString(string str)
        {
            str = Regex.Replace(str, @"\p{Lu}", m => " " + m.Value.ToLowerInvariant());
            return char.ToUpperInvariant(str[0]) + str[1..];
        }

        /// <summary>
        /// Loads a monospace font from a font texture.
        /// </summary>
        /// <param name="texture">The font texture.</param>
        /// <param name="charSize">The size of a single character.</param>
        /// <param name="characters">The list of characters in order in the texture.</param>
        /// <returns>A dictionary containing a texture for each character in the list.</returns>
        public static Dictionary<char, Texture> LoadFontFromTexture(Texture texture, Point charSize, List<char> characters)
        {
            Dictionary<char, Texture> result = new();

            for (int y = 0; y < texture.Height; y += charSize.Y + 1)
            {
                for (int x = 0; x < texture.Width; x += charSize.X + 1)
                {
                    if (result.Count >= characters.Count)
                        return result;
                    result.Add(characters[result.Count], new Texture(texture, new Rectangle(x, y, charSize.X, charSize.Y), Point.Zero));
                }
            }

            return result;
        }
    }
}
