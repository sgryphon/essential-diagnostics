using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Abstractions;
using System.Diagnostics;

namespace AbstractionDependency
{
    public class Application
    {
        ITraceSource trace;

        public Application()
            : this(new AssemblyTraceSource<Application>())
        {
        }

        public Application(ITraceSource<Application> trace)
        {
            this.trace = trace;
        }

        public void Run()
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "AbstractionDependency example start.");
            trace.TraceEvent(TraceEventType.Information, 8001, "AbstractionDependency example end.");
        }
    }
}
