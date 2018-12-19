using System;
using System.IO;

namespace DDown.Internal
{
    internal static class FileHelper
    {
        public const string MainFolder = "ddown";
        public const string PartitionFolder = ".partitions",
                            SavedFolder = ".saved";
        public static readonly string SavedPath, PartitionPath;

        static FileHelper () 
        {
            var mainFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                MainFolder);

            SavedPath = Path.Combine(mainFolder, SavedFolder);
            PartitionPath = Path.Combine(mainFolder, PartitionFolder);

            if (!Directory.Exists(SavedPath))
                Directory.CreateDirectory(SavedPath);

            if (!Directory.Exists(PartitionPath))
                Directory.CreateDirectory(PartitionPath);
        }

        public static string GetPartitionPath(string file)
        {
            return Path.Combine(PartitionPath, file);
        }
        public static string GetSavePath(string file)
        {
            return Path.Combine(SavedPath, file);
        }
        public static string[] GetAllFilesInSavedFolder()
        {
            return Directory.GetFiles(SavedPath, "*.json");
        }
        public static string GetFileName(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Url can not be null", nameof(url));
            }

            return GetFileName(new Uri(url));
        }

        public static string GetFileName(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return Path.GetFileName(uri.LocalPath);
        }
    }
}