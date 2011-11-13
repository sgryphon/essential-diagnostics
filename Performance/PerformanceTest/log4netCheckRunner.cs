using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace PerformanceTest
{
    class log4netCheckRunner : RunnerBase
    {
        ILog log1 = LogManager.GetLogger("Test.Logger1");
        ILog log2 = LogManager.GetLogger("Test.Logger2");

        protected override void TraceCritical1(int id, string message, params object[] data)
        {
            if (log1.IsFatalEnabled)
            {
                log1.FatalFormat(message, data);
            }
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
            if (log1.IsDebugEnabled)
            {
                log1.DebugFormat(message, data);
            }
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
            if (log2.IsDebugEnabled)
            {
                log2.DebugFormat(message, data);
            }
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
            if (log2.IsWarnEnabled)
            {
                log2.WarnFormat(message, data);
            }
        }
    }
}
