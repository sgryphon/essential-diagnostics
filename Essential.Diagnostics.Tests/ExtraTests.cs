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
            //
            // TODO: Add constructor logic here
            //
        }

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
            //  EmailTraceDestination dic = new EmailTraceDestination();
            //  dic.Add("Category", new string[] { "zijian.huang@dealersolutions.com.au" });
            //  EmailExTraceListener.AssignAddressbook(dic);
            //ErrorBufferExTraceListener.AssignAddressbook(dic);
            //ErrorXmlBufferTraceListener.AssignAddressbook(dic);
            //ErrorXmlBufferTraceListener.DefineCurrentPath("Campaign 1/AdGroup 2", "Good to hear");

        }

        #endregion


        [TestMethod]
        public void TestExtractSubject()
        {
            const string content = "Something to say";
            const string theRest = ". the rest of the trace.";
            string s = EmailTraceListenerBase_Accessor.ExtractSubject(content);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailTraceListenerBase_Accessor.ExtractSubject("2012-03-02 12:48 " + content);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailTraceListenerBase_Accessor.ExtractSubject("2012-03-02 12:48 " + content + theRest);
            Assert.IsTrue(s.StartsWith(content));

            s = EmailTraceListenerBase_Accessor.ExtractSubject("2012-03-02 12:48:22 abcde.fg:" + content);
            Assert.IsTrue(s.StartsWith("abcde.fg:" + content));

            s = EmailTraceListenerBase_Accessor.ExtractSubject("abcde.fg:" + content);
            Assert.IsTrue(s.StartsWith("abcde.fg:" + content));


        }


        //////////////////////// Integration tests for Email functions should not be executed often.
        ///*During integration tests, it is good to have a local SMTP server installed. Windows 7 does not have one, so you may use hMailServer. External SMTP server might be subject to spam control and network issue.

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListener()
        {
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(mockSmtpPort);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);

            Trace.TraceWarning("Anything. More detail go here.");
            Trace.TraceError("something wrong; can you tell? more here.");
            Trace.WriteLine("This is writeline.", "Category");
            Trace.WriteLine("This is another writeline.", "caTegory");
            Trace.WriteLine("Writeline without right category", "CCCC");
            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            server.Stop();
            Assert.AreEqual(2, messages.Count);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestEmailTraceListenerWithManyTraces()
        {
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(mockSmtpPort);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);

            for (int i = 0; i < 1000; i++)
            {
                Trace.TraceWarning("Anything. More detail go here.");
                Trace.TraceError("something wrong; can you tell? more here.");
                Trace.WriteLine("This is writeline.", "Category");
                Trace.WriteLine("This is another writeline.", "caTegory");
                Trace.WriteLine("Writeline without right category", "CCCC");
            }

            System.Threading.Thread.Sleep(10000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            server.Stop();
            Assert.AreEqual(2000, messages.Count);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestErrorBufferTraceListener()
        {
            Trace.TraceWarning("Anythingbbbb. More detail go here.");
            Trace.TraceError("something wrongbbbb; can you tell? more here.");
            ErrorBufferTraceListener.SendMailOfEventMessages();
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync()
        {
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(Ports.AssignAutomatically);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);

            MailMessageQueue queue = new MailMessageQueue("localhost", server.PortNumber, 3);
            // queue.AddAndSendAsync(new System.Net.Mail.MailMessage("testfrom@fonlowmail.com", "testto@fonlowmail.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            server.Stop();
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }

        const int mockSmtpPort = 9999;
        //I know I may read it from config, as suggested in http://stackoverflow.com/questions/625262/access-system-net-settings-from-app-config-programmatically-in-c-sharp, but I think the code is too long.

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsync2()
        {
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(Ports.AssignAutomatically);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);     

            MailMessageQueue queue = new MailMessageQueue("localhost", server.PortNumber, 4);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlowMail.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            server.Stop();
            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }


        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsyncWithConfig()
        {
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(mockSmtpPort);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);

            MailMessageQueue queue = new MailMessageQueue(3);
            // queue.AddAndSendAsync(new System.Net.Mail.MailMessage("testfrom@fonlowmail.com", "testto@fonlowmail.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            server.Stop();
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }



        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsyncWithManyMessages()
        {
            const int messageCount = 1000;
            List<Message> messages = new List<Message>();
            DefaultServer server = new DefaultServer(Ports.AssignAutomatically);
            server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            server.Start();
            Debug.WriteLine("Port: " + server.PortNumber);

            MailMessageQueue queue = new MailMessageQueue("localhost", server.PortNumber, 2);//smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848
            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
            for (int i = 0; i < messageCount; i++)
            {
                queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            }
            System.Threading.Thread.Sleep(5000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.
            server.Stop();
            Assert.AreEqual(messageCount, messages.Count);
            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsyncWithInvalidSmtpHost()
        {
            MailMessageQueue queue = new MailMessageQueue("funky.fonlow.com");
            Assert.IsTrue(queue.AcceptItem);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(10000);//need to wait longer for resolving the host, otherwise the test host is terminated resulting in thread abort.
            Assert.IsFalse(queue.AcceptItem);
            Assert.AreEqual(1, queue.Count);
        }

        [TestMethod]
        [TestCategory("MailIntegration")]
        public void TestSmtpClientAsyncWithInvalidRecipient()
        {
            MailMessageQueue queue = new MailMessageQueue("mail.fonlow.com");
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(3000);
            Assert.IsFalse(queue.AcceptItem);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            Assert.AreEqual(1, queue.Count);
        }


        //   Integration tests end                */
    }
}
