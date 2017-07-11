using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class ColoredConsoleTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ColoredCorrectDefaultCriticalEventColor()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Critical, 1);

            Assert.AreEqual(ConsoleColor.Red, mockConsole.ForegroundColorSet[0]);
            Assert.AreEqual(1, mockConsole.ResetColorCount);
        }

        [TestMethod]
        public void ColoredCorrectDefaultErrorEventColor()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Error, 1);

            Assert.AreEqual(ConsoleColor.Red, mockConsole.ForegroundColorSet[0]);
            Assert.AreEqual(1, mockConsole.ResetColorCount);
        }

        [TestMethod]
        public void ColoredCorrectDefaultWarningEventColor()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Warning, 1);

            Assert.AreEqual(ConsoleColor.Yellow, mockConsole.ForegroundColorSet[0]);
            Assert.AreEqual(1, mockConsole.ResetColorCount);
        }

        [TestMethod]
        public void ColoredCorrectDefaultInformationEventColor()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Information, 1);

            Assert.AreEqual(ConsoleColor.Gray, mockConsole.ForegroundColorSet[0]);
            Assert.AreEqual(1, mockConsole.ResetColorCount);
        }

        [TestMethod]
        public void ColoredCorrectDefaultVerboseEventColor()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Verbose, 1);

            Assert.AreEqual(ConsoleColor.DarkCyan, mockConsole.ForegroundColorSet[0]);
            Assert.AreEqual(1, mockConsole.ResetColorCount);
        }

        [TestMethod]
        public void ColoredCorrectMessageWritten()
        {
            var mockConsole = new MockConsole();
            var listener = new ColoredConsoleTraceListener();
            listener.Console = mockConsole;

            listener.TraceEvent(null, "source", TraceEventType.Information, 1, "{0}-{1}", 2, "A");

            var output = mockConsole.OutWriter.ToString().Trim();
            Assert.AreEqual("source Information: 1 : 2-A", output);
        }

        [TestMethod]
        public void ColoredConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("colored1Source");
            var listener = source.Listeners.OfType<ColoredConsoleTraceListener>().First();

            Assert.AreEqual("colored1", listener.Name);
            Assert.AreEqual("{DateTime} {EventType}: {Message}", listener.Template);
            Assert.AreEqual(ConsoleColor.DarkBlue, listener.GetConsoleColor(TraceEventType.Critical));
            Assert.AreEqual(ConsoleColor.DarkGreen, listener.GetConsoleColor(TraceEventType.Error));
            Assert.AreEqual(ConsoleColor.DarkCyan, listener.GetConsoleColor(TraceEventType.Warning));
            Assert.AreEqual(ConsoleColor.DarkRed, listener.GetConsoleColor(TraceEventType.Information));
            Assert.AreEqual(ConsoleColor.Gray, listener.GetConsoleColor(TraceEventType.Verbose));
            Assert.AreEqual(ConsoleColor.DarkGray, listener.GetConsoleColor(TraceEventType.Start));
            Assert.AreEqual(ConsoleColor.Blue, listener.GetConsoleColor(TraceEventType.Transfer));
        }


        [TestMethod]
        public void ColoredOtherConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("colored2Source");
            var listener = source.Listeners.OfType<ColoredConsoleTraceListener>().First();

            Assert.AreEqual("colored2", listener.Name);
            Assert.IsTrue(listener.UseErrorStream);
            Assert.IsTrue(listener.ConvertWriteToEvent);
            Assert.AreEqual(ConsoleColor.DarkBlue, listener.GetConsoleColor(TraceEventType.Start));
            Assert.AreEqual(ConsoleColor.DarkGreen, listener.GetConsoleColor(TraceEventType.Stop));
            Assert.AreEqual(ConsoleColor.DarkCyan, listener.GetConsoleColor(TraceEventType.Suspend));
            Assert.AreEqual(ConsoleColor.DarkRed, listener.GetConsoleColor(TraceEventType.Resume));
        }

        [TestMethod]
        public void ColoredConfigParametersRefreshCorrectly()
        {
            TraceSource source = new TraceSource("colored1Source");
            var listener1 = source.Listeners.OfType<ColoredConsoleTraceListener>().First();

            // Verify values are correct before changing
            Assert.AreEqual("{DateTime} {EventType}: {Message}", listener1.Template);
            Assert.AreEqual(ConsoleColor.DarkBlue, listener1.GetConsoleColor(TraceEventType.Critical));
            Assert.IsFalse(listener1.ConvertWriteToEvent);

            var configPath = ConfigUtility.GetConfigDirFromTestRunDirectory(TestContext.TestRunDirectory);
            Debug.WriteLine("configPath=" + configPath);

            try
            {
                using (var file = new FileResetScope(configPath))
                {
                    var doc = XDocument.Parse(file.OriginalText);
                    var configuration = doc.Root; //.Element("configuration");
                    var systemDiagnostics = configuration.Element("system.diagnostics");
                    var sharedListeners = systemDiagnostics.Element("sharedListeners");
                    var listenerConfig = sharedListeners.Elements().First(x => x.Attribute("name").Value == "colored1");

                    Assert.AreEqual("{DateTime} {EventType}: {Message}", listenerConfig.Attribute("template").Value, "config not loaded correctly.");

                    listenerConfig.SetAttributeValue("template", "TEST {Message}");
                    listenerConfig.SetAttributeValue("criticalColor", ConsoleColor.Cyan);
                    listenerConfig.SetAttributeValue("convertWriteToEvent", true);

                    doc.Save(configPath);

                    Trace.Refresh();

                    source = new TraceSource("colored1Source");
                    var listener2 = source.Listeners.OfType<ColoredConsoleTraceListener>().First();
                    Assert.AreEqual("TEST {Message}", listener2.Template);
                    Assert.AreEqual(ConsoleColor.Cyan, listener2.GetConsoleColor(TraceEventType.Critical));
                    Assert.IsTrue(listener2.ConvertWriteToEvent);
                }
            }
            finally
            {
                // Refresh after file is reset
                Trace.Refresh();
            }
        }

        [TestMethod]
        public void ColoredInitializeDataRefreshCorrectly()
        {
            TraceSource source = new TraceSource("colored1Source");
            var listener1 = source.Listeners.OfType<ColoredConsoleTraceListener>().First();

            // Verify values are correct before changing
            Assert.IsFalse(listener1.UseErrorStream);

            var configPath = ConfigUtility.GetConfigDirFromTestRunDirectory(TestContext.TestRunDirectory);

            using (var file = new FileResetScope(configPath))
            {
                var doc = XDocument.Parse(file.OriginalText);
                var configuration = doc.Root; //.Element("configuration");
                var systemDiagnostics = configuration.Element("system.diagnostics");
                var sharedListeners = systemDiagnostics.Element("sharedListeners");
                var listenerConfig = sharedListeners.Elements().First(x => x.Attribute("name").Value == "colored1");

                listenerConfig.SetAttributeValue("initializeData", true);

                doc.Save(configPath);
                Trace.Refresh();

                var listener2 = source.Listeners.OfType<ColoredConsoleTraceListener>().First();
                Assert.IsTrue(listener2.UseErrorStream);
                Assert.AreNotSame(listener1, listener2);
            }
        }

    }
}
