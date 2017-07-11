using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Essential.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EssentialDiagnosticsIntegrationTests
{
    [TestClass]
    public class RollingFileTraceListenerTests
    {
        [TestMethod]
        public void DisposedTextWriterShouldNotBeReused()
        {
            var template = "textWriterTest{0}.log";

            var a1 = Guid.NewGuid();
            var a2 = Guid.NewGuid();

            var a1File = string.Format(template, a1);
            var a2Files =
                Enumerable.Repeat(string.Format(template, a2), 1)
                    .Concat(
                        Enumerable.Range(1, 5)
                            .Select(i => string.Format(template, string.Format("{0}-{1}", a2, i)))
                    )
                    .ToArray();

            try
            {
                //lock files for a2
                using(var f0 = File.Open(a2Files[0], FileMode.Append, FileAccess.Write, FileShare.Read))
                using(var f1 = File.Open(a2Files[1], FileMode.Append, FileAccess.Write, FileShare.Read))
                using(var f2 = File.Open(a2Files[2], FileMode.Append, FileAccess.Write, FileShare.Read))
                using(var f3 = File.Open(a2Files[3], FileMode.Append, FileAccess.Write, FileShare.Read))
                using(var f4 = File.Open(a2Files[4], FileMode.Append, FileAccess.Write, FileShare.Read))
                using(var f5 = File.Open(a2Files[5], FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var listener = new RollingFileTraceListener(string.Format(template, "{ACTIVITYID}")))
                {
                    Trace.CorrelationManager.ActivityId = a1;
                    listener.TraceEvent(
                        null, 
                        "source",
                        TraceEventType.Information, 
                        1, 
                        "A1.1");
                    try
                    {
                        Trace.CorrelationManager.ActivityId = a2;
                        listener.TraceEvent(
                            null,
                            "source",
                            TraceEventType.Information,
                            1,
                            "A2.1");

                    }
                    catch (InvalidOperationException)
                    {
                        //should be exception about exosted number of retries
                        //we swallow it
                    }

                    //now trace with the first TextWriter and it should not throw "System.ObjectDisposedException : Cannot write to a closed TextWriter" expection
                    Trace.CorrelationManager.ActivityId = a1;
                    listener.TraceEvent(
                        null,
                        "source",
                        TraceEventType.Information,
                        1,
                        "A1.2");
                }
            }
            finally
            {
                //clean up files
                if (File.Exists(a1File))
                    File.Delete(a1File);
                foreach (var f in a2Files)
                    if (File.Exists(f))
                        File.Delete(f);
            }
        }
    }
}
