using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Essential.Diagnostics.Tests.Utility;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class TraceFormatterTest
    {
        [TestMethod()]
        public void FormatIdAndMessageTest()
        {
            var traceFormatter = new TraceFormatter();
            TraceListener listener = null;
            TraceEventCache eventCache = null;
            string source = "test"; 
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "{Id}.{Message}";
            string expected = "5.fnord";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatPrincipalNameTest()
        {
            var traceFormatter = new TraceFormatter();
            TraceListener listener = null;
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "{Id}.{PrincipalName}";
            string expected = "5.testuser";
            string actual = null;

            using (var scope = new UserResetScope("testuser"))
            {
                actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                    message, relatedActivityId, data);
            }
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatProcessIdTest()
        {
            var traceFormatter = new TraceFormatter();
            TraceListener listener = null;
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "{ProcessId}";
            string expected = Process.GetCurrentProcess().Id.ToString();

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatHttpRequestUrlTest()
        {
            var mockHttpTraceContext  =new MockHttpTraceContext();
            mockHttpTraceContext.RequestUrl = new Uri("http://test/x");
            var traceFormatter = new TraceFormatter();
            traceFormatter.HttpTraceContext = mockHttpTraceContext;
            TraceListener listener = null;
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "|{RequestUrl}|";
            string expected = "|http://test/x|";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatEmptyHttpContextTest()
        {
            // The default is HttpContext.Current, which should be empty when running unit test
            var traceFormatter = new TraceFormatter();
            TraceListener listener = null;
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "|{RequestUrl}|";
            string expected = "||";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatMessagePrefixAll()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            TraceListener listener = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 0;
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "<{MessagePrefix}>";
            string message = "Something to say";

            string expected = "<Something to say>";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatMessagePrefixSentinel()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            TraceListener listener = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 0;
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "<{MessagePrefix}>";
            string message = "Something to say. the rest of the trace.";
            string expected = "<Something to say>";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FormatMessagePrefixLength()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            TraceListener listener = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 0;
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "<{MessagePrefix}>";
            //                1234567890123456789012345678901234567890
            string message = "Something to say Something to say Something to say. the rest of the trace.";
            string expect = "<Something to say Something to say Som...>";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expect, actual);
        }

        [TestMethod()]
        public void FormatMessagePrefixControlCharacter()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            TraceListener listener = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 0;
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "<{MessagePrefix}>";
            string message = "Something to\tsay";

            string expected = "<Something tosay>";

            var actual = traceFormatter.Format(template, listener, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

    }
}
