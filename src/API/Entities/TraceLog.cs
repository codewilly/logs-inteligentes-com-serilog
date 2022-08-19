namespace API.Entities
{
    public class TraceLog
    {
        public DateTime At { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }
    }
}
