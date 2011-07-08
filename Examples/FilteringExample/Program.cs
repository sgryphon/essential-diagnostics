using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

namespace FilteringExample
{
    public class Program
    {
        static TraceSource trace = new TraceSource("FilteringExample");

        public static void Main(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "Filtering example started.");
            trace.TraceEvent(TraceEventType.Verbose, 0, "About to start activity.");
            trace.TraceEvent(TraceEventType.Start, 2001, "Activity started.");
            var originalPrincipal = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("User1"), new string[0]);
                trace.TraceEvent(TraceEventType.Warning, 4001, "About to try something.");
                throw new Exception("Problem");
            }
            catch (Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 5001, "Exception: {0}", ex);
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
            trace.TraceEvent(TraceEventType.Verbose, 10001, "About to stop activity.");
            trace.TraceEvent(TraceEventType.Stop, 2002, "Activity stopped.");
            trace.TraceEvent(TraceEventType.Information, 8001, "Filtering example stopped.");
            Console.ReadLine();
        }
    }
}
