using Editor.Saved;
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

        public static List<string> LogEntries { get; private set; } = new();
        private static readonly ConcurrentQueue<LogEntry> LogsQueue = new();

        public static bool LoggedLastFrame { get; private set; }

        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
            => LogsQueue.Enqueue(new(message, logLevel, DateTime.Now));

        public static async void UpdateLogsAsync()
        {
            LoggedLastFrame = false;

            if (LogsQueue.IsEmpty)
                return;

            LoggedLastFrame = true;

            await Task.Run(UpdateLogs);
        }

        private static readonly Action UpdateLogs = () =>
        {
            // Empty the queue
            while (!LogsQueue.IsEmpty)
            {
                if (!LogsQueue.TryDequeue(out LogEntry log))
                    // Avoid deadlocks by breaking out of the loop
                    break;

                string toWrite = "[" + (log.Time.Hour < 10 ? "0" + log.Time.Hour : log.Time.Hour)
                    + ":" + (log.Time.Minute < 10 ? "0" + log.Time.Minute : log.Time.Minute)
                    + ":" + (log.Time.Second < 10 ? "0" + log.Time.Second : log.Time.Second)
                    + "." + (log.Time.Millisecond < 10 ? "00" + log.Time.Millisecond : (log.Time.Millisecond < 100 ? "0" + log.Time.Millisecond : log.Time.Millisecond));

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

                LogEntries.Add(toWrite + log.Message);
            }
        };

        public static void EndLogging(Config config)
        {
            if (!config.EnableLogging)
                return;

            UpdateLogs();
            LogEntries.Add(string.Empty); // Add an empty line at the end of the logs

            // Remove all the logs below the minimum log level
            LogEntries = LogEntries.FindAll(log => (byte) GetLevel(log) >= (byte) config.LogLevel);

            if (LogEntries.Count == 0)
                return;

            AddLogsToLatestFile();

            if (LogEntries.Count > 0)
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
                File.WriteAllLines(filePath, LogEntries);
            else
                File.AppendAllLines(filePath, LogEntries);
        }

        public static void Test()
        {
            Log("Testing DEBUG log.", LogLevel.Debug);
            Log("Testing INFO log.", LogLevel.Info);
            Log("Testing WARNING log.", LogLevel.Warning);
            Log("Testing ERROR log.", LogLevel.Error);
            Log("Testing FATAL log.", LogLevel.Fatal);
        }

        public static LogLevel GetLevel(string log)
        {
            if (log.Contains("[DEBUG]"))
                return LogLevel.Debug;
            else if (log.Contains("[INFO]"))
                return LogLevel.Info;
            else if (log.Contains("[WARN]"))
                return LogLevel.Warning;
            else if (log.Contains("[ERROR]"))
                return LogLevel.Error;
            return LogLevel.Fatal;
        }
    }
}
