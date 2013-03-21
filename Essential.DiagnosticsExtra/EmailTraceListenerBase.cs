using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.ComponentModel;
using System.Net.Mail;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Output trace messages of warning and error only as Email messages via SMTP. Every messages sent will go through a MailMessage queue which will send 
    /// messages through multiple SmtpClient connections in a connection pool.
    /// 
    /// The subject line of the Email message will be the text before ':', ';', ',', '.', '-' in the trace message, along with the identity of the application.
    /// The body of the Email will be the trace message.
    /// 
    /// The SMTP host settings are defined in MailSettings of app.config, as documented at http://msdn.microsoft.com/en-us/library/w355a94k.aspx. However, the from address in MailSettings is not used in this trace listener,
    /// since it might be used by the application. The trace listener should have its own from address in order to be filtered.
    /// 
    /// It supports the following custom attributes used in config:
    /// * fromAddress
    /// * toAddress
    /// * maxConnections: Maximum SmtpClient connections in pool. The default is 2.
    /// 
    /// Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
    /// it is recommended that the Email subject or body should log the datetime of the trace message.
    /// </summary>
    /// <remarks>
    /// Each message is sent in an asynchnous call. If the host process is terminated, the thread running the sending will be terminated as well, 
    /// therefore the last few error message traced might be lost. Because of the latency of Email, performance and the limitation of Email relay, this listener is not so appropriate in
    /// a service app that expect tens of thousands of concurrent requests per minutes. Ohterwise, a critical error in the service app with trigger tens of thousands of warnings piled in the MailMessage queue.
    /// Alternatively, to avoid memory blow because of a larrge message queue, you may consider to make SmtpClient deliver messages to a directory through defining SmtpDeliveryMethod=SpecifiedPickupDirectory or PickupDirectoryFromIis,
    /// so the otehr application may pickup and send.
    /// 
    /// In addition, firewall, anti-virus software and the mail server spam policy may also have impact on this listener, so system administrators have to be involved to ensure the operation of this listener.
    /// 
    /// If you define SmtpDeliveryMethod to make SmtpClient write messages to a pickup directory, please make sure the total number of client connections is no more than 2, since 
    /// concurrent access to a file system is almost certain to slow down the overall performance unless you are RAID. For the best performance, you really need to run some integration test
    /// to get the optimized number for a RAID system.
    /// 
    /// Please note, defining a large number of SMTP connections in pool may not necessarily give you the best performance. The overall performance with the optimized number of concurrent connections depends on the following factors:
    /// 1. The number of processors
    /// 2. The implementation/config of the SMTP server
    /// 3. The average size of Email messages
    /// 
    /// Because the Email messages are sent asynchronously, so if the application process crashes, it is possible that the last few messages before the crash might not be sent out, unless your application has
    /// implemented a handler catching all uncaught exception, and the handler will flush all trace listeners. Anyway, your log file should have all trace messages.
    /// </remarks>
    public abstract class EmailTraceListenerBase : TraceListener
    {
        protected EmailTraceListenerBase()
        {
            smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
        }

        protected EmailTraceListenerBase(string toAddress)
            : this()
        {
            this.toAddress = toAddress;
        }

        string toAddress;

        System.Net.Configuration.SmtpSection smtpConfig;

        /// <summary>
        /// Defined in attribute fromAddress or property From of system.net/mailSettings/smtp. 
        /// </summary>
        protected string FromAddress
        {
            get 
            {
               return smtpConfig.From;
            }
        }

        /// <summary>
        /// Normally taken from initializeData of config.
        /// 
        /// </summary>
        protected string ToAddress
        {
            get
            {
                return toAddress;
            }
        }

        /// <summary>
        /// Maximum concurrent connections in the SmtpClient pool, defined in custom attribute maxConnections. The default value is 2.
        /// </summary>
        protected int MaxConnections
        {
            get
            {//todo: test with config change.
                string s = Attributes["maxConnections"];
                int m;
                return Int32.TryParse(s, out m) ? m : 2;
            }
        }

        static string[] supportedAttributes = new string[] {"maxConnections" };

        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        // object clientLock = new object();

        MailMessage CreateMailMessage(string subject, string body, string recipient)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(FromAddress);
            mailMessage.To.Add(recipient);
            if (String.IsNullOrEmpty(subject))
                subject = "Subject empty";

            MailMessageHelper.SanitiseEmailSubject(mailMessage, subject);
            mailMessage.Body = StartupInfo.GetParagraph(body);
            return mailMessage;
        }

        protected void SendEmailAsync(string subject, string body, string recipient)
        {
            MessageQueue.AddAndSendAsync(CreateMailMessage(subject, body, recipient));
        }

        /// <summary>
        /// Send Email via a SmtpClient in pool.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        protected void SendEmailAsync(string subject, string body)
        {
            SendEmailAsync(subject, body, ToAddress);
        }

        /// <summary>
        /// Send Email via a new SmtpClient.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        protected void SendEmail(string subject, string body)
        {
            SmtpClient client = null;
            try
            {
                client = new SmtpClient();
                Debug.WriteLine("subject = " + subject);
                using (var mailMessage = new MailMessage(FromAddress, ToAddress, MailMessageHelper.SanitiseSubject(subject), body))
                {
                    client.Send(mailMessage);
                }

            }
            finally
            {
                if ((client != null) && (typeof(SmtpClient).GetInterface("IDisposable") != null))
                {
                    (client as IDisposable).Dispose();
                }
            }
        }



        MailMessageQueue messageQueue;

        static object objectLock = new object();

        MailMessageQueue MessageQueue
        {
            get
            {
                lock (objectLock)
                {
                    if (messageQueue == null)
                    {
                        messageQueue = new MailMessageQueue(MaxConnections);
                        Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return messageQueue;
            }
        }


    }
}
