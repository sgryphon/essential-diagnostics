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


        [TestMethod]
        public void TestEmailSubject()
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            MailUtil_Accessor.AssignEmailSubject(message, "abcde");
            MailUtil_Accessor.AssignEmailSubject(message, "abcde\nfg");
            Assert.AreEqual("abcde", message.Subject);
            const string s = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb " +
                "ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc" +
                "ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd ";
            MailUtil_Accessor.AssignEmailSubject(message, s);
            Assert.AreEqual(s.Substring(0, 254), message.Subject);


        }
 //////////////////////// Integration tests for Email functions should not be executed often.
///*During integration tests, it is good to have a local SMTP server installed. Windows 7 does not have one, so you may use hMailServer. External SMTP server might be subject to spam control and network issue.

                [TestMethod]
                [TestCategory("MailIntegration")]
                public void TestEmailTraceListener()
                {
                    Trace.TraceWarning("Anything. More detail go here.");
                    Trace.TraceError("something wrong; can you tell? more here.");
                    Trace.WriteLine("This is writeline.", "Category");
                    Trace.WriteLine("This is another writeline.", "caTegory");
                    Trace.WriteLine("Writeline without right category", "CCCC");
                    System.Threading.Thread.Sleep(15000);//need to wait, otherwise the test host is terminated resulting in thread abort.
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
                    SmtpClientAsync_Accessor client = new SmtpClientAsync_Accessor("localhost",true);
                    client.SendCompleted = client_SendCompleted;
                    client.SendAsync(new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync", "are you there? async"));
                    System.Threading.Thread.Sleep(15000);//need to wait, otherwise the test host is terminated resulting in thread abort.
                }

                [TestMethod]
                [TestCategory("MailIntegration")]
                public void TestSendEmailAsync()
                {
                    MailUtil_Accessor.SendEmailAsync("localhost", new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsync2", "are you there? async"));
                    System.Threading.Thread.Sleep(15000);//need to wait, otherwise the test host is terminated resulting in thread abort.
                }

                static void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
                {
                    Debug.WriteLine("done.");
                }


                [TestMethod]
                [TestCategory("MailIntegration")]
                public void TestSendEmailAsync64()
                {
                    for (int i = 0; i < 60; i++)
                    {
                        MailUtil_Accessor.SendEmailAsync("localhost", new System.Net.Mail.MailMessage("andy@fonlowmail.com", "arnold@fonlowmail.com", "HelloAsyncMany"+i, "are you there? async"));
                    } 
            
                    System.Threading.Thread.Sleep(40000);//need to wait, otherwise the thread will be terminated too early.
                }



//   Integration tests end                */
    }
}
