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
    public class AssemblyTraceLogTests
    {
        [TestMethod()]
        public void AssemblyTraceLogTest()
        {
            var log = new AssemblyTraceLog<MyEventId, AssemblyTraceLogTests>();
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Information(MyEventId.MyApplicationStart, "a1");
            try
            {
                throw new ApplicationException("b1");
            }
            catch (Exception ex)
            {

                log.Error(MyEventId.MyApplicationError, ex, "a{0}", 2);
            }

            Assert.AreEqual("Essential.Diagnostics.Tests", log.TraceSource.Name);

            var events = listener.MethodCallInformation;
            Assert.AreEqual("Essential.Diagnostics.Tests", events[0].Source);
            Assert.AreEqual(1100, events[0].Id);
            Assert.AreEqual(TraceEventType.Information, events[0].EventType);
            Assert.AreEqual("a1", events[0].Message);

            Assert.AreEqual(5100, events[1].Id);
            Assert.AreEqual(TraceEventType.Error, events[1].EventType);
            StringAssert.StartsWith(events[1].Message, "a2|Exception: System.ApplicationException: b1");
        }

        private enum MyEventId
        {
            MyApplicationStart = 1100,
            MyApplicationError = 5100
        }

        [TestMethod()]
        public void AlternateExceptionFormatting()
        {
            var log = new AssemblyTraceLog<MyEventId, AssemblyTraceLogTests>();
            log.ExceptionSeparator = " ";
            log.ExceptionFormat = "Exception={{{0}}}";
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            try
            {
                throw new ApplicationException("b1");
            }
            catch (Exception ex)
            {

                log.Error(MyEventId.MyApplicationError, ex, "a{0}", 2);
            }

            var events = listener.MethodCallInformation;
            Assert.AreEqual(5100, events[0].Id);
            Assert.AreEqual(TraceEventType.Error, events[0].EventType);
            Console.WriteLine(events[0].Message);
            StringAssert.StartsWith(events[0].Message, "a2 Exception={System.ApplicationException: b1");
        }
    }
}