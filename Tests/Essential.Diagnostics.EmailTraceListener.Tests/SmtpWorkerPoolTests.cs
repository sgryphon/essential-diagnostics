//using Essential.Net.Mail;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace Essential.Diagnostics.Tests
//{
//    [TestClass]
//    public class SmtpWorkerPoolTests
//    {
//        static string pickupDirectory;

//        [ClassInitialize()]
//        public static void ClassInitialize(TestContext testContext)
//        {
//            var smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
//            pickupDirectory = (smtpConfig != null) ? smtpConfig.SpecifiedPickupDirectory.PickupDirectoryLocation : null;
//            if (!String.IsNullOrEmpty(pickupDirectory))
//            {
//                if (!Directory.Exists(pickupDirectory))
//                {
//                    try
//                    {
//                        Directory.CreateDirectory(pickupDirectory);
//                    }
//                    catch (Exception ex)
//                    {
//                        throw new ApplicationException(
//                            string.Format("Cannot create pickup directory '{0}'; can't run tests.", pickupDirectory), ex);
//                    }
//                }
//            }

//        }

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            if (!string.IsNullOrEmpty(pickupDirectory))
//            {
//                ClearPickupDirectory();
//            }
//        }

//        void ClearPickupDirectory()
//        {
//            string[] filePaths = Directory.GetFiles(pickupDirectory);
//            foreach (string filePath in filePaths)
//                File.Delete(filePath);
//        }

//        [TestCleanup]
//        public void TestCleanup()
//        {
//        }

//        void AssertMessagesSent(int expected)
//        {
//            Assert.AreEqual(expected, Directory.GetFiles(pickupDirectory).Count());
//        }

//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorkerSendOne()
//        {
//            SmtpWorkerPool pool = new SmtpWorkerPool(3);
//            var asyncResult = pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            //System.Threading.Thread.Sleep(500); //need to wait, otherwise the test host is terminated resulting in thread abort.
//            pool.EndSend(asyncResult);
//            AssertMessagesSent(1);
//            //Assert.IsTrue(queue.AcceptItem);
//            //Assert.AreEqual(0, queue.Count);
//        }


//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorkerSendTwo()
//        {
//            SmtpWorkerPool pool = new SmtpWorkerPool(4);
//            pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
//            AssertMessagesSent(2);
////            Assert.IsTrue(queue.AcceptItem);
////            Assert.AreEqual(0, queue.Count);
//        }


//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorkerSendMany()
//        {
//            const int messageCount = 1000;

//            SmtpWorkerPool pool = new SmtpWorkerPool(4);
//            //smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848

//            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
//            for (int i = 0; i < messageCount; i++)
//            {
//                pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            }
//            System.Threading.Thread.Sleep(2000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.
//            AssertMessagesSent(messageCount);
//            //Assert.IsTrue(queue.AcceptItem);
//            //Assert.AreEqual(0, queue.Count);
//        }


//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorker2SendOne()
//        {
//            SmtpWorkerPool2 pool = new SmtpWorkerPool2(3);
//            var asyncResult = pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            //System.Threading.Thread.Sleep(500); //need to wait, otherwise the test host is terminated resulting in thread abort.
//            pool.EndSend(asyncResult);
//            AssertMessagesSent(1);
//        }

//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorker2SendTwo()
//        {
//            SmtpWorkerPool2 pool = new SmtpWorkerPool2(4);
//            pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            System.Threading.Thread.Sleep(2000);//need to wait, otherwise the test host is terminated resulting in thread abort.
//            AssertMessagesSent(2);
//        }

//        [TestMethod]
//        [TestCategory("MailIntegration")]
//        public void SmtpWorker2SendMany()
//        {
//            const int messageCount = 1000;

//            SmtpWorkerPool2 pool = new SmtpWorkerPool2(4);
//            //smtp4dev apparently accept only 2 concurrent connections, according to http://smtp4dev.codeplex.com/discussions/273848

//            Debug.WriteLine("Start sending messages at " + DateTime.Now.ToString());
//            for (int i = 0; i < messageCount; i++)
//            {
//                pool.BeginSend(new System.Net.Mail.MailMessage("user1@example.com", "user2@example.com", "HelloAsync", "are you there? async"), null, null);
//            }
//            System.Threading.Thread.Sleep(2000);//need to wait for around 1-3 seconds for 1000 messages., otherwise the test host is terminated resulting in thread abort.
//            AssertMessagesSent(messageCount);
//        }


//    }
//}
