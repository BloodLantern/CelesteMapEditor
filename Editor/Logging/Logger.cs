using System;
using System.Collections.Generic;
using System.IO;

namespace Editor.Logging
{
    public static class Logger
    {
        private const string LogsDirectory = "logs";

        private static readonly List<StreamWriter> logFiles = new();

        public static void AddDefaultLoggingFiles()
        {
            AddLatestLoggingFile();
            AddDefaultLoggingFile();
        }

        public static void AddLatestLoggingFile()
        {
            string file = Path.Combine(LogsDirectory, "latest.log");

            Directory.CreateDirectory(LogsDirectory);
            logFiles.Add(File.CreateText(file));
        }

        public static void AddDefaultLoggingFile()
        {
            DateTime now = DateTime.Now;
            string file = Path.Combine(
                LogsDirectory,
                now.Year
                + "-" + (now.Month < 10 ? "0" + now.Month : now.Month)
                + "-" + (now.Day < 10 ? "0" + now.Day : now.Day)
                + ".log"
            );

            AddLoggingFile(file);
        }

        public static void AddLoggingFile(string filePath)
        {
            Directory.CreateDirectory(Directory.GetParent(filePath).ToString());

            int logCount = 0;
            if (File.Exists(filePath))
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    if (line == string.Empty)
                        logCount++;
                }
            }

            logFiles.Add(File.AppendText(filePath));
            Log("Starting logging #" + logCount + " to file: " + filePath);
        }

        public static void ClearLoggingFiles()
        {
            foreach (StreamWriter logFile in logFiles)
            {
                logFile.Write("\n");
                logFile.Close();
            }
            logFiles.Clear();
        }

        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            DateTime now = DateTime.Now;
            foreach (StreamWriter logFile in logFiles)
            {
                logFile.Write(
                    "[" + (now.Hour < 10 ? "0" + now.Hour : now.Hour)
                    + ":" + (now.Minute < 10 ? "0" + now.Minute : now.Minute)
                    + ":"+ (now.Second < 10 ? "0" + now.Second : now.Second)
                );
                switch (logLevel)
                {
                    case LogLevel.Info:
                        logFile.Write("] [INFO] ");
                        break;
                    case LogLevel.Warning:
                        logFile.Write("] [WARN] ");
                        break;
                    case LogLevel.Error:
                        logFile.Write("] [ERROR] ");
                        break;
                    case LogLevel.Fatal:
                        logFile.Write("] [FATAL] ");
                        break;
                }
                logFile.WriteLine(message);
                logFile.FlushAsync();
            }
        }
    }
}
