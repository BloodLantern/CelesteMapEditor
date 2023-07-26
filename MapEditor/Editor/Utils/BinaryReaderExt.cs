using System.IO;

namespace Editor.Utils
{
    public static class BinaryReaderExt
    {
        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((ch = stream.ReadChar()) != char.MinValue)
                str += ch.ToString();
            return str;
        }
    }
}
