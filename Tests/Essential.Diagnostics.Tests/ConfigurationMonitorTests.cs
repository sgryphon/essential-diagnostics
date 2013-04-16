using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Essential.Diagnostics.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class ConfigurationMonitorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConfigFileChangeTriggersReload()
        {
            //var configPath = Assembly.GetExecutingAssembly().Location + ".config";
            //var configPath = @"D:\Code\Diagnostics\EssentialDiagnostics\Essential.Diagnostics.Tests\bin\Debug\Essential.Diagnostics.Tests.dll.config";
            var configPath = ConfigUtility.GetConfigDirFromTestRunDirectory(TestContext.TestRunDirectory);
            Console.WriteLine("Config path: '{0}'", configPath);

            TraceSource source = new TraceSource("inmemory2Source");
            var listener1 = source.Listeners.OfType<InMemoryTraceListener>().First();
            listener1.Clear();

            // Verify starting settings
            source.TraceEvent(TraceEventType.Information, 1, "A");
            var events1 = listener1.GetEvents();
            Assert.AreEqual(666, listener1.Limit);
            Assert.AreEqual(1, events1.Length);

            using (var file = new FileResetScope(configPath))
            {
                using (var configMonitor = new TraceConfigurationMonitor(configPath, false))
                {
                    configMonitor.Start();

                    var changedConfigText = file.OriginalText.Replace("666", "777");
                    File.WriteAllText(configPath, changedConfigText);

                    // Allow time for async watcher to trigger
                    Thread.Sleep(100);

                    source.TraceEvent(TraceEventType.Information, 1, "B");

                    var listener2 = source.Listeners.OfType<InMemoryTraceListener>().First();
                    var events2 = listener2.GetEvents();
                    System.Threading.Thread.Sleep(500);
                    Assert.AreEqual(777, listener2.Limit);
                    Assert.AreEqual(1, events2.Length);
                }
            }
        }
    }
}
