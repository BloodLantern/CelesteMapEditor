using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Editor.Logging
{
    public static class Logger
    {
        private const string LogsDirectory = "logs";

        public static readonly List<string> Logs = new();
        private static readonly ConcurrentQueue<string> logsQueue = new();

        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            DateTime now = DateTime.Now;
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

            logsQueue.Enqueue(toWrite + message);
        }

        public static async void UpdateLogsAsync()
        {
            if (logsQueue.IsEmpty)
                return;

            await Task.Run(updateLogs);
        }

        private static readonly Action updateLogs = () =>
        {
            // Empty the queue
            while (!logsQueue.IsEmpty)
            {
                if (!logsQueue.TryDequeue(out string log))
                    // Avoid deadlocks by breaking out of the loop
                    break;

                Logs.Add(log);
            }
        };

        public static void EndLogging(Session session)
        {
            if (!session.Config.EnableLogging)
                return;

            updateLogs();
            AddLogsToLatestFile();
            AddLogsToDefaultFile();
        }

        private static void AddLogsToLatestFile()
        {
            string latestFile = Path.Combine(LogsDirectory, "latest.log");

            Directory.CreateDirectory(LogsDirectory);

            AddLogsToFile(latestFile, true);
        }

        private static void AddLogsToDefaultFile()
        {
            DateTime now = DateTime.Now;
            string defaultFile = Path.Combine(
                LogsDirectory,
                now.Year
                + "-" + (now.Month < 10 ? "0" + now.Month : now.Month)
                + "-" + (now.Day < 10 ? "0" + now.Day : now.Day)
                + ".log"
            );

            AddLogsToFile(defaultFile, false);
        }

        private static void AddLogsToFile(string filePath, bool overwrite)
        {
            Directory.CreateDirectory(Directory.GetParent(filePath).ToString());

            if (overwrite)
                File.WriteAllLines(filePath, Logs);
            else
                File.AppendAllLines(filePath, Logs);
        }

        public static void Test()
        {
            Log("Testing INFO log.", LogLevel.Info);
            Log("Testing WARNING log.", LogLevel.Warning);
            Log("Testing ERROR log.", LogLevel.Error);
            Log("Testing FATAL log.", LogLevel.Fatal);
        }
    }
}
