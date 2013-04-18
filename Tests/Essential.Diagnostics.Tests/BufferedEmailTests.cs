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
    public class BufferedEmailTests
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

    }
}
