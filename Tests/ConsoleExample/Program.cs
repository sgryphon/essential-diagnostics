using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<int> doSomething = () =>
            {
                var x = new Random().Next(26);
                Debug.WriteLine((char)(97 + x));
                Trace.TraceWarning("Anything. More detail go here.");
                Trace.TraceError("Error. More detail go here.");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.TraceInformation("Just some information");
                return 0;
            };
            const int count = 500;

            TaskFactory<int> factory = new TaskFactory<int>(TaskCreationOptions.PreferFairness, TaskContinuationOptions.LongRunning);//so I have more threads
            List<Task> tasks = new List<Task>(count);
            Console.WriteLine("{0:s} Before task loop.", DateTimeOffset.Now);
            Debug.WriteLine(string.Format("{0:s} Before task loop.", DateTimeOffset.Now));
            for (int i = 0; i < count; i++)
            {
                tasks.Add(factory.StartNew(doSomething));
            };
            Console.WriteLine("{0:s} After task loop.", DateTimeOffset.Now);
            Debug.WriteLine(string.Format("{0:s} After task loop.", DateTimeOffset.Now));
            Task.WaitAll(tasks.ToArray(), System.Threading.Timeout.Infinite);//so now we have a long mail message queue.
            Console.WriteLine("{0:s} After wait all.", DateTimeOffset.Now);
            Debug.WriteLine(string.Format("{0:s} After wait all.", DateTimeOffset.Now));
            Console.ReadLine();
        }
    }
}
