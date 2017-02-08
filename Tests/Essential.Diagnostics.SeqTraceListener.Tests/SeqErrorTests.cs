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
    }
}
