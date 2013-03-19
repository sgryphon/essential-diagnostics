using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Essential.Diagnostics;
//using Rnwood.SmtpServer;
using System.Diagnostics;

namespace EssentialDiagnosticsIntegrationTests
{
    [TestFixture]
    [Description("Test with a local SMTP server that may handle domain fonlowmail.com.")]
    public class EmailTraceListenersTests
    {
        public EmailTraceListenersTests()
        {
            smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
            pickupDirectory = (smtpConfig != null) ? smtpConfig.SpecifiedPickupDirectory.PickupDirectoryLocation : null;
            mockSmtpPort = smtpConfig.Network.Port;
        }

        string pickupDirectory;
        System.Net.Configuration.SmtpSection smtpConfig;
        int mockSmtpPort = 9999;

        [Test]
        public void TestMailMessageQueueWithInvalidSmtpHost()
        {
            MailMessageQueue queue = new MailMessageQueue("funky.fonlow.com", 25, 3);
            Assert.IsTrue(queue.AcceptItem);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlow.com", "arnold@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(10000);//need to wait longer for resolving the host, otherwise the test host is terminated resulting in thread abort.
            Assert.IsFalse(queue.AcceptItem);
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void TestMailMessageQueueWithInvalidRecipient()
        {
            MailMessageQueue queue = new MailMessageQueue("mail.fonlow.com", 25, 3);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(5000);
            Assert.IsFalse(queue.AcceptItem);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void TestMailMessageQueue()
        {
            //List<Message> messages = new List<Message>();
            //DefaultServer server = new DefaultServer(mockSmtpPort);
            //server.MessageReceived += (s, ea) => messages.Add(ea.Message);
            //server.Start();

            MailMessageQueue queue = new MailMessageQueue(3);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(3000);//need to wait, otherwise the test host is terminated resulting in thread abort.
       //     server.Stop();

            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
         //   Assert.AreEqual(1, messages.Count);
        }

        [Test]
        public void TestMailMessageQueue2()
        {

            MailMessageQueue queue = new MailMessageQueue(4);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }



        [Test]
        [Description("Stress testing the Smtp")]
        public void TestMailMessageQueueWithManyMessages()
        {
            const int messageCount = 100;
            bool done = false;
            MailMessageQueue queue = new MailMessageQueue(8);
            DateTime dt = DateTime.Now;
            queue.QueueEmpty += (obj, e) => { done = true; };
            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
            for (int i = 0; i < messageCount; i++)
            {
                queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            }

            while (!done)
            {
                System.Threading.Thread.Sleep(1000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.

            }            

            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
            Debug.WriteLine("total time (seconds): " + (DateTime.Now - dt).TotalSeconds);
        }


        [Test]
        [Description("EmailTraceListener defined in config should be working and sending Email messages out.")]
        public void TestEmailTraceListener()
        {
            Trace.TraceWarning("Anything. More detail go here.");
            Trace.TraceError("something wrong; can you tell? more here.");
            Trace.WriteLine("This is writeline.", "Category");
            Trace.WriteLine("This is another writeline.", "caTegory");
            Trace.WriteLine("Writeline without right category", "CCCC");
            System.Threading.Thread.Sleep(5000);//need to wait, otherwise the test host is terminated resulting in thread abort.
        }

        [Test]
        [Description("Expect an error trace.")]
        public void TestAnErrorBufferTraceListenerWithoutDefinedIn()
        {
            Trace.TraceWarning("Anything. More detail go here.");
            Trace.TraceError("something wrong; can you tell? more here.");
            Trace.WriteLine("This is writeline.", "Category");
            Trace.WriteLine("This is another writeline.", "caTegory");
            Trace.WriteLine("Writeline without right category", "CCCC");
            ErrorBufferEmailTraceListener.SendMailOfEventMessages();
            System.Threading.Thread.Sleep(5000);
        }




    }
}
