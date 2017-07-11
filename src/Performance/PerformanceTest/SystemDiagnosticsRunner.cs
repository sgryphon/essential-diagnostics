using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PerformanceTest
{
    class SystemDiagnosticsRunner : RunnerBase
    {
        string description;
        TraceSource trace1;
        TraceSource trace2;

        public SystemDiagnosticsRunner(string description, string source1Name, string source2Name)
        {
            this.description = description;
            trace1 = new TraceSource(source1Name);
            trace2 = new TraceSource(source2Name);
        }

        public override string Name
        {
            get
            {
                return "Sys.Diag:" + description;
            }
        }

        protected override void TraceCritical1(int id, string message, params object[] data)
        {
            trace1.TraceEvent(TraceEventType.Critical, id, message, data);
        }

        protected override void TraceVerbose1(int id, string message, params object[] data)
        {
            trace1.TraceEvent(TraceEventType.Verbose, id, message, data);
        }

        protected override void TraceVerbose2(int id, string message, params object[] data)
        {
            trace2.TraceEvent(TraceEventType.Verbose, id, message, data);
        }

        protected override void TraceWarning2(int id, string message, params object[] data)
        {
            trace2.TraceEvent(TraceEventType.Warning, id, message, data);
        }
    }
}
