using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace PerformanceTest
{
    class NLogRunner : RunnerBase
    {
        string description;
        Logger logger1;
        Logger logger2;

        public NLogRunner(string description, string source1Name, string source2Name)
        {
            this.description = description;
            logger1 = LogManager.GetLogger(source1Name);
            logger2 = LogManager.GetLogger(source2Name);
        }

        public override string Name
        {
            get
            {
                return "NLog:" + description;
            }
        }

        protected override void TraceCritical1(int id, string message, params object[] data)
        {
            logger1.Fatal(message, data);
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
            logger1.Debug(message, data);
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
            logger2.Debug(message, data);
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
            logger2.Warn(message, data);
        }
    }
}
