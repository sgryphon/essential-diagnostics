using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LogRetentionExample
{
    public class Program
    {
        static TraceSource trace = new TraceSource("RetentionExample");

        public static void Main(string[] args)
        {
            Console.WriteLine("Retention example started.");
            trace.TraceEvent(TraceEventType.Information, 1001, "Retention example started.");

            int numberOfTasks = 16;
            int messagesPerTask = 100;

            Action sendMessages = () =>
            {
                var batchGuid = Guid.NewGuid();
                Console.WriteLine("{0:s} Batch {1} started.", DateTimeOffset.Now, batchGuid);
                for (int i = 0; i < messagesPerTask; i++)
                {
                    trace.TraceEvent(TraceEventType.Warning, 3000 + i, "Warning {0} - {1}", i, batchGuid);
                }
                Console.WriteLine("{0:s} Batch {1} finished.", DateTimeOffset.Now, batchGuid);
            };

            var factory = new TaskFactory();
            var tasks = new List<Task>(numberOfTasks);
            Console.WriteLine("{0:s} Before task loop.", DateTimeOffset.Now);
            for (int i = 0; i < numberOfTasks; i++)
            {
                tasks.Add(factory.StartNew(sendMessages));
            };
            Console.WriteLine("{0:s} After task loop.", DateTimeOffset.Now);

            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10));
            Console.WriteLine("{0:s} Finished (after wait all).", DateTimeOffset.Now);

            trace.TraceEvent(TraceEventType.Information, 8001, "Filtering example stopped.");
            Console.ReadLine();

        }
    }
}
