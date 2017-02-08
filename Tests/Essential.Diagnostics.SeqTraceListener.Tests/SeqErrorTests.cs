using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class SeqErrorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SeqBatchErrorResponseShouldRetry()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.RequestTimeout, null)
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
            Thread.Sleep(100);

            // Before batch timeout, should have only received one
            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);
            var request0Body = mockRequestFactory.RequestsCreated[0].RequestBody;
            Console.WriteLine(request0Body);

            // After batch timeout, should have two requests (second got error response)
            Thread.Sleep(500);
            Assert.AreEqual(2, mockRequestFactory.RequestsCreated.Count);
            var request1Body = mockRequestFactory.RequestsCreated[1].RequestBody;
            Console.WriteLine(request1Body);

            // After retry timeout, should have three requests 
            Thread.Sleep(1200);
            Assert.AreEqual(3, mockRequestFactory.RequestsCreated.Count);
            var request2Body = mockRequestFactory.RequestsCreated[2].RequestBody;
            Console.WriteLine(request2Body);

            Assert.AreEqual(request1Body, request2Body);

            // Let background thread finish
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void SeqBatchErrorAbandonAfterMaxRetries()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.RequestTimeout, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.RequestTimeout, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.RequestTimeout, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.RequestTimeout, null)
                );
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 5;
            listener.BatchTimeout = TimeSpan.FromMilliseconds(500);
            listener.MaxRetries = 3;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            // First batch (okay)
            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message 1");
            Thread.Sleep(100);
            // Second batch (fail, plus 3 retry fails)
            listener.TraceEvent(null, "TestSource", TraceEventType.Information, 2, "Poison Message 2");
            Thread.Sleep(600);
            // Wait (above) for the batch to have started
            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 3, "Test Message 3");
            Thread.Sleep(100);

            // By now should have two requests (second got error response)
            Assert.AreEqual(2, mockRequestFactory.RequestsCreated.Count);
            var request0Body = mockRequestFactory.RequestsCreated[0].RequestBody;
            Console.WriteLine(request0Body);
            var request1Body = mockRequestFactory.RequestsCreated[1].RequestBody;
            Console.WriteLine(request1Body);

            // Three retries after 500, 1000, and 2000 ms, plus the 6th should have succeeded
            Thread.Sleep(3500);
            Assert.AreEqual(6, mockRequestFactory.RequestsCreated.Count);
            var request4Body = mockRequestFactory.RequestsCreated[4].RequestBody;
            Console.WriteLine(request4Body);
            var request5Body = mockRequestFactory.RequestsCreated[5].RequestBody;
            Console.WriteLine(request5Body);

            Assert.AreEqual(request1Body, request4Body);
            StringAssert.Contains(request0Body, "Test Message 1");
            StringAssert.Contains(request1Body, "Poison Message 2");
            StringAssert.Contains(request5Body, "Test Message 3");

            // Let background thread finish
            Thread.Sleep(1000);
        }

    }
}
