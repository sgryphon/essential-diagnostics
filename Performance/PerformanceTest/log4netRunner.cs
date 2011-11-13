using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace PerformanceTest
{
    class log4netRunner : RunnerBase
    {
        string description;
        ILog log1;
        ILog log2;

        public log4netRunner(string description, string source1Name, string source2Name)
        {
            this.description = description;
            log1 = LogManager.GetLogger(source1Name);
            log2 = LogManager.GetLogger(source2Name);
        }

        public override string Name
        {
            get
            {
                return "log4net:" + description;
            }
        }

        protected override void TraceCritical1(int id, string message, params object[] data)
        {
            log1.FatalFormat(message, data);
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
            log1.DebugFormat(message, data);
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
            log2.DebugFormat(message, data);
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
            log2.WarnFormat(message, data);
        }
    }
}
