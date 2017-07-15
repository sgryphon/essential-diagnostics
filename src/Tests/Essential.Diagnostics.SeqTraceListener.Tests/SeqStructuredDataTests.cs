using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class SeqStructuredDataTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SeqHandlesStructuredDataMessage()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData("{a}", 1);
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":1");
        }

        [TestMethod]
        public void SeqHandlesStructuredDataProperties()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData(new Dictionary<string, object>() { { "a", 1 } }, "{a}");
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":1");
            var regexEvent1 = new Regex("\"EventId\":1");
            StringAssert.Matches(requestBody, regexEvent1);
        }

        [TestMethod]
        public void SeqStructuredTraceOptionsInMessage()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.TraceOutputOptions = TraceOptions.LogicalOperationStack;
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData("x{LogicalOperationStack}");
            try
            {
                Trace.CorrelationManager.StartLogicalOperation("X");
                listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);
            }
            finally
            {
                Trace.CorrelationManager.StopLogicalOperation();
            }

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"x{LogicalOperationStack}\"");

            var regexStackX = new Regex("\"LogicalOperationStack\":\\[\"X\"\\]");
            StringAssert.Matches(requestBody, regexStackX);
        }

        [TestMethod]
        public void SeqStructuredDataOverrides()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.TraceOutputOptions = TraceOptions.LogicalOperationStack;
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData(new Dictionary<string, object>() { { "LogicalOperationStack", "A" } });
            try
            {
                Trace.CorrelationManager.StartLogicalOperation("X");
                listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);
            }
            finally
            {
                Trace.CorrelationManager.StopLogicalOperation();
            }

            // NOTE: Not sure if structured data should override trace options or the other way around.
            // Structured Data override -- allows per-trace specific values
            // Trace Option override -- allows dummy value to be passed in with templateMessage and then overriden by listener
            //                          (there are work arounds; e.g. pass property collection instead of template values)

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "TestSource");
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"\"");

            var regexStackA = new Regex("\"LogicalOperationStack\":\"A\"");
            StringAssert.Matches(requestBody, regexStackA);
            var regexStackX = new Regex("\"LogicalOperationStack\":\\[\"X\"\\]");
            StringAssert.DoesNotMatch(requestBody, regexStackX);
        }

        [TestMethod]
        public void SeqStructuredDataDoNotOverrideReserved()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData(new Dictionary<string, object>() { { "EventId", "A" } });
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "TestSource");
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"\"");

            var regexEvent1 = new Regex("\"EventId\":1");
            StringAssert.Matches(requestBody, regexEvent1);
            var regexEventA = new Regex("\"EventId\":\"A\"");
            StringAssert.DoesNotMatch(requestBody, regexEventA);
        }

        [TestMethod]
        public void SeqStructuredArray()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData("{a}", new [] { 1, 2, 3 });
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":[1,2,3]");
        }

        [TestMethod]
        public void SeqStructuredCustomObject()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData("{a}", new TestObject());
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            // Note that the '\' is encoded as '\\'
            StringAssert.Contains(requestBody, @"""a"":""w=x\\y'z""");
        }

        class TestObject
        {
            public override string ToString()
            {
                return @"w=x\y'z";
            }
        }
    }
}
