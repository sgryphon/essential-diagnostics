using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;

namespace HelloNetCore
{
    public class TestEventSource
    {
        public void Run()
        {
            Console.WriteLine("EventSource Example (.NET Core 1.0)");

            var customEventSource = new CustomEventSource();
            customEventSource.Information("Test .NET Core 1.0");
        }

        [EventSource(Guid = "{8D123D8D-BABC-4A9D-A506-512510EEC510}", Name = "CustomEventSource")]
        public class CustomEventSource : EventSource
        {
            [Event(1,
                Level = EventLevel.Informational,
                //Message = "Information: {0}",
                Keywords = EventKeywords.None,
                Opcode = EventOpcode.Info,
                Task = EventTask.None)]
            public void Information(string message)
            {
                this.WriteEvent(2, message);
            }
        }
    }
}
