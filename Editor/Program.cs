using Editor.Logging;
using System;
using System.Windows.Forms;

namespace Editor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.AddDefaultLoggingFiles();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            Logger.ClearLoggingFiles();
        }
    }
}
