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
        public void SendsEventsToDatabase()
        {
            var table = DbProviderFactories.GetFactoryClasses();
            var providerRow = table.Rows.Find("Essential.Diagnostics.Tests.MockDbProvider");
            var typeName = providerRow["AssemblyQualifiedName"].ToString();
            Type type = Type.GetType(typeName);
            FieldInfo field = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if ((null != field) && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
            {
                object instance = field.GetValue(null);
            }

            // Test should pull our mock command off the queue
            var mockCommand1 = new MockCommand();
            MockDbFactory.Instance.CommandQueue.Enqueue(mockCommand1);

            var listener = new SqlDatabaseTraceListener("TestProvider");

            listener.TraceEvent(null, "Source", TraceEventType.Warning, 1, "{0}-{1}", 2, "A");

            Assert.AreEqual(1, mockCommand1.MockCommandsExecuted.Count);
            var properties = mockCommand1.MockCommandsExecuted[0];
            Assert.AreEqual("Source", properties["@TraceSource"]);
            Assert.AreEqual("1", properties["@EventId"]);
            Assert.AreEqual("2-A", properties["@MessageText"]);
        }

    }
}
