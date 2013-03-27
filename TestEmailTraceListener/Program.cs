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
            Action<int> d = (k) =>
            {
                Trace.TraceWarning("Anything. More detail go here.");
                Trace.TraceError("Error. More detail go here.");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.WriteLine("This is another writeline.", "caTegory");
                Trace.WriteLine("Writeline without right category", "CCCC");

            };
            const int count = 1000;
             Parallel.For(0, count,d);

            //the listener will wait the end of the process anyway.
            Console.WriteLine(String.Format("Check if there are {0} mail messages in pickup directory at {1}.", count, pickupDirectory));
        }


    }
}
