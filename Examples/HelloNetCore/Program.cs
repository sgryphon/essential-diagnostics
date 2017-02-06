using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HelloNetCore
{
    public class Program
    {
        static TraceSource trace = new TraceSource("Example.ColoredConsole.Source");

        public static void Main(string[] args)
        {
            Console.WriteLine("Colored Console Example (.NET Core 1.0)");
            trace.TraceEvent(TraceEventType.Information, 1001, "Net20 example started.");
            trace.TraceEvent(TraceEventType.Verbose, 0, "Net20 verbose.");
            trace.TraceEvent(TraceEventType.Warning, 4001, "Net20 warning.");
            Console.ReadLine();
        }
    }
}
