using Editor;
using Editor.Logging;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MapEditor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.AddDefaultLoggingFiles();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow());

            Logger.ClearLoggingFiles();
        }
    }
}