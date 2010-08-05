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
            Assert.AreEqual(0, events[0].LogicalOperationStack.Length);

            Assert.AreEqual(2, events[1].Id);
            Assert.AreEqual(2, events[1].LogicalOperationStack.Length);
            Assert.AreEqual("Y", events[1].LogicalOperationStack[0]);
            Assert.AreEqual("X", events[1].LogicalOperationStack[1]);

            Assert.AreEqual(3, events[2].Id);
            Assert.AreEqual(0, events[2].LogicalOperationStack.Length);
        }
    }
}
