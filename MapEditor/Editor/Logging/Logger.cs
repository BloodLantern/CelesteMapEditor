using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Editor.Logging
{
    public static class Logger
    {
        private class LogEntry
        {
            public string Message;
            public LogLevel Level;
            public DateTime Time;

            public LogEntry(string message, LogLevel level, DateTime time)
            {
                Message = message;
                Level = level;
                Time = time;
            }
        }

        private const string LogsDirectory = "logs";

        public static readonly List<string> Logs = new();
        private static readonly ConcurrentQueue<LogEntry> logsQueue = new();

        public static bool LoggedLastFrame { get; private set; }

        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
            => logsQueue.Enqueue(new LogEntry(message, logLevel, DateTime.Now));

        public static async void UpdateLogsAsync()
        {
            LoggedLastFrame = false;

            if (logsQueue.IsEmpty)
                return;

            LoggedLastFrame = true;

            await Task.Run(updateLogs);
        }

        private static readonly Action updateLogs = () =>
        {
            // Empty the queue
            while (!logsQueue.IsEmpty)
            {
                if (!logsQueue.TryDequeue(out LogEntry log))
                    // Avoid deadlocks by breaking out of the loop
                    break;

                string toWrite = "[" + (log.Time.Hour < 10 ? "0" + log.Time.Hour : log.Time.Hour)
                    + ":" + (log.Time.Minute < 10 ? "0" + log.Time.Minute : log.Time.Minute)
                    + ":" + (log.Time.Second < 10 ? "0" + log.Time.Second : log.Time.Second);

                switch (log.Level)
                {
                    case LogLevel.Debug:
                        toWrite += "] [DEBUG] ";
                        break;
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

                Logs.Add(toWrite + log.Message);
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
            Log("Testing DEBUG log.", LogLevel.Debug);
            Log("Testing INFO log.", LogLevel.Info);
            Log("Testing WARNING log.", LogLevel.Warning);
            Log("Testing ERROR log.", LogLevel.Error);
            Log("Testing FATAL log.", LogLevel.Fatal);
        }
    }
}
