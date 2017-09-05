using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Abstractions;
using Essential.Diagnostics;

namespace AbstractionDependency
{
    public class ApplicationTest2
    {
        public void TestRun()
        {
            var traceSource = new TraceSource("AbstractionDependency");
            var listener = (InMemoryTraceListener)traceSource.Listeners.Cast<TraceListener>().First(l => l is InMemoryTraceListener);
            listener.Clear();
            var applicationToTest = new Application();

            applicationToTest.Run();

            Console.WriteLine("Expected events: 1001, 8001");
            var events = listener.GetEvents();
            Console.WriteLine("Actual events: {0}, {1}", events[0].Id, events[1].Id);
        }

    }
}
