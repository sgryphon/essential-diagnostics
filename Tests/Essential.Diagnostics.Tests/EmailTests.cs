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
        public void BufferedSendOne()
        {
            var source = new TraceSource("bufferedEmailSource");

            BufferedEmailTraceListener.ClearAll();

            source.TraceEvent(TraceEventType.Warning, 0, "Message 1");
            source.TraceEvent(TraceEventType.Error, 0, "Message 2");

            BufferedEmailTraceListener.SendAll();
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(1);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendMany()
        {
            TraceSource source = new TraceSource("emailSource");

            for (int i = 0; i < 1000; i++)
            {
                source.TraceEvent(TraceEventType.Warning, 0, "Message 1 - {0}", i);
                source.TraceEvent(TraceEventType.Error, 0, "Message 2 - {0}", i);
            }

            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2000);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void EmailSendManyThreads()
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
            System.Threading.Thread.Sleep(3000);

            AssertMessagesSent(2000);
        }

    }
}
