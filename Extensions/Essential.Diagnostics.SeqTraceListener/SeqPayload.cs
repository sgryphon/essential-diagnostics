using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Essential.Diagnostics
{
    class SeqPayload
    {
        public SeqPayload()
        {
            Properties = new Dictionary<string, object>();
        }

        public int EventId { get; set; }

        public TraceEventType EventType { get; set; }

        public string MessageTemplate { get; set; }

        public Dictionary<string, object> Properties { get; set; }

        public DateTimeOffset EventTime { get; set; }

        public Guid ActivityId { get; set; }

        public Guid? RelatedActivityId { get; set; }

        public string Source { get; set; }

        public object[] Data { get; set; }
    }
}
