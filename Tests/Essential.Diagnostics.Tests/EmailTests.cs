using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using System.Web;
using System.Net;
using Essential.Diagnostics;
using System.ComponentModel;
using System.IO;
using Essential.Net.Mail;
using System.Threading;

namespace Essential.Diagnostics.Tests
{
    /// <summary>
    /// Summary description for TestSystem
    /// </summary>
    [TestClass]
    public class EmailTests
    {
        static string pickupDirectory;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            var smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
            pickupDirectory = (smtpConfig != null) ? smtpConfig.SpecifiedPickupDirectory.PickupDirectoryLocation : null;
            if (!String.IsNullOrEmpty(pickupDirectory))
            {
                if (!Directory.Exists(pickupDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(pickupDirectory);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(
                            string.Format("Cannot create pickup directory '{0}'; can't run tests.", pickupDirectory), ex);
                    }
                }
            }

        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (!string.IsNullOrEmpty(pickupDirectory))
            {
                ClearPickupDirectory();
            }
        }

        void ClearPickupDirectory()
        {
            string[] filePaths = Directory.GetFiles(pickupDirectory);
            foreach (string filePath in filePaths)
                File.Delete(filePath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        void AssertMessagesSent(int expected)
        {
            AssertMessagesSent(expected, null);
        }

        void AssertMessagesSent(int expected, string message)
        {
            Assert.AreEqual(expected, Directory.GetFiles(pickupDirectory).Count(), message);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendTwo()
        {
            TraceSource source = new TraceSource("emailSource");

            source.TraceEvent(TraceEventType.Warning, 0, "Message 1");
            source.TraceEvent(TraceEventType.Error, 0, "Message 2");

            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailFilterSendFiltered()
        {
            TraceSource source = new TraceSource("emailFilterSource");

            source.TraceEvent(TraceEventType.Error, 0, "Include Error.");
            source.TraceInformation("Include Info.");
            source.TraceEvent(TraceEventType.Verbose, 0, "Default filter does not include Verbose.");

            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendManyMaxUnlimited()
        {
            TraceSource source = new TraceSource("emailSource");

            for (int i = 0; i < 1000; i++)
            {
                source.TraceEvent(TraceEventType.Warning, 0, "Message 1 - {0}", i);
                source.TraceEvent(TraceEventType.Error, 0, "Message 2 - {0}", i);
            }

            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2000, "All messages should be sent as max is unlimited.");
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendManyThreadsMaxUnlimited()
        {
            TraceSource source = new TraceSource("emailSource");

            Action d = () =>
            {
                try
                {
                    var guid = Guid.NewGuid();
                    source.TraceEvent(TraceEventType.Warning, 0, "Message 1 - {0}", guid);
                    source.TraceEvent(TraceEventType.Error, 0, "Message 2 - {0}", guid);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Action exception: {0}", ex));
                }
            };

            for (int i = 0; i < 1000; i++)
            {
                d.BeginInvoke(null, null);
            }

            // Need to wait, otherwise messages haven't been sent and Assert throws exception
            System.Threading.Thread.Sleep(10000);

            AssertMessagesSent(2000, "All messages should be sent as max is unlimited.");
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailFloodTestMaxDefault50()
        {
            TraceSource source = new TraceSource("emailFloodSource");

            for (int i = 0; i < 100; i++)
            {
                source.TraceEvent(TraceEventType.Warning, 0, "Message 1 - {0}", i);
                source.TraceEvent(TraceEventType.Error, 0, "Message 2 - {0}", i);
            }

            System.Threading.Thread.Sleep(10000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(50, "Should be limited by default max traces of 50.");
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailFloodManyThreadsMax100()
        {
            TraceSource source = new TraceSource("emailFlood2Source");

            Action d = () =>
            {
                try
                {
                    var guid = Guid.NewGuid();
                    source.TraceEvent(TraceEventType.Warning, 0, "Message 1 - {0}", guid);
                    source.TraceEvent(TraceEventType.Error, 0, "Message 2 - {0}", guid);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Action exception: {0}", ex));
                }
            };

            for (int i = 0; i < 200; i++)
            {
                d.BeginInvoke(null, null);
            }

            // Need to wait, otherwise messages haven't been sent and Assert throws exception
            System.Threading.Thread.Sleep(3000);

            AssertMessagesSent(100, "Should be limited by max traces of 100.");
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendIntermittent()
        {
            TraceSource source = new TraceSource("emailSource");

            Debug.WriteLine("Expect 1");
            source.TraceEvent(TraceEventType.Warning, 0, "Message 1");
            Debug.WriteLine("Expect 2");
            source.TraceEvent(TraceEventType.Error, 0, "Message 2");
            source.TraceEvent(TraceEventType.Error, 0, "Message 3");
            source.TraceEvent(TraceEventType.Error, 0, "Message 4");

            Thread.Sleep(300);
            source.TraceEvent(TraceEventType.Warning, 0, "Message 5");
            Thread.Sleep(150);
            Debug.WriteLine("Expect 1");
            Thread.Sleep(150);
            source.TraceEvent(TraceEventType.Warning, 0, "Message 6");
            Thread.Sleep(300);
            source.TraceEvent(TraceEventType.Warning, 0, "Message 7");

            Thread.Sleep(450);
            Debug.WriteLine("Expect 0");
            Thread.Sleep(450);

            Debug.WriteLine("Expect 1");
            source.TraceEvent(TraceEventType.Warning, 0, "Message 8");
            Thread.Sleep(300);
            source.TraceEvent(TraceEventType.Warning, 0, "Message 9");
            Thread.Sleep(300);
            source.TraceEvent(TraceEventType.Warning, 0, "Message 10");

            Debug.WriteLine("Expect 2");
            source.TraceEvent(TraceEventType.Warning, 0, "Message 11");
            source.TraceEvent(TraceEventType.Warning, 0, "Message 12");

            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(12);
        }


    }
}
