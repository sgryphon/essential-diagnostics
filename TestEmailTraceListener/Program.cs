using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TestEmailTraceListener
{
    class Program
    {
        static void Main(string[] args)
        {
            var smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;

            string pickupDirectory = (smtpConfig != null) ? smtpConfig.SpecifiedPickupDirectory.PickupDirectoryLocation : null;

            string[] filePaths = Directory.GetFiles(pickupDirectory);
            foreach (string filePath in filePaths)
                File.Delete(filePath);

            Console.WriteLine("Test if all messages are sent before the hosting process finishes.");
            Func<int> doSomething = () =>
            {
                Trace.TraceWarning("Anything. More detail go here.");
                Trace.TraceError("Error. More detail go here.");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.WriteLine("This is another writeline.", "caTegory");
                Trace.WriteLine("Writeline without right category", "CCCC");
                return 0;

            };
            const int count = 5000;
          //  Parallel.For(0, count, d);

            TaskFactory<int> factory = new TaskFactory<int>(TaskCreationOptions.PreferFairness, TaskContinuationOptions.LongRunning);//so I have more threads
            List<Task> tasks = new List<Task>(count);
            for (int i = 0; i < count; i++)
            {
                tasks.Add(factory.StartNew(doSomething));
            }

            Task.WaitAll(tasks.ToArray(), System.Threading.Timeout.Infinite);//so now we have a long mail message queue.

            //the listener will wait the end of the process anyway.
            System.Threading.Thread.Sleep(30000);//just to wait all threads finished.
            Console.WriteLine(String.Format("Check if there are {0} mail messages in pickup directory at {1}.", count*2, pickupDirectory));
        }


    }
}
