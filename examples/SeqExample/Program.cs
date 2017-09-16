using System;
using System.Diagnostics;
using System.Threading;

using Essential.Diagnostics;
using Essential.Diagnostics.Abstractions;

namespace SeqFrameworkDiagnostics.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            // Basic source
            var source = new TraceSource("SeqFrameworkDiagnostics.Example.Basic");
            source.TraceEvent(TraceEventType.Information, (int)ExampleEventId.StartExample, "Hello, {0}, from .NET Framework", Environment.UserName);
            source.TraceData(TraceEventType.Information, (int)ExampleEventId.DataTrace, "Data Item", 42.80D, Guid.NewGuid());

            source.TraceEvent(TraceEventType.Verbose, 0, "a={1} b={0}", "B", "A");

            // Detailed source
            var detailedSource = new TraceSource("SeqFrameworkDiagnostics.Example.Detailed");
            detailedSource.TraceEvent(TraceEventType.Warning, (int)ExampleEventId.DetailedWarning, "Sample detailed warning");

            // Logical operation stack context (using scope extension)
            using (var requestScope = new LogicalOperationScope(source, "Request 1234", (int)ExampleEventId.BeginRequest, (int)ExampleEventId.EndRequest, "Begin request", "End request"))
            {
                using (var transactionScope = new LogicalOperationScope("Transaction 5678"))
                {
                    source.TraceEvent(TraceEventType.Information, (int)ExampleEventId.LogicalOperationStackExample, "Sample with operation stack context {0}, {1}, {2}", 42, new DateTimeOffset(1973, 5, 14, 0, 0, 0, TimeSpan.FromHours(10)), "Fnord");
                }
            }

            // Activity ID & transfers (using scope extension)
            using (var activityScope = new ActivityScope(source))
            {
                source.TraceEvent(TraceEventType.Information, (int)ExampleEventId.ActivityTransferExample, "Correlation example with activity ID transfer");
            }

            // Fluent interface extensions
            ITraceLog<ExampleEventId> log = new AssemblyTraceLog<ExampleEventId, Program>();
            try
            {
                log.Information(ExampleEventId.FluentExample, "Fluent logging API, {0}", Environment.OSVersion);
                log.Verbose("About to throw exception");
                throw new DivideByZeroException();
            }
            catch (Exception ex)
            {
                log.Error(ExampleEventId.DivideException, ex, "Ooops!");
            }
            stopwatch.Stop();
            log.Verbose("Time {0:'P''T'hh'H'mm'M'ss'.'fff'S'}", stopwatch.Elapsed);
            log.Information(ExampleEventId.EndExample, "Done");

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        public enum ExampleEventId
        {
            StartExample = 1000,
            DataTrace = 2000,
            LogicalOperationStackExample = 3100,
            ActivityTransferExample = 3200,
            FluentExample = 3300,
            DetailedWarning = 4000,
            DivideException = 5000,
            BeginRequest = 6000,
            EndRequest = 7000,
            EndExample = 8000
        }
    }
}
