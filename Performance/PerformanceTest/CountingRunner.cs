using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTest
{
    class CountingRunner : RunnerBase
    {
        int critical1 = 0;
        int verbose1 = 0;
        int verbose2 = 0;
        int warning2 = 0;

        protected override void TraceCritical1(int id, string message, params object[] data)
        {
            critical1++;
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
            verbose1++;
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
            verbose2++;
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
            warning2++;
        }

        protected override void Start()
        {
            critical1 = 0;
            verbose1 = 0;
            verbose2 = 0;
            warning2 = 0;
        }

        protected override void Finish()
        {
            if (Output)
            {
                Console.WriteLine("Trace1 received {0} verbose and {1} critical.", verbose1, critical1);
                Console.WriteLine("Trace2 received {0} verbose and {1} warning.", verbose2, warning2);
            }
        }
    }
}
