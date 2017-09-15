using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TestErrorBufferEmailTraceListener
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
                var x = new Random().Next(26);
                Console.Write((char)(97 + x));
                Debug.WriteLine((char)(97 + x));

                Trace.TraceWarning("Anything. More detail go here.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Trace.TraceError("Error. More detail go here.@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@222222222222222222222222222222222222");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.WriteLine("This is another writeline.", "caTegory");
                Trace.WriteLine("Writeline without right category", "CCCC");
                Console.Write((char)(65 + x));
                Debug.WriteLine((char)(65 + x));
            };
            const int count = 200;
            Parallel.For(0, count, d);

            Trace.TraceWarning("The last message");
            System.Threading.Thread.Sleep(200);
            //though it is not likely a console app could have many threads, test the performance with threads anyway, while BufferedEmailTraceListener's IsThreadSafe=false;
            Console.WriteLine(String.Format("Check if there is 1 mail message in pickup directory at {0} in the same size.", pickupDirectory));
        }
    }
}
