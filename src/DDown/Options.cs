using System;

namespace DDown
{
    public class Options
    {
        /// <summary>
        /// Default as Processor Count
        /// </summary>
        /// <returns></returns>
        public int PartitionCount { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// Default: Current Path of Process (Environment.CurrentDirectory)
        /// </summary>
        /// <returns></returns>
        public string OutputFolder { get; set; } = Environment.CurrentDirectory;
        public string Name { get; set; } = default(string);
        public bool Override { get; set; } = true;
    }
}