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

        // 2013-04-16 SG: Accessor missing
        //[TestMethod]
        //public void TestExtractSubject()
        //{
        //    const string content = "Something to say";
        //    const string theRest = ". the rest of the trace.";
        //    string s = MailMessageHelper_Accessor.ExtractSubject(content);
        //    Assert.IsTrue(s.StartsWith(content));

        //    s = MailMessageHelper_Accessor.ExtractSubject("2012-03-02 12:48 " + content);
        //    Assert.IsTrue(s.StartsWith(content));

        //    s = MailMessageHelper_Accessor.ExtractSubject("2012-03-02 12:48 " + content + theRest);
        //    Assert.IsTrue(s.StartsWith(content));

        //    s = MailMessageHelper_Accessor.ExtractSubject("2012-03-02 12:48:22 abcde.fg:" + content);
        //    Assert.IsTrue(s.StartsWith("abcde.fg:" + content));

        //    s = MailMessageHelper_Accessor.ExtractSubject("abcde.fg:" + content);
        //    Assert.IsTrue(s.StartsWith("abcde.fg:" + content));
        //}


        //////////////////////// Integration tests for Email functions should not be executed often.
        ///*During integration tests, it is good to have a local SMTP server installed. Windows 7 does not have one, so you may use hMailServer. External SMTP server might be subject to spam control and network issue.

        void AssertMessagesSent(int expected)
        {
            Assert.AreEqual(expected, Directory.GetFiles(pickupDirectory).Count());
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListener()
        {
            TraceSource source = new TraceSource("emailSource");

            source.TraceEvent(TraceEventType.Warning, 0, "Anything. More detail go here.");
            source.TraceEvent(TraceEventType.Error, 0, "something wrong; can you tell? more here.");
            source.TraceInformation("Default filter does not include Info.");

            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestErrorBufferTraceListener()
        {
            var source = new TraceSource("bufferedEmailSource");

            BufferedEmailTraceListener.ClearAll();

            source.TraceEvent(TraceEventType.Warning, 0, "Anythingbbbb. More detail go here.");
            source.TraceEvent(TraceEventType.Error, 0, "something wrongbbbb; can you tell? more here.");
            source.TraceInformation("Default filter does not include Info.");

            BufferedEmailTraceListener.SendAll();
            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(1);

        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListenerWithManyTraces()
        {
            TraceSource source = new TraceSource("emailSource");

            for (int i = 0; i < 1000; i++)
            {
                source.TraceEvent(TraceEventType.Warning, 0, "Anything. More detail go here.");
                source.TraceEvent(TraceEventType.Error, 0, "something wrong; can you tell? more here.");
                source.TraceInformation("Default filter does not include Info.");
            }

            System.Threading.Thread.Sleep(10000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2000);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListenerWithManyTracesInThreads()
        {
            TraceSource source = new TraceSource("emailSource");

            Action d = () =>
            {
                source.TraceEvent(TraceEventType.Warning, 0, "Anything. More detail go here.");
                source.TraceEvent(TraceEventType.Error, 0, "something wrong; can you tell? more here.");
                source.TraceInformation("Default filter does not include Info.");
            };

            for (int i = 0; i < 1000; i++)
            {
                d.BeginInvoke(null, null);
            }

            System.Threading.Thread.Sleep(10000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2000);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync()
        {
            MailMessageQueue queue = new MailMessageQueue(3);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(500);//need to wait, otherwise the test host is terminated resulting in thread abort.
            AssertMessagesSent(1);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }


        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync2()
        {
            MailMessageQueue queue = new MailMessageQueue(4);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            AssertMessagesSent(2);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }


        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsyncWithManyMessages()
        {
            const int messageCount = 1000;

            MailMessageQueue queue = new MailMessageQueue(4);//smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848
            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
            for (int i = 0; i < messageCount; i++)
            {
                queue.AddAndSendAsync(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"));
            }
            System.Threading.Thread.Sleep(2000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.
            AssertMessagesSent(messageCount);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }

    }
}
