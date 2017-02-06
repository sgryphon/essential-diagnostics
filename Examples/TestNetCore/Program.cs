using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HelloNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var testTraceSource = new TestTraceSource();
            testTraceSource.Run();

            var testEventSource = new TestEventSource();
            testEventSource.Run();

            Console.ReadLine();
        }
    }
}
