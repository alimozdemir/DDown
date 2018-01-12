using System;

namespace DDown
{
    public class Options
    {
        /// <summary>
        /// Default as 1
        /// </summary>
        /// <returns></returns>
        public int ConnectionCount { get; set; } = 1;
        /// <summary>
        /// Default: Current Path of Process (Environment.CurrentDirectory)
        /// </summary>
        /// <returns></returns>
        public string OutputFolder { get; set; } = Environment.CurrentDirectory;
        public string Name { get; set; } = default(string);
        public bool Override { get; set; } = true;
    }
}