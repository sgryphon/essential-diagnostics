using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Essential.Diagnostics.Tests.Utility
{
    public class TestTraceListener : TraceListenerBase
    {
        internal List<TraceInfo> MethodCallInformation = new List<TraceInfo>();

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            // Note: References stored, so info could be mutable, 
            // however (unlike InMemoryTraceListener) this class is only for testing,
            // so should not be an issue.
            var traceInfo = new TraceInfo()
            {
                EventCache = eventCache,
                Source = source,
                EventType = eventType,
                Id = id,
                Message = message,
                RelatedActivityId = relatedActivityId,
                Data = data
            };
            MethodCallInformation.Add(traceInfo);
        }

        internal class TraceInfo
        {
            public TraceEventCache EventCache { get; set; }
            public string Source { get; set; }
            public TraceEventType EventType { get; set; }
            public int Id { get; set; }
            public string Message { get; set; }
            public Guid? RelatedActivityId { get; set; }
            public object[] Data { get; set; }
        }
    }
}
