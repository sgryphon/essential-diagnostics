using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Essential.Diagnostics.Abstractions;
using System.Threading;
using System.Security.Principal;

namespace TemplateArguments
{
    public class Program
    {
        static ITraceSource trace = new AssemblyTraceSource<Program>();

        public static void Main(string[] args)
        {
            var originalPrincipal = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("User1"), new string[0]);
                Trace.CorrelationManager.StartLogicalOperation("Started=true");
                Trace.CorrelationManager.StartLogicalOperation("UserId=1");
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                // Trace Data
                var data = new object[] { "TraceData", TimeZoneInfo.Local.BaseUtcOffset };
                trace.TraceData(TraceEventType.Verbose, 6001, data);
                // Trace Transfer
                var newActivityId = Guid.NewGuid();
                trace.TraceTransfer(0, "TraceTransfer", newActivityId);
                Trace.CorrelationManager.ActivityId = newActivityId;
                // Trace Event
                Thread.CurrentThread.Name = "MyThread";
                trace.TraceEvent(TraceEventType.Information, 7001, "TraceEvent");
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
            Console.ReadLine();
        }
    }
}
