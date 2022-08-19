using System.Collections.Generic;

namespace API.Entities
{
    public class Trace
    {
        public Trace()
        {
            Logs = new List<TraceLog>();
        }

        public string Id { get; set; }

        public ICollection<TraceLog> Logs { get; set; }
    }
}
