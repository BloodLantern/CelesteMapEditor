using Editor.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Editor.Celeste
{
    public static class BinaryPacker
    {
        public class Element
        {
            public string Package;
            public string Name;
            public Dictionary<string, object> Attributes;
            public List<Element> Children;

            public bool HasAttr(string name) => Attributes != null && Attributes.ContainsKey(name);

            public string Attr(string name, string defaultValue = "")
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj.ToString();
            }

            public bool AttrBool(string name, bool defaultValue = false)
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj is bool flag ? flag : bool.Parse(obj.ToString());
            }

            public float AttrFloat(string name, float defaultValue = 0.0f)
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj is float num ? num : float.Parse(obj.ToString(), CultureInfo.InvariantCulture);
            }
        }

        public static readonly HashSet<string> IgnoreAttributes = new() {
            "_eid"
        };
        private static string[] stringLookup;

        public static Element FromBinary(string filename)
        {
            Element element;
            using (FileStream input = File.OpenRead(filename))
            {
                BinaryReader reader = new(input);
                reader.ReadString();
                string mapName = reader.ReadString();
                short length = reader.ReadInt16();
                stringLookup = new string[length];
                for (int i = 0; i < length; ++i)
                    stringLookup[i] = reader.ReadString();
                element = ReadElement(reader);
                element.Package = mapName;
            }
            return element;
        }

        private static Element ReadElement(BinaryReader reader)
        {
            Element element = new()
            {
                Name = stringLookup[reader.ReadInt16()]
            };

            byte valueCount = reader.ReadByte();
            if (valueCount > 0)
                element.Attributes = new Dictionary<string, object>();

            for (int i = 0; i < valueCount; ++i)
            {
                string key = stringLookup[reader.ReadInt16()];
                byte value = reader.ReadByte();
                object obj = null;
                switch (value)
                {
                    case 0:
                        obj = reader.ReadBoolean();
                        break;
                    case 1:
                        obj = Convert.ToInt32(reader.ReadByte());
                        break;
                    case 2:
                        obj = Convert.ToInt32(reader.ReadInt16());
                        break;
                    case 3:
                        obj = reader.ReadInt32();
                        break;
                    case 4:
                        obj = reader.ReadSingle();
                        break;
                    case 5:
                        obj = stringLookup[reader.ReadInt16()];
                        break;
                    case 6:
                        obj = reader.ReadString();
                        break;
                    case 7:
                        short count = reader.ReadInt16();
                        obj = RunLengthEncoding.Decode(reader.ReadBytes(count));
                        break;
                }
                element.Attributes.Add(key, obj);
            }

            short childCount = reader.ReadInt16();
            if (childCount > 0)
                element.Children = new List<Element>();

            for (int i = 0; i < childCount; ++i)
                element.Children.Add(ReadElement(reader));

            return element;
        }
    }
}