using System;
using System.IO;

namespace DDown.Internal
{
    internal static class FileHelper
    {
        public const string PartitionFolder = ".partitions",
                            SavedFolder = ".saved";

        public static void EnsureFoldersCreated()
        {
            if (!Directory.Exists(SavedFolder))
                Directory.CreateDirectory(SavedFolder);

            if (!Directory.Exists(PartitionFolder))
                Directory.CreateDirectory(PartitionFolder);
            
        }
        public static string GetPartitionPath(string file)
        {
            return Path.Combine(PartitionFolder, file);
        }
        public static string GetSavePath(string file)
        {
            return Path.Combine(SavedFolder, file);
        }
        public static string[] GetAllFilesInSavedFolder()
        {
            return Directory.GetFiles(SavedFolder, "*.json");
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