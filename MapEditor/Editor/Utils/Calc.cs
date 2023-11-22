using Editor.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            float val1 = s * v;
            float val2 = val1 * (1f - Math.Abs(angle / 60f % 2f - 1f));
            float val3 = v - val1;
            if (angle < 60)
                return new Color(val3 + val1, val3 + val2, val3);
            if (angle < 120)
                return new Color(val3 + val2, val3 + val1, val3);
            if (angle < 180)
                return new Color(val3, val3 + val1, val3 + val2);
            if (angle < 240)
                return new Color(val3, val3 + val2, val3 + val1);
            return angle < 300 ? new Color(val3 + val2, val3, val3 + val1) : new Color(val3 + val1, val3, val3 + val2);
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
            // Regex: https://regex101.com/r/3rQ25V/1
            // Matches any uppercase letter that has a lowercase variant,
            // that is not the first character in the string,
            // and that is either preceded or followed by a lowercase letter that has an uppercase variant
            str = Regex.Replace(str, @"(?:(?<=\p{Ll})\p{Lu})|(?:\p{Lu}(?=\p{Ll}))(?<!^.)", m => " " + m.Value);
            return char.ToUpperInvariant(str[0]) + str[1..];
        }

        public static bool BetweenInterval(float val, float interval) => val % (interval * 2f) > interval;

        public static bool OnInterval(float val, float prevVal, float interval) => (int) (prevVal / interval) != (int) (val / interval);

        public static Vector2 Abs(Vector2 vec) => new(MathF.Abs(vec.X), MathF.Abs(vec.Y));

        public static List<T> DeepCloneList<T>(List<T> instance) where T : ICloneable => instance.Select(x => x.Clone()).ToList() as List<T>;

        public static bool DeepEqualsList<T>(List<T> list, List<T> otherList)
        {
            if (list == otherList)
                return true;

            if (list.Count != otherList.Count)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Equals(otherList[i]))
                    return false;
            }

            return true;
        }
    }
}
