using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using Essential.Diagnostics;

namespace MonitorConfig
{
    class Program
    {
        private static TraceConfigurationMonitor configMonitor;
        private static int count;
        private const double intervalMilliseconds = 2000;
        static TraceSource timerTrace = new TraceSource("MonitorConfig");

        static void Main(string[] args)
        {
            configMonitor = new TraceConfigurationMonitor();
            configMonitor.Start();

            StartTimerAndWait();

            configMonitor.Stop();
            configMonitor.Dispose();
        }

        private static void StartTimerAndWait()
        {
            using (var timer = new Timer(intervalMilliseconds))
            {
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Enabled = true;

                Console.WriteLine("TraceConfigurationMonitor is started.");
                Console.WriteLine("Modify the MonitorConfig.exe.config file to see the effects.");
                Console.ReadKey();
            }
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var eventTypes = new TraceEventType[]
                                {
                                    TraceEventType.Verbose,
                                    TraceEventType.Information,
                                    TraceEventType.Warning,
                                    TraceEventType.Error,
                                    TraceEventType.Critical,
                                };
            var level = count % eventTypes.Length;
            var eventType = eventTypes[level];
            timerTrace.TraceEvent(eventType, count, "Event {0}, type {1}.", count, eventType);
            count++;
        }

    }
}
