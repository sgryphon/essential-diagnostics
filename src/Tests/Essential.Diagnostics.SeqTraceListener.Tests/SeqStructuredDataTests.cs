using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            var regexData = new Regex("\"Data\":");
            StringAssert.DoesNotMatch(requestBody, regexData);
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

            var structuredData = new StructuredData("{a}", new[] { 1, 2, 3 });
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":[1,2,3]");
        }

        [TestMethod]
        public void SeqStructuredChildArray()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var list1 = new ArrayList() { 1, "A" };
            var dictionary2 = new Dictionary<string, object>()
            {
                { "x", 2 },
                { "y", list1 }
            };
            var structuredData = new StructuredData("{a}", dictionary2);
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":{\"x\":2,\"y\":[1,\"A\"]}");
        }

        [TestMethod]
        public void SeqStructuredRecursiveArrayShouldStop()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var list1 = new ArrayList() { 1 };
            var list2 = new ArrayList() { 2, list1 };
            list1.Add(list2);
            var structuredData = new StructuredData("{a}|{b}", list1, list2);
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}|{b}\"");
            StringAssert.Contains(requestBody, "\"a\":[1,[2,\"System.Collections.ArrayList\"]]");
            StringAssert.Contains(requestBody, "\"b\":[2,[1,\"System.Collections.ArrayList\"]]");
        }

        [TestMethod]
        public void SeqStructuredRecursiveDictionaryShouldStop()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var dictionary1 = new Dictionary<string, object>() {
                { "A", 1 }
            };
            var dictionary2 = new Dictionary<string, object>() {
                { "X", 2 },
                { "Y", dictionary1 }
            };
            dictionary1.Add("B", dictionary2);
            var structuredData = new StructuredData("{a}|{b}", dictionary1, dictionary2);
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}|{b}\"");
            StringAssert.Contains(requestBody, "\"a\":{\"A\":1,\"B\":{\"X\":2,\"Y\":\"System.Collections.Generic.Dictionary`2[System.String,System.Object]\"}}");
            StringAssert.Contains(requestBody, "\"b\":{\"X\":2,\"Y\":{\"A\":1,\"B\":\"System.Collections.Generic.Dictionary`2[System.String,System.Object]\"}}");
        }

        [TestMethod]
        public void SeqHandlesStructuredDictionary()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var dictionaryData = new Dictionary<string, object>() {
                { "MessageTemplate", "{a}" },
                { "a",  1 }
            };
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, dictionaryData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{a}\"");
            StringAssert.Contains(requestBody, "\"a\":1");
            //var regexTemplateData = new Regex("\"MessageTemplate\":\"{Data}\"");
            //StringAssert.DoesNotMatch(requestBody, regexTemplateData);
            var regexData = new Regex("\"Data\":");
            StringAssert.DoesNotMatch(requestBody, regexData);
        }

        [TestMethod]
        public void SeqIgnoresStructuredDictionaryWhenTurnedOff()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;
            listener.ProcessDictionaryData = false;

            var dictionaryData = new Dictionary<string, object>() {
                { "MessageTemplate", "{a}" },
                { "a",  1 }
            };
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, dictionaryData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            var regexTemplateData = new Regex("\"MessageTemplate\":\"{Data}\"");
            StringAssert.Matches(requestBody, regexTemplateData);
            var regexData = new Regex("\"Data\":");
            StringAssert.Matches(requestBody, regexData);
        }

        [TestMethod]
        public void SeqHandlesStructuredOperationAsTraceOutput()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;
            listener.TraceOutputOptions = TraceOptions.LogicalOperationStack;

            var structuredData = new StructuredData(new Dictionary<string, object>() { { "a", 1 } });
            Trace.CorrelationManager.StartLogicalOperation(structuredData);
            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "x{0}", "y");
            Trace.CorrelationManager.StopLogicalOperation();

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"x{0}\"");
            StringAssert.Contains(requestBody, "\"0\":\"y\"");
            StringAssert.Contains(requestBody, "\"LogicalOperationStack\":[{\"a\":1}]");
            var regexData = new Regex("\"Data\":");
            StringAssert.DoesNotMatch(requestBody, regexData);
        }

        [TestMethod]
        public void SeqHandlesStructuredOperationAsPropertiesOnly()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var structuredData = new StructuredData(new Dictionary<string, object>() { { "a", 1 } });
            Trace.CorrelationManager.StartLogicalOperation(structuredData);
            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "x{0}", "y");
            Trace.CorrelationManager.StopLogicalOperation();

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"x{0}\"");
            StringAssert.Contains(requestBody, "\"0\":\"y\"");
            StringAssert.Contains(requestBody, "\"a\":1");
            var regexData = new Regex("\"Data\":");
            StringAssert.DoesNotMatch(requestBody, regexData);
            var regexStack = new Regex("\"LogicalOperationStack\":");
            StringAssert.DoesNotMatch(requestBody, regexStack);
        }

        [TestMethod]
        public void SeqHandlesDictionaryOperationAsProperties()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var dictionaryData = new Dictionary<string, object>() { { "a", 1 } };
            Trace.CorrelationManager.StartLogicalOperation(dictionaryData);
            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "x{0}", "y");
            Trace.CorrelationManager.StopLogicalOperation();

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"x{0}\"");
            StringAssert.Contains(requestBody, "\"0\":\"y\"");
            StringAssert.Contains(requestBody, "\"a\":1");
            var regexData = new Regex("\"Data\":");
            StringAssert.DoesNotMatch(requestBody, regexData);
            var regexStack = new Regex("\"LogicalOperationStack\":");
            StringAssert.DoesNotMatch(requestBody, regexStack);
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

        [TestMethod]
        public void SeqDestructureCustomObject()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.BatchSize = 0;
            listener.BatchSender.HttpWebRequestFactory = mockRequestFactory;

            var testObject = new TestObject() { X = 1.2, Y = 3.4 };
            var structuredData = new StructuredData("{@a}", testObject);
            listener.TraceData(null, "TestSource", TraceEventType.Warning, 1, structuredData);

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "\"MessageTemplate\":\"{@a}\"");
            StringAssert.Contains(requestBody, "\"a\":{\"X\":1.2,\"Y\":3.4}");
        }

        class TestObject
        {
            public double X { get; set; }

            public double Y { get; set; }

            public override string ToString()
            {
                return @"w=x\y'z";
            }
        }
    }
}
