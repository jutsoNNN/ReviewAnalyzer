using System;

namespace ReviewAnalyzer.Utilities
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
            Console.ResetColor();
        }

        public static void Error(string message, Exception? ex = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
            if (ex != null) Console.WriteLine($"Exception: {ex.Message}\n{ex.StackTrace}");
            Console.ResetColor();
        }

        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss} - {message}");
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
            Console.ResetColor();
        }
    }
}