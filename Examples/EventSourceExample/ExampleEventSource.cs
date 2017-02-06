using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceExample
{
    [EventSource(Guid = "{67CB0356-4841-4AC7-B192-1D0FBBE089C8}")]
    class ExampleEventSource : EventSource
    {
        public static ExampleEventSource Log => new ExampleEventSource();

        [Event(1, Level = EventLevel.Informational, Message = "Event 1 message: {0}")]
        public void ExampleEvent1(string text)
        {
            this.WriteEvent(1, text);
        }

        [Event(2, Level = EventLevel.Error, Message = "Event 2 message: {0}, exception: {1}")]
        public void ExampleEvent2(string comment, string exceptionDetails)
        {
            this.WriteEvent(2, comment, exceptionDetails);
        }
    }
}
