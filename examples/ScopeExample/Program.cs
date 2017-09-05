using Essential.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;

namespace ScopeExample
{
    public class Program
    {
        static TraceSource trace = new TraceSource("ScopeExample");

        public static void Main(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "Scope example started.");
            Thread.Sleep(500);
            using (var logicalOperationScope = new LogicalOperationScope(string.Format("Transaction:{0}", 1)))
            {
                Thread.Sleep(500);
                using (var activityScope = new ActivityScope(trace, 0, 2001, 0, 3001, 
                    "Example Transfer In", "Example Start", "Example Transfer Out", "Example Stop", 
                    "Example Activity"))
                {
                    Thread.Sleep(500);
                    trace.TraceEvent(TraceEventType.Warning, 4001, "Example warning.");
                    Thread.Sleep(500);
                }
                Thread.Sleep(500);
            }
            Thread.Sleep(500);
            trace.TraceEvent(TraceEventType.Information, 8001, "Scope example finished.");
            Console.ReadLine();
        }
    }
}
