using Editor.Utils;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Editor.Celeste
{
    public class EntityData
    {
        public int ID;
        public string Name;
        public LevelData Level;
        public Vector2 Position;
        public Vector2 Origin;
        public Size Size;
        public Vector2[] Nodes;
        public Dictionary<string, object> Values = new();

        public EntityData(string name, LevelData level, Vector2[] nodes)
        {
            Name = name;
            Level = level;
            Nodes = nodes;
        }

        public bool Has(string key) => Values.ContainsKey(key);

        public string Attr(string key, string defaultValue = "")
        {
            return Values.TryGetValue(key, out object obj) ? (string) obj : defaultValue;
        }

        public float Float(string key, float defaultValue = 0.0f)
        {
            if (Values.TryGetValue(key, out object obj))
            {
                if (obj is float num)
                    return num;
                if (float.TryParse(obj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    return result;
            }
            return defaultValue;
        }

        public bool Bool(string key, bool defaultValue = false)
        {
            if (Values.TryGetValue(key, out object obj))
            {
                if (obj is bool flag)
                    return flag;
                if (bool.TryParse(obj.ToString(), out bool result))
                    return result;
            }
            return defaultValue;
        }

        public int Int(string key, int defaultValue = 0)
        {
            if (Values.TryGetValue(key, out object obj))
            {
                if (obj is int num)
                    return num;
                if (int.TryParse(obj.ToString(), out int result))
                    return result;
            }
            return defaultValue;
        }

        public char Char(string key, char defaultValue = '\0')
        {
            return Values.TryGetValue(key, out object obj) && char.TryParse(obj.ToString(), out char result) ? result : defaultValue;
        }
    }
}
