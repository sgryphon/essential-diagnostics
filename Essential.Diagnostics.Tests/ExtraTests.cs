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
using Rnwood.SmtpServer;
using System.IO;

namespace Essential.Diagnostics.Tests
{
    /// <summary>
    /// Summary description for TestSystem
    /// </summary>
    [TestClass]
    public class TestSystem
    {
        public TestSystem()
        {
            smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
            mockSmtpPort = smtpConfig.Network.Port;

            pickupDirectory = (smtpConfig != null) ? smtpConfig.SpecifiedPickupDirectory.PickupDirectoryLocation : null;
            if (!String.IsNullOrEmpty(pickupDirectory))
            {
                if (!Directory.Exists(pickupDirectory))
                {
                    Directory.CreateDirectory(pickupDirectory);
                }
            }
        }

       string pickupDirectory;

       System.Net.Configuration.SmtpSection smtpConfig;
        int mockSmtpPort = 9999;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {

        }

        #endregion

        [TestMethod]
        public void TestExtractSubject()
        {
            const string content = "Something to say";
            const string theRest = ". the rest of the trace.";
            string s = EmailUtility_Accessor.ExtractSubject(content);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailUtility_Accessor.ExtractSubject("2012-03-02 12:48 " + content);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailUtility_Accessor.ExtractSubject("2012-03-02 12:48 " + content + theRest);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailUtility_Accessor.ExtractSubject("2012-03-02 12:48:22 abcde.fg:" + content);
            Assert.IsTrue(s.StartsWith("abcde.fg:" + content));

            s = EmailUtility_Accessor.ExtractSubject("abcde.fg:" + content);
            Assert.IsTrue(s.StartsWith("abcde.fg:" + content));


        }


        //////////////////////// Integration tests for Email functions should not be executed often.
        ///*During integration tests, it is good to have a local SMTP server installed. Windows 7 does not have one, so you may use hMailServer. External SMTP server might be subject to spam control and network issue.



        void AssertMessagesSent(int expected)
        {
            Assert.AreEqual(expected, Directory.GetFiles(pickupDirectory).Count());
        }

        void ClearPickupDirectory()
        {
            string[] filePaths = Directory.GetFiles(pickupDirectory);
            foreach (string filePath in filePaths)
                File.Delete(filePath);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListener()
        {
            ClearPickupDirectory();


            Trace.TraceWarning("Anything. More detail go here.");
            Trace.TraceError("something wrong; can you tell? more here.");
            Trace.WriteLine("This is writeline.", "Category");
            Trace.WriteLine("This is another writeline.", "caTegory");
            Trace.WriteLine("Writeline without right category", "CCCC");
            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestErrorBufferTraceListener()
        {
            ClearPickupDirectory();

            Trace.Listeners.Remove("emailTraceListener");//otherwise this listener will send mail as well

            ErrorBufferEmailTraceListener.Clear();
            Trace.TraceWarning("Anythingbbbb. More detail go here.");
            Trace.TraceError("something wrongbbbb; can you tell? more here.");
            ErrorBufferEmailTraceListener.SendMailOfEventMessages();
            AssertMessagesSent(1);

            Trace.Refresh();//so reload all listeners
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListenerWithManyTraces()
        {
            ClearPickupDirectory();

            for (int i = 0; i < 1000; i++)
            {
                Trace.TraceWarning("Anything. More detail go here.");
                Trace.TraceError("something wrong; can you tell? more here.");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.WriteLine("This is another writeline.", "caTegory");
                Trace.WriteLine("Writeline without right category", "CCCC");
            }

            System.Threading.Thread.Sleep(10000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            AssertMessagesSent(2000);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync()
        {
            ClearPickupDirectory();
            MailMessageQueue queue = new MailMessageQueue(3);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(500);//need to wait, otherwise the test host is terminated resulting in thread abort.
            AssertMessagesSent(1);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }


        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync2()
        {
            ClearPickupDirectory();
            MailMessageQueue queue = new MailMessageQueue(4);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowMail.com", "HelloAsync", "are you there? async"));
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
            ClearPickupDirectory();

            MailMessageQueue queue = new MailMessageQueue(4);//smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848
            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
            for (int i = 0; i < messageCount; i++)
            {
                queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            }
            System.Threading.Thread.Sleep(2000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.
            AssertMessagesSent(messageCount);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }



        //   Integration tests end                */
    }
}
