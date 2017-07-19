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
    }
}
