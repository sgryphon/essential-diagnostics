using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Essential.Diagnostics.Tests.Utility;

namespace Essential.Diagnostics.Abstractions.Tests
{
    [TestClass()]
    public class TraceLogTests
    {
        [TestMethod()]
        public void TraceLogTestCreateNamedSource()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");

            Assert.AreEqual("tracelogTestSource", log.TraceSource.Name);
            Assert.AreEqual(SourceLevels.All, log.TraceSource.Switch.Level);
        }

        [TestMethod()]
        public void TraceLogTestCreateFromExistingSource()
        {
            TraceSource source = new TraceSource("tracelog2Source");

            var log = new TraceLog<GenericEventId>(source);

            Assert.AreEqual("tracelog2Source", log.TraceSource.Name);
            Assert.AreEqual(SourceLevels.Off, log.TraceSource.Switch.Level);
        }

        [TestMethod()]
        public void TraceLogTestCreateFromAssemblyTraceSource()
        {
            ITraceSource source = new AssemblyTraceSource<TraceLogTests>();

            var log = new TraceLog<GenericEventId>(source);

            Assert.AreEqual("Essential.Diagnostics.Fluent.Tests", log.TraceSource.Name);
        }

        [TestMethod()]
        public void CriticalTestWithThrownExceptionOnly()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            try
            {
                throw new ApplicationException("a0");
            }
            catch (Exception ex)
            {
                log.Critical(GenericEventId.AuthenticationCriticalError, ex);
            }

            var events = listener.MethodCallInformation;

            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
            Assert.AreEqual(9300, events[0].Id);

            Console.WriteLine(events[0].Message);
            var expectedMessageWithStartOfStackTrace = "Exception: System.ApplicationException: a0\r\n   at Essential.Diagnostics.Abstractions.Tests.TraceLogTests.CriticalTestWithThrownExceptionOnly() ";
            StringAssert.StartsWith(events[0].Message, expectedMessageWithStartOfStackTrace);
        }

        [TestMethod()]
        public void CriticalTestWithExceptionAndMessage()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            var ex = new ApplicationException("a1");
            log.Critical(GenericEventId.ConfigurationCriticalError, ex, "b{0}");

            var events = listener.MethodCallInformation;

            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
            Assert.AreEqual(9000, events[0].Id);

            Console.WriteLine(events[0].Message);
            var expectedMessageWithBraces = "b{0}|Exception: System.ApplicationException: a1";
            StringAssert.StartsWith(events[0].Message, expectedMessageWithBraces);
        }

        [TestMethod()]
        public void CriticalTestWithExceptionAndFormatAndArgs()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            var ex = new ApplicationException("a2");
            log.Critical(GenericEventId.ConnectionCriticalError, ex, "b{0}", 1);

            var events = listener.MethodCallInformation;

            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual("tracelogTestSource", events[0].Source);
            Assert.AreEqual(9200, events[0].Id);

            Console.WriteLine(events[0].Message);
            var expectedMessageWithInsertedArgs = "b1|Exception: System.ApplicationException: a2";
            StringAssert.StartsWith(events[0].Message, expectedMessageWithInsertedArgs);
        }

        [TestMethod()]
        public void CriticalTestMessage()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Critical(GenericEventId.ConfigurationCriticalError, "c{0}");

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual(9000, events[0].Id);
            StringAssert.StartsWith(events[0].Message, "c{0}");
        }

        [TestMethod()]
        public void CriticalTestFormatAndArgs()
        {
            var log = new TraceLog<GenericEventId>("tracelogTestSource");
            var listener = log.TraceSource.Listeners.OfType<TestTraceListener>().First();
            listener.MethodCallInformation.Clear();

            log.Critical(GenericEventId.ConfigurationCriticalError, "c{0}", 1);

            var events = listener.MethodCallInformation;
            Assert.AreEqual(TraceEventType.Critical, events[0].EventType);
            Assert.AreEqual(9000, events[0].Id);
            StringAssert.StartsWith(events[0].Message, "c1");
        }

        [TestMethod()]
        public void ErrorTestException()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Error(GenericEventId.AuthenticationError, 
                        new ApplicationException("a1")))
                .ThenVerifyTraceInfo(TraceEventType.Error, 
                    5300, 
                    "Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void ErrorTestExceptionAndMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Error(GenericEventId.AuthenticationError,
                        new ApplicationException("a1"),
                        "b{1}"))
                .ThenVerifyTraceInfo(TraceEventType.Error,
                    5300,
                    "b{1}|Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void ErrorTestExceptionAndFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Error(GenericEventId.AuthenticationError,
                        new ApplicationException("a1"),
                        "b{0}",
                        2))
                .ThenVerifyTraceInfo(TraceEventType.Error,
                    5300,
                    "b2|Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void ErrorTestMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Error(GenericEventId.AuthenticationError,
                        "b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Error,
                    5300,
                    "b{0}");
        }

        [TestMethod()]
        public void ErrorTestFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Error(GenericEventId.AuthenticationError,
                        "b{0}",
                        3))
                .ThenVerifyTraceInfo(TraceEventType.Error,
                    5300,
                    "b3");
        }

        [TestMethod()]
        public void InformationTestMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Information(GenericEventId.ConfigurationStart,
                        "b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Information,
                    1000,
                    "b{0}");
        }

        [TestMethod()]
        public void InformationTestFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Information(GenericEventId.ConnectionEvent,
                        "a{0}",
                        2))
                .ThenVerifyTraceInfo(TraceEventType.Information,
                    2200,
                    "a2");
        }

        [TestMethod()]
        public void VerboseTestMessageNoId()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Verbose("b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Verbose,
                    0,
                    "b{0}");
        }

        [TestMethod()]
        public void VerboseTestFormatAndArgsNoId()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Verbose("b{0}",
                    1))
                .ThenVerifyTraceInfo(TraceEventType.Verbose,
                    0,
                    "b1");
        }

        [TestMethod()]
        public void VerboseTestMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Verbose((GenericEventId)10001,
                        "b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Verbose,
                    10001,
                    "b{0}");
        }

        [TestMethod()]
        public void VerboseTestFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Verbose((GenericEventId)10002,
                        "a{0}",
                        2))
                .ThenVerifyTraceInfo(TraceEventType.Verbose,
                    10002,
                    "a2");
        }

        [TestMethod()]
        public void WarningTestException()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Warning(GenericEventId.SystemWarning,
                        new ApplicationException("a1")))
                .ThenVerifyTraceInfo(TraceEventType.Warning,
                    4100,
                    "Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void WarningTestExceptionAndMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Warning(GenericEventId.SystemWarning,
                        new ApplicationException("a1"),
                        "b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Warning,
                    4100,
                    "b{0}|Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void WarningTestExceptionAndFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Warning(GenericEventId.SystemWarning,
                        new ApplicationException("a1"),
                        "b{0}",
                        2))
                .ThenVerifyTraceInfo(TraceEventType.Warning,
                    4100,
                    "b2|Exception: System.ApplicationException: a1");
        }

        [TestMethod()]
        public void WarningTestMessage()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Warning(GenericEventId.ConfigurationWarning,
                        "b{0}"))
                .ThenVerifyTraceInfo(TraceEventType.Warning,
                    4000,
                    "b{0}");
        }

        [TestMethod()]
        public void WarningTestFormatAndArgs()
        {
            GivenTestLog()
                .WhenLogAction(log => log.Warning(GenericEventId.SystemWarning,
                        "a{0}",
                        2))
                .ThenVerifyTraceInfo(TraceEventType.Warning,
                    4100,
                    "a2");
        }

        // Helper

        private TestTraceLogContext GivenTestLog()
        {
            return new TestTraceLogContext("tracelogTestSource");
        }

        class TestTraceLogContext
        {
            ITraceLog<GenericEventId> log;
            TestTraceListener listener;

            public TestTraceLogContext(string name)
            {
                log = new TraceLog<GenericEventId>(name);
                var source = log.TraceSource;
                listener = source.Listeners.OfType<TestTraceListener>().First();
                listener.MethodCallInformation.Clear();
            }

            public TestTraceLogContext WhenLogAction(Action<ITraceLog<GenericEventId>> action)
            {
                action(log);
                return this;
            }

            public void ThenVerifyTraceInfo(TraceEventType eventType, int id, string messageStart)
            {
                var info = listener.MethodCallInformation[0];
                Assert.AreEqual(eventType, info.EventType);
                Assert.AreEqual(id, info.Id);
                Console.WriteLine(info.Message);
                StringAssert.StartsWith(info.Message, messageStart);
            }

        }

    }
}