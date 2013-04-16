using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class InMemoryTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void RecordsTraceEventSentDirectly()
        {
            var listener = new InMemoryTraceListener();

            listener.TraceEvent(null, "Source", TraceEventType.Warning, 1, "{0}-{1}", 2, "A");

            var events = listener.GetEvents();
            Assert.AreEqual(1, events.Length);

            Assert.AreEqual("Source", events[0].Source);
            Assert.AreEqual(1, events[0].Id);
            Assert.AreEqual("2-A", events[0].Message);
        }

        [TestMethod]
        public void OverwriteWhenMoreTracesThanLimit()
        {
            var listener = new InMemoryTraceListener(6);

            for (var count = 1; count <= 15; count++)
            {
                listener.TraceEvent(null, "Source", TraceEventType.Warning, count, "");
            }

            var events = listener.GetEvents();
            Assert.AreEqual(6, events.Length);

            Assert.AreEqual(10, events[0].Id);
            Assert.AreEqual(15, events[5].Id);
        }

        [TestMethod]
        public void RecordsEventFromTraceSource()
        {
            TraceSource source = new TraceSource("inmemory1Source");

            // Note: If you don't <clear />, then DefaultTraceListener is automatically included
            foreach (TraceListener l in source.Listeners)
            {
                Console.WriteLine("{0} ({1})", l.Name, l.GetType().Name);
            }

            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B");

            var events = listener.GetEvents();
            Assert.AreEqual(1, events.Length);

            Assert.AreEqual("inmemory1Source", events[0].Source);
            Assert.AreEqual(2, events[0].Id);
            Assert.AreEqual("3-B", events[0].Message);
        }

        [TestMethod]
        public void ResetClearsAndResetsPointer()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener.Clear();

            source.TraceEvent(TraceEventType.Warning, 1, "A");
            var events1 = listener.GetEvents();
            Assert.AreEqual(1, events1.Length);
            Assert.AreEqual(1, events1[0].Id);

            listener.Clear();
            var events2 = listener.GetEvents();
            Assert.AreEqual(0, events2.Length);

            source.TraceEvent(TraceEventType.Warning, 2, "B");
            var events3 = listener.GetEvents();
            Assert.AreEqual(1, events3.Length);
            Assert.AreEqual(2, events3[0].Id);
        }

        [TestMethod]
        public void ConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("inmemory1Source");

            var listener = source.Listeners.OfType<InMemoryTraceListener>().First();

            Assert.AreEqual("inmemory1", listener.Name);
            Assert.AreEqual(10, listener.Limit);
        }

        [TestMethod]
        public void InitializeDataRefreshCorrectly()
        {
            TraceSource source = new TraceSource("inmemory1Source");
            var listener1 = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener1.Clear();
            source.TraceEvent(TraceEventType.Warning, 1, "A");

            // Verify values are correct before changing
            Assert.AreEqual(10, listener1.Limit);
            Assert.AreEqual(1, listener1.GetEvents().Length);

            var configPath = ConfigUtility.GetConfigDirFromTestRunDirectory(TestContext.TestRunDirectory);

            using (var file = new FileResetScope(configPath))
            {
                var doc = XDocument.Parse(file.OriginalText);
                var configuration = doc.Root; //.Element("configuration");
                var systemDiagnostics = configuration.Element("system.diagnostics");
                var sharedListeners = systemDiagnostics.Element("sharedListeners");
                var listenerConfig = sharedListeners.Elements().First(x => x.Attribute("name").Value == "inmemory1");

                listenerConfig.SetAttributeValue("initializeData", "20");

                doc.Save(configPath);
                Trace.Refresh();

                var listener2 = source.Listeners.OfType<InMemoryTraceListener>().First();
                Assert.AreEqual(20, listener2.Limit);
                Assert.AreEqual(0, listener2.GetEvents().Length);
                Assert.AreNotSame(listener1, listener2);
            }
        }

    }
}
