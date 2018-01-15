using System.Collections.Generic;
using Newtonsoft.Json;

namespace DDown.Internal
{
    internal class Save
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public List<Partition> Partitions { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}