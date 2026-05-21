using System;
using System.IO;

namespace AiSuite.Utils
{
    public static class Logger
    {
        private readonly static string LogFileName = "log.txt";
        private static string logFilePath;

        public static void Initialize(string logPath)
        {
            logFilePath = Path.Combine(logPath, LogFileName);
        }

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
            }

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";

            Console.WriteLine(line);
            File.AppendAllText(logFilePath, line + Environment.NewLine);
        }
    }
}