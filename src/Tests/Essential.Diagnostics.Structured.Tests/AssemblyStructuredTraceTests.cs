using Essential.Diagnostics.Structured;
using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class AssemblyStructuredTraceTests
    {
        [TestMethod()]
        public void StructuredTraceInterface()
        {
            var structuredTrace = new AssemblyStructuredTrace<TestEventId, TestClass>();
            var listener = structuredTrace.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            var c = new TestClass(structuredTrace);
            c.DoWork(1);

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual(1, events[0].Id);
            var data = (IStructuredData)events[0].Data[0];
            Assert.AreEqual("a{b}", data["MessageTemplate"]);
            Assert.AreEqual(1, data["b"]);
        }

        public enum TestEventId
        {
            TestEvent1 = 1,
        }

        public class TestClass
        {
            IStructuredTrace<TestEventId> trace;

            public TestClass(IStructuredTrace<TestEventId, TestClass> trace)
            {
                this.trace = trace;
            }

            public void DoWork(int b)
            {
                trace.Information(TestEventId.TestEvent1, "a{b}", b);
            }
        }
    }
}
