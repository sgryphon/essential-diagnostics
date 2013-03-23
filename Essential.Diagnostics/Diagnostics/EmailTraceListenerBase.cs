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
    /// The SMTP host settings are defined in MailSettings of app.config, as documented at http://msdn.microsoft.com/en-us/library/w355a94k.aspx. 
    /// 
    /// It supports the following custom attributes used in config:
    /// * maxConnections: Maximum SmtpClient connections in pool. The default is 2.
    /// 
    /// Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
    /// it is recommended that the Email subject or body should log the datetime of the trace message.
    /// </summary>
    public abstract class EmailTraceListenerBase : TraceListenerBase
    {
        protected EmailTraceListenerBase()
        {
            smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
        }

        protected EmailTraceListenerBase(string toAddress)
            : this()
        {
            this.toAddress = toAddress;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SendAllBeforeExit();
        }

        protected virtual void SendAllBeforeExit()
        {
            const int interval = 200;
            int totalWaitTime = 0;
            while ((!MessageQueue.Idle) && (totalWaitTime < SendAllTimeoutInSeconds * 1000))
            {
                System.Threading.Thread.Sleep(interval);
                totalWaitTime += interval;
            }
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

        protected int SendAllTimeoutInSeconds
        {
            get
            {//todo: test with config change.
                string s = Attributes["sendAllTimeoutInSeconds"];
                int m;
                return Int32.TryParse(s, out m) ? m : 10;
            }
        }

        const string defaultSubjectTemplate = "{MESSAGE} -- Machine: {MACHINENAME}; User: {USER}; Process: {PROCESS}; AppDomain: {APPDOMAIN}; Local Time: {LOCALDATETIME}";

        const string defaultMessageTemplate = "Time: {LOCALDATETIME}\nMachine: {MACHINENAME}\nUser: {USER}\nProcess: {PROCESS}\nAppDomain: {APPDOMAIN}\n\n{MESSAGE}";

        protected string SubjectTemplate
        {
            get 
            { 
                string s= Attributes["subjectTemplate"];
                if (String.IsNullOrEmpty(s))
                    return defaultSubjectTemplate;

                return s;
            }
        }

        protected string MessageTemplate
        {
            get 
            { 
                string s= Attributes["messageTemplate"];
                if (String.IsNullOrEmpty(s))
                    return defaultMessageTemplate;

                return s;
            }
        }

        static string[] supportedAttributes = new string[] {"maxConnections", "sendAllTimeoutInSeconds", "subjectTemplate", "messageTemplate" };

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
            mailMessage.Subject = subject;
            mailMessage.Body = body;
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

        MailMessageQueue messageQueue;

        static object objectLock = new object();

        /// <summary>
        /// Mail message queue created upon the first warning trace.
        /// </summary>
        protected MailMessageQueue MessageQueue
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
