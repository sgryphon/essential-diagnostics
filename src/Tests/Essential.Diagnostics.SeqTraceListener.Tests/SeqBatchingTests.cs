using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class SeqBatchingTests
    {
        public TestContext TestContext { get; set; }

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
            listener.MaxRetries = 5;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message");
            // Although immediate, it is still async, so need to sleep thread
            Thread.Sleep(200);

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
            listener.MaxRetries = 5;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message 1");
            Thread.Sleep(100);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 2, "Test Message 2");
            Thread.Sleep(50);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 3, "Test Message 3");
            Thread.Sleep(50);

            // Before batch timeout, should have only received one
            Thread.Sleep(200);
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

        [TestMethod]
        public void SeqBatchSizeShouldSendImmediately()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 5;
            listener.BatchTimeout = TimeSpan.FromMilliseconds(500);
            listener.MaxRetries = 5;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message 1");
            Thread.Sleep(100);

            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 2, "Test Message 2");
            Thread.Sleep(50);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 3, "Test Message 3");
            Thread.Sleep(50);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 4, "Test Message 4");
            Thread.Sleep(50);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 5, "Test Message 5");
            Thread.Sleep(50);
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 6, "Test Message 6");
            Thread.Sleep(50);

            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 7, "Test Message 7");
            Thread.Sleep(50);

            // Before batch timeout, should have only received two
            Assert.AreEqual(2, mockRequestFactory.RequestsCreated.Count);

            // After batch timeout, should have received three requests
            Thread.Sleep(600);
            Assert.AreEqual(3, mockRequestFactory.RequestsCreated.Count);

            var request0Body = mockRequestFactory.RequestsCreated[0].RequestBody;
            Console.WriteLine(request0Body);
            StringAssert.Contains(request0Body, "Test Message 1");

            var request1Body = mockRequestFactory.RequestsCreated[1].RequestBody;
            Console.WriteLine(request1Body);
            StringAssert.Contains(request1Body, "Test Message 2");
            StringAssert.Contains(request1Body, "Test Message 6");

            var request2Body = mockRequestFactory.RequestsCreated[2].RequestBody;
            Console.WriteLine(request2Body);
            StringAssert.Contains(request2Body, "Test Message 7");

            // Let background thread finish
            Thread.Sleep(1000);
        }
    }
}
