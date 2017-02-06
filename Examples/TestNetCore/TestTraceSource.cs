using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HelloNetCore
{
    public class TestTraceSource
    {

        public void Run()
        {
            Console.WriteLine("TraceSource Example (.NET Core 1.0)");

            // There is no config support for TraceSource, so need to add listener manually
            var trace = new TraceSource("Example.ColoredConsole.Source", SourceLevels.All);
            var consoleListener = new TextWriterTraceListener(Console.Out);
            trace.Listeners.Add(consoleListener);

            trace.TraceEvent(TraceEventType.Information, 1001, "Net20 example started.");
            trace.TraceEvent(TraceEventType.Verbose, 0, "Net20 verbose.");
            trace.TraceEvent(TraceEventType.Warning, 4001, "Net20 warning.");
        }
    }
}
