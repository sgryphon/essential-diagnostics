using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Essential.Diagnostics;

namespace ScopeExample
{
    public class Program
    {
        static TraceSource trace = new TraceSource("ScopeExample");

        public static void Main(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "Scope example started.");
            using (var logicalOperationScope = new LogicalOperationScope(string.Format("Transaction={0}", 1)))
            {
                using (var activityScope = new ActivityScope(trace, 0, 2001, 0, 3001))
                {
                    trace.TraceEvent(TraceEventType.Warning, 4001, "Example warning.");
                }
            }
            trace.TraceEvent(TraceEventType.Information, 8001, "Scope example finished.");
            Console.ReadLine();
        }
    }
}
