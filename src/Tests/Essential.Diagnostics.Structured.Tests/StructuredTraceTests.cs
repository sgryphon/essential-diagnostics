using Essential.Diagnostics.Structured;
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
    public class StructuredTraceTests
    {
        [TestMethod()]
        public void StructuredTraceTemplateAndValues()
        {
            var structuredTrace = new StructuredTrace<StandardEventId>("structuredTraceTestSource");
            var listener = structuredTrace.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            structuredTrace.Critical(StandardEventId.ConfigurationCriticalError, "c{a}", 1);

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual(9000, events[0].Id);
            var data = (IStructuredData)events[0].Data[0];
            Assert.AreEqual("c{a}", data["MessageTemplate"]);
            Assert.AreEqual(1, data["a"]);
        }

        [TestMethod()]
        public void StructuredTraceScope()
        {
            var structuredTrace = new StructuredTrace<StandardEventId>("structuredTraceTestSource");
            var listener = structuredTrace.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            using (var scope = structuredTrace.BeginScope("a", 1))
            {
                structuredTrace.Critical(StandardEventId.ConfigurationCriticalError, "x{b}", 2);
            }

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            var data = (IStructuredData)events[0].Data[0];
            Assert.AreEqual("x{b}", data["MessageTemplate"]);
            Assert.AreEqual(2, data["b"]);
            var operation = events[0].LogicalOperationStack[0];
            Assert.AreEqual("a=1", operation);
        }

        [TestMethod()]
        public void StructuredTraceScopeMessage()
        {
            var structuredTrace = new StructuredTrace<StandardEventId>("structuredTraceTestSource");
            var listener = structuredTrace.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            using (var scope = structuredTrace.BeginScope(new StructuredData("y{a}", 1)))
            {
                structuredTrace.Critical(StandardEventId.ConfigurationCriticalError, "x{b}", 2);
            }

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            var data = (IStructuredData)events[0].Data[0];
            Assert.AreEqual("x{b}", data["MessageTemplate"]);
            Assert.AreEqual(2, data["b"]);
            var operation = events[0].LogicalOperationStack[0];
            Assert.AreEqual("y1", operation);
        }
    }
}
