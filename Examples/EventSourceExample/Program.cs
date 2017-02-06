using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TraceSource Example (.NET 4.5 required)");
            ExampleEventSource.Log.ExampleEvent1("Test event source informational");
            try
            {
                throw new NotSupportedException("Test exception");
            }
            catch (Exception ex)
            {
                ExampleEventSource.Log.ExampleEvent2("Exception occurred", ex.ToString());
            }
            Console.WriteLine("Done");
        }
    }
}
