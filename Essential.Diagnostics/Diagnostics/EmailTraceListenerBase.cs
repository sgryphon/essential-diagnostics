using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Essential.Net.Mail;

namespace Essential.Diagnostics
{
    /// <remarks>
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
    /// </remarks>
    public abstract class EmailTraceListenerBase : TraceListenerBase
    {
        const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
        const int defaultMaxConnections = 2;
        static readonly Regex controlCharRegex = new Regex(@"\p{C}", RegexOptions.Compiled);
        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "bodyTemplate", "BodyTemplate", "bodytemplate",
            "traceTemplate", "TraceTemplate", "tracetemplate" };

        string toAddress;
        SmtpWorkerPool smtpWorkerPool;
        SmtpWorkerPool2 smtpWorkerPool2;
        static object smtpWorkerPoolLock = new object();

        protected EmailTraceListenerBase(string toAddress)
        {
            this.toAddress = toAddress;
        }

        /// <summary>
        /// Gets the email address the trace messages will be sent to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is part of initializeData; if the value changes the
        /// listener is recreated. See the constructor parameter for details
        /// of the supported formats.
        /// </para>
        /// </remarks>
        public string ToAddress
        {
            get
            {
                return toAddress;
            }
        }

        /// <summary>
        /// Gets or sets the maximum concurrent connections in the SmtpClient pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The is deffined by the custom attribute maxConnections. The default value is 2.
        /// </para>
        /// </remarks>
        public int MaxConnections
        {
            get
            {//todo: test with config change.
                string s = Attributes["maxConnections"];
                int value;
                return Int32.TryParse(s, out value) ? value : defaultMaxConnections;
            }
            set
            {
                Attributes["maxConnections"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the template used to construct the email subject.
        /// </summary>
        public string SubjectTemplate
        {
            get 
            { 
                string s = Attributes["subjectTemplate"];
                if (String.IsNullOrEmpty(s))
                {
                    return DefaultSubjectTemplate;
                }
                return s;
            }
            set
            {
                Attributes["subjectTemplate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template used to construct the email body.
        /// </summary>
        public string BodyTemplate
        {
            get
            {
                string s = Attributes["bodyTemplate"];
                if (String.IsNullOrEmpty(s)) 
                {
                    return DefaultBodyTemplate;
                }
                return s;
            }
            set
            {
                Attributes["bodyTemplate"] = value;
            }
        }


        protected virtual string DefaultSubjectTemplate { get { return "{MessagePrefix} -- Machine: {MachineName}; User: {User}; Process: {Process}; AppDomain: {AppDomain}"; } }

        protected virtual string DefaultBodyTemplate { get { return "Time: {LocalDateTime}\nMachine: {MachineName}\nUser: {User}\nProcess: {Process}\nAppDomain: {AppDomain}\n\n{Message}"; } }


        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
        }

        SmtpWorkerPool SmtpWorkerPool
        {
            get
            {
                lock (smtpWorkerPoolLock)
                {
                    if (smtpWorkerPool == null)
                    {
                        smtpWorkerPool = new SmtpWorkerPool(MaxConnections);
                        //Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return smtpWorkerPool;
            }
        }

        SmtpWorkerPool2 SmtpWorkerPool2
        {
            get
            {
                lock (smtpWorkerPoolLock)
                {
                    if (smtpWorkerPool2 == null)
                    {
                        smtpWorkerPool2 = new SmtpWorkerPool2(MaxConnections);
                        //Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return smtpWorkerPool2;
            }
        }

        /// <summary>
        /// Send Email via a SmtpClient in pool.
        /// </summary>
        internal void SendEmail(string subject, string body, bool waitForComplete)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.To.Add(ToAddress);
            mailMessage.Subject = SanitiseSubject(subject);
            mailMessage.Body = body;

            var asyncResult = SmtpWorkerPool2.BeginSend(mailMessage,
                (ar) => { ((MailMessage)ar.AsyncState).Dispose(); }, 
                mailMessage);
            if (waitForComplete)
            {
                SmtpWorkerPool2.EndSend(asyncResult);
            }
        }

        static string SanitiseSubject(string subject)
        {
            if (subject.Length > 254) 
            {
                subject = subject.Substring(0, subjectMaxLength - 3) + "...";
            }

            if (controlCharRegex.IsMatch(subject))
            {
                subject = controlCharRegex.Replace(subject, "");
            }

            return subject;
        }

    }
}
