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
        public void TestSmtpClientAsyncWithInvalidSmtpHost()
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
        public void TestSmtpClientAsyncWithInvalidRecipient()
        {
            MailMessageQueue queue = new MailMessageQueue("mail.fonlow.com", 25, 3);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(3000);
            Assert.IsFalse(queue.AcceptItem);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("HeyAndy@fonlow.com", "NotExist@fonlow.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void TestSmtpClientAsync()
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
        public void TestSmtpClientAsync2()
        {

            MailMessageQueue queue = new MailMessageQueue(4);
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            queue.AddAndSendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.

            Assert.IsTrue(queue.AcceptItem);
            Assert.AreEqual(0, queue.Count);
        }



        [Test]
        public void TestSmtpClientAsyncWithManyMessages()
        {
            const int messageCount = 100;
            bool done = false;
            MailMessageQueue queue = new MailMessageQueue(8);//smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848
            DateTime dt = DateTime.Now;
            queue.QueueEmpty += (obj, e) => { done = true; };// new EventHandler(queue_MessageQueueEmpty);
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





    }
}
