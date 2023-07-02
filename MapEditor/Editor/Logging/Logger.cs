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
            //logFiles.Add(Console.Out as StreamWriter);
        }

        public static void AddLatestLoggingFile()
        {
            string file = Path.Combine(LogsDirectory, "latest.log");

            Directory.CreateDirectory(LogsDirectory);

            StreamWriter stream = File.CreateText(file);
            stream.AutoFlush = true;
            logFiles.Add(stream);
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

            StreamWriter stream = File.AppendText(filePath);
            stream.AutoFlush = true;
            logFiles.Add(stream);
            Log($"Starting logging #{logCount} to file: {filePath}");
        }

        public static void ClearLoggingFiles()
        {
            foreach (StreamWriter logFile in logFiles)
            {
                if (logFile == null)
                    continue;

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
                string toWrite = "[" + (now.Hour < 10 ? "0" + now.Hour : now.Hour)
                    + ":" + (now.Minute < 10 ? "0" + now.Minute : now.Minute)
                    + ":" + (now.Second < 10 ? "0" + now.Second : now.Second);

                switch (logLevel)
                {
                    case LogLevel.Info:
                        toWrite += "] [INFO] ";
                        break;
                    case LogLevel.Warning:
                        toWrite += "] [WARN] ";
                        break;
                    case LogLevel.Error:
                        toWrite += "] [ERROR] ";
                        break;
                    case LogLevel.Fatal:
                        toWrite += "] [FATAL] ";
                        break;
                }

                logFile.WriteLine(toWrite + message);
            }
        }
    }
}
