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
        public void BasicFormatTest()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            string source = "test"; 
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "{Id}.{Message}";
            string expected = "5.fnord";

            var actual = traceFormatter.Format(template, eventCache, source, eventType, id,
                message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void BasicContextTest()
        {
            var traceFormatter = new TraceFormatter();
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
                actual = traceFormatter.Format(template, eventCache, source, eventType, id,
                    message, relatedActivityId, data);
            }
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ProcessContextTest()
        {
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "{ProcessId}";
            string expected = Process.GetCurrentProcess().Id.ToString();

            var actual = traceFormatter.Format(template, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void HttpContextTest()
        {
            var mockHttpTraceContext  =new MockHttpTraceContext();
            mockHttpTraceContext.RequestUrl = new Uri("http://test/x");
            var traceFormatter = new TraceFormatter();
            traceFormatter.HttpTraceContext = mockHttpTraceContext;
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "|{RequestUrl}|";
            string expected = "|http://test/x|";

            var actual = traceFormatter.Format(template, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void EmptyHttpContextTest()
        {
            // The default is HttpContext.Current, which should be empty when running unit test
            var traceFormatter = new TraceFormatter();
            TraceEventCache eventCache = null;
            string source = "test";
            TraceEventType eventType = TraceEventType.Warning;
            int id = 5;
            string message = "fnord";
            Guid? relatedActivityId = null;
            object[] data = null;
            string template = "|{RequestUrl}|";
            string expected = "||";

            var actual = traceFormatter.Format(template, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

            Assert.AreEqual(expected, actual);
        }

    }
}
