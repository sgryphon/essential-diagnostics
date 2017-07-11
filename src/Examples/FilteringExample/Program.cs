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
            var originalPrincipal = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("User1"), new string[0]);
                trace.TraceEvent(TraceEventType.Warning, 4001, "Some warning.");
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
            trace.TraceEvent(TraceEventType.Information, 8001, "Filtering example stopped.");
            Console.ReadLine();
        }
    }
}
