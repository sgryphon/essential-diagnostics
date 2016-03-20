using Essential.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeqFrameworkDiagnosticsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new TraceSource("SeqFrameworkDiagnosticsExample");
            var source2 = new TraceSource("SeqSource2");

            Thread.CurrentThread.Name = "TestThread";

            source.TraceEvent(TraceEventType.Information, 1, "Hello, {0}, from .NET Framework", Environment.UserName);

            const int k = 42;
            const int l = 100;

            using (var opertionScope = new LogicalOperationScope(source, "Transaction 12345", 21, 22))
            {
                source.TraceEvent(TraceEventType.Verbose, 2, "Sample verbose message, k={0}, l={1}", k, l);

                source2.TraceEvent(TraceEventType.Warning, 3, "Source 2 warning message, k={0}, l={1}", k, l);

                source.TraceData(TraceEventType.Information, 4, new object[] { "Data1", new DateTime(2000, 1, 1), 5 });

                source.TraceInformation("Information message");

                Trace.TraceError("Trace error");

                Trace.TraceInformation("Trace information");

                Trace.TraceWarning("Trace warning");

                Trace.WriteLine(DateTime.Now, "Category1");

                Trace.Write("Write only", "Category2");

                using (var activityScope = new ActivityScope(source, 11, 12, 13, 14,
                        "in", "start", "out", "stop"))
                {
                    source.TraceEvent(TraceEventType.Critical, 33, "Message critical {0}: {1}", 5, Thread.CurrentPrincipal.Identity);

                    try
                    {
                        throw new DivideByZeroException();
                    }
                    catch (Exception ex)
                    {
                        source.TraceEvent(TraceEventType.Error, 99, "Oops! {0}", ex);
                    }
                }
            }

            Console.ReadKey();

        }
    }
}
