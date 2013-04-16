using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics.Tests.Utility;
using System.Data.Common;
using System.Reflection;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class SqlDatabaseTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void HandlesEventSentDirectly()
        {
            // Test should pull our mock command off the queue
            var mockCommand1 = new MockCommand();
            MockDbFactory.Instance.CommandQueue.Clear();
            MockDbFactory.Instance.CommandQueue.Enqueue(mockCommand1);

            var listener = new SqlDatabaseTraceListener("TestProvider");

            listener.TraceEvent(null, "Source1", TraceEventType.Warning, 1, "{0}-{1}", 2, "A");

            Assert.AreEqual(1, mockCommand1.MockCommandsExecuted.Count);

            var properties = mockCommand1.MockCommandsExecuted[0];
            Assert.AreEqual("Source1", properties["@Source"]);
            Assert.AreEqual("1", properties["@Id"]);
            Assert.AreEqual("2-A", properties["@Message"]);
        }

        [TestMethod]
        public void HandlesEventFromTraceSource()
        {
            // Test should pull our mock command off the queue
            var mockCommand1 = new MockCommand();
            MockDbFactory.Instance.CommandQueue.Clear();
            MockDbFactory.Instance.CommandQueue.Enqueue(mockCommand1);

            TraceSource source = new TraceSource("sql2Source");

            source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B");

            Assert.AreEqual(1, mockCommand1.MockCommandsExecuted.Count);

            var properties = mockCommand1.MockCommandsExecuted[0];
            Assert.AreEqual("sql2Source", properties["@Source"]);
            Assert.AreEqual("2", properties["@Id"]);
            Assert.AreEqual("Warning", properties["@EventType"]);
            Assert.AreEqual("3-B", properties["@Message"]);
            Assert.AreEqual("App2", properties["@ApplicationName"]);
        }

        [TestMethod]
        public void ConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("sql2Source");
            var listener = source.Listeners.OfType<SqlDatabaseTraceListener>().First();

            Assert.AreEqual("sql2", listener.Name);
            Assert.AreEqual("TestProvider", listener.ConnectionName);
            Assert.AreEqual("Command2", listener.CommandText);
            Assert.AreEqual("App2", listener.ApplicationName);
            Assert.AreEqual(10, listener.MaxMessageLength);
        }


        // TODO: Test to check _all_ parameters work.
        
        // TODO: Test to check max message length trimming works.

    }
}
