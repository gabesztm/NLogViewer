using System;

namespace NLogViewer
{
    class LogEntry
    {
        public DateTime TimeStamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
