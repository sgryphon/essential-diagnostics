using System;
using System.Diagnostics;

namespace HelloNet20
{
    class Program
    {
        static TraceSource trace = new TraceSource("Example.ColoredConsole.Source");

        static void Main(string[] args)
        {
            Console.WriteLine("Colored Console Example (.NET 2.0)");
            trace.TraceEvent(TraceEventType.Information, 1001, "Net20 example started.");
            trace.TraceEvent(TraceEventType.Verbose, 0, "Net20 verbose.");
            trace.TraceEvent(TraceEventType.Warning, 4001, "Net20 warning.");
            Console.ReadLine();
        }
    }
}
