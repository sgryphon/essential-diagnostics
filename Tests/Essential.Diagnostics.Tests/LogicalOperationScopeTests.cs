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

        [TestMethod]
        public void ScopeDefaultMessages()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            using (var scope1 = new LogicalOperationScope(source, "X"))
            {
                source.TraceEvent(TraceEventType.Warning, 1, "A");
            }

            var events = listener.GetEvents();

            Assert.AreEqual(3, events.Length);
            Assert.AreEqual("Start operation", events[0].Message);
            Assert.AreEqual("Stop operation", events[2].Message);
        }

        [TestMethod]
        public void ScopeCustomMessages()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            using (var scope1 = new LogicalOperationScope(source, "X", 11, 12, "Operation Go", "Done"))
            {
                source.TraceEvent(TraceEventType.Warning, 1, "A");
            }

            var events = listener.GetEvents();

            Assert.AreEqual(3, events.Length);
            Assert.AreEqual("Operation Go", events[0].Message);
            Assert.AreEqual(11, events[0].Id);
            Assert.AreEqual("Done", events[2].Message);
            Assert.AreEqual(12, events[2].Id);
        }

        [TestMethod]
        public void ScopeManualStartStopMessages()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            var operationId = "X";
            using (var scope1 = new LogicalOperationScope(operationId))
            {
                source.TraceEvent(TraceEventType.Start, 11, "Start {0}", operationId);

                source.TraceEvent(TraceEventType.Warning, 1, "A");

                source.TraceEvent(TraceEventType.Stop, 12, "Stop {0}", operationId);
            }

            var events = listener.GetEvents();

            Assert.AreEqual(3, events.Length);
            Assert.AreEqual("Start X", events[0].Message);
            Assert.AreEqual(11, events[0].Id);
            Assert.AreEqual("Stop X", events[2].Message);
            Assert.AreEqual(12, events[2].Id);
        }

    }
}
