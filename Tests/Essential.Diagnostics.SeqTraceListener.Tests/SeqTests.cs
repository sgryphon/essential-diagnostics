using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Essential.Diagnostics.Tests.Utility;
using System.Net;
using System.Threading;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class SeqTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SeqHandlesBasicEvent()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message");

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual("application/json; charset=utf-8", request.ContentType);
            Assert.AreEqual("http://testuri/api/events/raw", request.Uri);
            Assert.AreEqual(0, request.Headers.Count);

            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "TestSource");
            StringAssert.Contains(requestBody, "Test Message");
        }

        [TestMethod]
        public void SeqHandlesEventFromTraceSource()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            TraceSource source = new TraceSource("seq1Source");
            var listener = source.Listeners.OfType<SeqTraceListener>().First();
            listener.HttpWebRequestFactory = mockRequestFactory;

            source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B");

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual("application/json; charset=utf-8", request.ContentType);
            Assert.AreEqual("http://127.0.0.1:5341/api/events/raw", request.Uri);
            Assert.AreEqual(1, request.Headers.Count);

            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "seq1Source");
            StringAssert.Contains(requestBody, "{0}-{1}");
        }

        [TestMethod]
        public void SeqConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("seq2Source");
            var listener = source.Listeners.OfType<SeqTraceListener>().First();

            Assert.AreEqual("seq2", listener.Name);
            Assert.AreEqual("http://localhost:5341", listener.ServerUrl);
            Assert.AreEqual("12345", listener.ApiKey);
            Assert.AreEqual(6789, listener.BatchSize);
            Assert.AreEqual(2345, listener.BatchTimeout.TotalMilliseconds);

            Assert.AreEqual(TraceOptions.ThreadId, listener.TraceOutputOptions & TraceOptions.ThreadId);
        }

        [TestMethod]
        public void SeqTraceOptionsAndAdditionalProperties()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            TraceSource source = new TraceSource("seq3Source");
            var listener = source.Listeners.OfType<SeqTraceListener>().First();
            listener.HttpWebRequestFactory = mockRequestFactory;

            source.TraceEvent(TraceEventType.Information, 1, "TestMessage");

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];

            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "ThreadId");
            StringAssert.Contains(requestBody, "ProcessId");
            StringAssert.Contains(requestBody, "MachineName");
        }

        // TODO: Test to check _all_ parameters work.

        // TODO: Test to check max message length trimming works.


        [TestMethod]
        public void SeqBatchFirstMessageSentImmediately()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 5;
            listener.BatchTimeout = TimeSpan.FromMilliseconds(500);
            listener.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message");
            // Although immediate, it is still async, so need to sleep thread
            Thread.Sleep(10);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            // Let background thread finish
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void SeqBatchSecondMessageDelayedByBatchTimeout()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 5;
            listener.BatchTimeout = TimeSpan.FromMilliseconds(500);
            listener.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message 1");
            Thread.Sleep(10);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 2, "Test Message 2");
            Thread.Sleep(10);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 3, "Test Message 3");
            Thread.Sleep(10);

            // Before batch timeout, should have only received one
            Thread.Sleep(400);
            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            // After batch timeout, should have received two requests
            Thread.Sleep(200);
            Assert.AreEqual(2, mockRequestFactory.RequestsCreated.Count);

            var request0Body = mockRequestFactory.RequestsCreated[0].RequestBody;
            Console.WriteLine(request0Body);
            StringAssert.Contains(request0Body, "Test Message 1");

            var request1Body = mockRequestFactory.RequestsCreated[1].RequestBody;
            Console.WriteLine(request1Body);
            StringAssert.Contains(request1Body, "Test Message 2");
            StringAssert.Contains(request1Body, "Test Message 3");

            // Let background thread finish
            Thread.Sleep(1000);
        }


    }
}