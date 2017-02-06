using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace EventSourceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TraceSource Example (.NET 4.5 required)");

            using (var consoleEventListener = new ConsoleEventListener())
            {
                consoleEventListener.EnableEvents(ExampleEventSource.Log, EventLevel.Verbose);
                ExampleEventSource.Log.ExampleEvent1("Test event source informational");
                try
                {
                    throw new NotSupportedException("Test exception");
                }
                catch (Exception ex)
                {
                    ExampleEventSource.Log.WriteException("Exception occurred", ex);
                }
            }
            Console.WriteLine("Done");
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }
    }
}
