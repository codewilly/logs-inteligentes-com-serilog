using Serilog.Events;

namespace API.Models
{
    public class TraceContainer
    {
        public TraceContainer()
        {
            LogEvents = new();
        }

        public string TraceId { get; set; }

        public List<LogEvent> LogEvents { get; set; }
    }
}
