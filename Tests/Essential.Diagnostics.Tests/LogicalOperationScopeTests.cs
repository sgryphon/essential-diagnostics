using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class LogicalOperationScopeTests
    {
        [TestMethod]
        public void ScopeShouldHaveAStackOfValues()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            source.TraceEvent(TraceEventType.Warning, 1, "A");
            using (var scope1 = new LogicalOperationScope("X"))
            {
                using (var scope2 = new LogicalOperationScope("Y"))
                {
                    source.TraceEvent(TraceEventType.Warning, 2, "B");
                }
            }
            source.TraceEvent(TraceEventType.Warning, 3, "C");

            var events = listener.GetEvents();

            Assert.AreEqual(1, events[0].Id);
            var logicalOperationStack0 = events[0].GetLogicalOperationStack();
            Assert.AreEqual(0, logicalOperationStack0.Length);

            Assert.AreEqual(2, events[1].Id);
            var logicalOperationStack1 = events[1].GetLogicalOperationStack();
            Assert.AreEqual(2, logicalOperationStack1.Length);
            Assert.AreEqual("Y", logicalOperationStack1[0]);
            Assert.AreEqual("X", logicalOperationStack1[1]);

            Assert.AreEqual(3, events[2].Id);
            var logicalOperationStack2 = events[2].GetLogicalOperationStack();
            Assert.AreEqual(0, logicalOperationStack2.Length);
        }
    }
}
