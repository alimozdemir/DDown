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
        private static string[] Sizes = new string[] { "B", "KB", "MB", "GB", "TB" };
        public static string AsReadable(this int length)
        {
            int index = 0;

            while (length >= 1024 && index < Sizes.Length - 1)
            {
                index++;
                length /= 1024;
            }

            return $"{length} {Sizes[index]}";
        }
        public static string AsReadable(this long length)
        {
            int index = 0;

            while (length >= 1024 && index < Sizes.Length - 1)
            {
                index++;
                length /= 1024;
            }

            return $"{length} {Sizes[index]}";
        }
    }
}