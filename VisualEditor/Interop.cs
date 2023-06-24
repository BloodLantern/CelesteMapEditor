using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualEditor
{
    internal static class Interop
    {
        const string DllPath = "../../../../x64/Debug/Editor.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Logger_OpenDefaultFile();

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Logger_LogInfo(string message);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Map_load(string filepath);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
    }
}
