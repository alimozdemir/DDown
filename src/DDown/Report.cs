namespace DDown
{
    public struct Report
    {
        public int PartitionId { get; set; }
        public int Percent { get; set; }
        public long Length { get; set; }
        public long Current { get; set; }
    }
}