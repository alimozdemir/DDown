using System;
using System.Collections.Generic;
using DDown.Internal;

namespace DDown
{
    public class Status
    {
        public bool Done { get; set; }
        /// <summary>
        /// This value shows the download is continued or started from beginning.
        /// </summary>
        /// <returns></returns>
        public bool Continued { get; set; }
        /// <summary>
        /// Total length of the file.
        /// </summary>
        /// <returns></returns>
        public long Length { get; set; }
        /// <summary>
        /// Total count of the file. Length - 1
        /// </summary>
        /// <returns></returns>
        public long Count { get { return Length - 1; } }
        /// <summary>
        /// Dividing the files with partitions is supported or not.
        /// </summary>
        /// <returns></returns>
        public bool IsRangeSupported { get; set; }
        /// <summary>
        /// This is an internal collection that contains all partitions
        /// </summary>
        /// <returns></returns>
        internal List<Partition> Partitions { get; set; } = new List<Partition>();
        /// <summary>
        /// Total partition count
        /// </summary>
        public int PartitionCount => Partitions.Count;

        public override string ToString()
        {
            return $"Length:{Length}{Environment.NewLine}IsRangeSupported:{IsRangeSupported}";
        }
    }
}