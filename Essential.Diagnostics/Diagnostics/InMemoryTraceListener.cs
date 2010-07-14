using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Essential.Diagnostics
{
    public class InMemoryTraceListener : TraceListenerBase
    {
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            throw new NotImplementedException();
        }
    }
}
