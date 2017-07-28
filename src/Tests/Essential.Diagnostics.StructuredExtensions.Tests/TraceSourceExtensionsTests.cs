using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class TraceSourceExtensionsTests
    {
        [TestMethod()]
        public void TraceStructuredMessage()
        {
            var traceSource = new TraceSource("testSource");
            var listener = traceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            traceSource.TraceStructuredData(TraceEventType.Critical, 9000, "x{a}", 1);

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual(9000, events[0].Id);
            var data = events[0].Data[0];
            Assert.AreEqual("x1", data.ToString());
        }

        [TestMethod()]
        public void TraceStructuredProperties()
        {
            var traceSource = new TraceSource("testSource");
            var listener = traceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            var properties = new Dictionary<string, object>() {
                { "a", 1 },
                { "b", "B" },
            };
            traceSource.TraceStructuredData(TraceEventType.Critical, 9000, properties);

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual(9000, events[0].Id);
            var data = events[0].Data[0];
            Assert.AreEqual("a=1 b='B'", data.ToString());
        }
    }
}
