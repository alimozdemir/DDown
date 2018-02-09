using System.Collections.Generic;
using Newtonsoft.Json;

namespace DDown.Internal
{
    internal class Save
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public int Length { get; set; }
        public bool IsRangeSupported { get; set; }
        public List<Partition> Partitions { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}