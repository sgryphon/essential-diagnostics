using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTest
{
    class NullRunner : RunnerBase
    {
        public override string Name
        {
            get
            {
                return base.Name + " (base time)";
            }
        }
        protected override void TraceCritical1(int id, string message, params object[] data)
        {
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
        }
    }
}
