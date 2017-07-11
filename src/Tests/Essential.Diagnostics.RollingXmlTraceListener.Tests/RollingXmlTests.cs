using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class RollingXmlTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void XmlHandlesEventSentDirectly()
        {
            var mockFileSystem = new MockFileSystem();
            var listener = new RollingXmlTraceListener(null);
            listener.FileSystem = mockFileSystem;

            listener.TraceEvent(null, "source", TraceEventType.Information, 1, "{0}-{1}", 2, "A");
            listener.Flush();

            Assert.AreEqual(1, mockFileSystem.OpenedItems.Count);
            var tuple0 = mockFileSystem.OpenedItems[0];
            // (earlier name was "QTAgent32-")
            // VS2012 process name "vstest.executionengine.x86-"
            // VS2015 process name "te.processhost.managed-"
            //StringAssert.StartsWith(tuple0.Item1, "vstest.executionengine.x86-" + DateTimeOffset.Now.Year.ToString());
            var data = tuple0.Item2.GetBuffer();
            var output = Encoding.UTF8.GetString(data, 0, (int)tuple0.Item2.Length);
            StringAssert.StartsWith(output, "<E2ETraceEvent");
        }

        [TestMethod]
        public void XmlHandlesEventFromTraceSource()
        {
            var mockFileSystem = new MockFileSystem();
            TraceSource source = new TraceSource("rollingXml1Source");
            var listener = source.Listeners.OfType<RollingXmlTraceListener>().First();
            listener.FileSystem = mockFileSystem;

            source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B");
            source.Flush(); // or have AutoFlush configured

            Assert.AreEqual(1, mockFileSystem.OpenedItems.Count);
            var tuple0 = mockFileSystem.OpenedItems[0];
            // (earlier name was "QTAgent32-")
            // VS2012 process name "vstest.executionengine.x86-"
            // VS2015 process name "te.processhost.managed-"
            //StringAssert.StartsWith(tuple0.Item1, "vstest.executionengine.x86-" + DateTimeOffset.Now.Year.ToString());
            var output = Encoding.UTF8.GetString(tuple0.Item2.GetBuffer(), 0, (int)tuple0.Item2.Length);
            StringAssert.StartsWith(output, "<E2ETraceEvent");
        }

        [TestMethod]
        public void XmlRollOverTest()
        {
            var mockFileSystem = new MockFileSystem();
            var listener = new RollingXmlTraceListener("Log{DateTime:HHmmss}");
            listener.FileSystem = mockFileSystem;

            listener.TraceEvent(null, "souce", TraceEventType.Information, 1, "A");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            listener.TraceEvent(null, "souce", TraceEventType.Information, 2, "B");
            listener.Flush();

            Assert.AreEqual(2, mockFileSystem.OpenedItems.Count);
        }

        [TestMethod]
        public void XmlConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("rollingXml2Source");
            var listener = source.Listeners.OfType<RollingXmlTraceListener>().First();

            Assert.AreEqual("rollingXml2", listener.Name);
            Assert.AreEqual("Trace{DateTime:yyyyMMdd}.svclog", listener.FilePathTemplate);
        }

    }
}
