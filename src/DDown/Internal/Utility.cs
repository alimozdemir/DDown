using System;
using System.IO;

namespace DDown.Internal
{
    internal static class Utility
    {
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