using System;
using System.Collections.Generic;
using DDown.Internal;

namespace DDown
{
    public class Status
    {
        public long Length { get; set; }
        public long Count { get { return Length - 1; } }
        public bool IsRangeSupported { get; set; }
        internal List<Partition> Partitions { get; set; } = new List<Partition>();
        public int PartitionCount => Partitions.Count;

        public override string ToString()
        {
            return $"Length:{Length}{Environment.NewLine}IsRangeSupported:{IsRangeSupported}";
        }
    }
}