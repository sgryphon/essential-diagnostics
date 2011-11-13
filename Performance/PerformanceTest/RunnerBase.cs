using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Abstractions;
using System.Diagnostics;

namespace PerformanceTest
{
    public abstract class RunnerBase
    {
        int[] data1 = new int[] { 1, 2, 3, 5, 8, 13, 21 };
        string[] data2 = new string[] { "alpha", "beta", "gamma" };
        TimeSpan elapsed;

        public virtual string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public TimeSpan BaseTime { get; set; }

        public TimeSpan Elapsed { get { return elapsed; } }

        public int Iterations { get; set; }

        public bool Output { get; set; }

        public void Run()
        {
            if (Output)
            {
                Console.WriteLine("");
                //Console.WriteLine("{2:s} start (x{1}) {0}.", Name, Iterations, DateTimeOffset.Now);
            }
            Start();
            Stopwatch sw = Stopwatch.StartNew();

            var countCritical = 0;
            for (int index = 0; index < Iterations; index++)
            {
                var counter1 = index & 0x0FFF;
                var counter2 = (index >> 1) & 0x0FFF;
                var index1 = index % 7;
                var index2 = index % 3;

                if ((index & 0xffff) == 0)
                {
                    // Log critical every 64,000 messages
                    TraceCritical1(9000 + countCritical++, "Message (critical error).");
                }
                else if ((index & 0x0fff) == 0)
                {
                    // Log warning every 4,000 messages
                    TraceWarning2(4000 + index1 + index2, "Message (warning {0}).", counter2);
                }
                else
                {
                    // Log verbose
                    if ((index & 0xFF) == 0)
                    {
                        if ((index & 0x1) == 0)
                        {
                            TraceVerbose1(counter1, "Message 1 is {0} + {1}.", data1[index1], data2[index2]);
                        }
                        else
                        {
                            TraceVerbose1(counter2, "Message 2 is {0}.", data1[index1]);
                        }
                    }
                    else
                    {
                        if ((index & 0x1) == 0)
                        {
                            TraceVerbose2(counter1, "Message 1 is {0} + {1}.", data1[index1], data2[index2]);
                        }
                        else
                        {
                            TraceVerbose2(counter2, "Message 2 is {0}.", data1[index1]);
                        }
                    }
                }
            }

            sw.Stop();
            elapsed = sw.Elapsed;
            if (Output)
            {
                //Console.WriteLine("{1:s} stop {0}.", Name, DateTimeOffset.Now);
                var difference = Elapsed - BaseTime;
                Console.WriteLine("{1,10:f4} difference (x{2}) for {0}", Name, difference.TotalMilliseconds, Iterations);
            }
            Finish();
        }

        protected abstract void TraceCritical1(int id, string message, params object[] data);
        protected abstract void TraceVerbose1(int id, string message, params object[] data);

        protected abstract void TraceVerbose2(int id, string message, params object[] data);
        protected abstract void TraceWarning2(int id, string message, params object[] data);

        protected virtual void Start() { }
        protected virtual void Finish() { }
    }
}
