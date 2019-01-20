using System;

namespace DDown.CLI
{
    public class TimeLog
    {
        public static void WriteLine(string text)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] {text}");
        }
    }
    public static class Readable
    {
        private static string[] Sizes = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        // solution: https://stackoverflow.com/a/4975942/2107255
        public static string AsReadable(this int length)
        {
            if (length == 0)
                return "0" + Sizes[0];
            long bytes = Math.Abs(length);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(length) * num).ToString() + Sizes[place];
        }
        public static string AsReadable(this long length)
        {
            if (length == 0)
                return "0" + Sizes[0];
            long bytes = Math.Abs(length);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(length) * num).ToString() + Sizes[place];
        }
    }
}