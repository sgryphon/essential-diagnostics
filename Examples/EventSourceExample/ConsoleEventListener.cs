using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace EventSourceExample
{
    class ConsoleEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine("{0}, {1}, {2}, {3}/{4}/{5}/{6}, {7}",
                eventData.EventId,
                eventData.EventSource,
                eventData.Level,
                eventData.Keywords,
                eventData.Opcode,
                eventData.Task,
                eventData.Version,
                string.Format(eventData.Message, eventData.Payload.ToArray()));
        }
    }
}
