using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essential.Diagnostics.Tests.Utility;
using System.Diagnostics;

namespace Essential.Diagnostics.Abstractions.Tests
{
    [TestClass()]
    public class GenericTraceLogTests
    {
        [TestMethod()]
        public void GenericTraceLogTestClass()
        {
            var log = new GenericTraceLog("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Information(GenericEventId.SystemStart, "System started");

            var events = listener.MethodCallInformation;

            Assert.AreEqual(1100, events[0].Id);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
            Assert.AreEqual("System started", events[0].Message);
        }

        [TestMethod()]
        public void TraceLogTestGeneric()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Information(GenericEventId.SystemStart, "System started");

            var events = listener.MethodCallInformation;

            Assert.AreEqual(1100, events[0].Id);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
            Assert.AreEqual("System started", events[0].Message);
        }

        [TestMethod()]
        public void TraceLogTestCustom()
        {
            var log = new TraceLog<MyEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Information(MyEventId.MyEvent2, "a1");

            var events = listener.MethodCallInformation;

            Assert.AreEqual(2, events[0].Id);
            Assert.AreEqual("a1", events[0].Message);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
        }

        private enum MyEventId
        {
            MyEvent1 = 1,
            MyEvent2 = 2
        }

        [TestMethod()]
        public void TraceLogTestInt32()
        {
            var log = new TraceLog<int>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Information(3, "a1");

            var events = listener.MethodCallInformation;
            Assert.AreEqual(3, events[0].Id);
            Assert.AreEqual("a1", events[0].Message);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
        }

        [TestMethod()]
        [ExpectedException(typeof(TypeInitializationException))]
        public void TraceLogTestInt64ShouldError()
        {
            var log = new TraceLog<long>("tracelogTestSource");
            log.Information(3L, "a1");
        }

    }
}