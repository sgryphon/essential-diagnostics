using Essential.Net.Mail;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Sends a formatted email containing the contents of the trace.
    /// </summary>
    /// <remarks>
	/// <para>
	/// Sends each trace message received in an email, using the specified subject 
	/// and body templates.
	/// </para>
    /// <para>
    /// It is strongly recommended to set a filter to only accept Warning and above errors,
    /// or otherwise reduce the number of trace events that are processed by this listener
	/// to avoid flooding.
    /// </para>
    /// <para>
    /// Sending an email is an expensive operation, so messages are queued and sent on
    /// a separate thread. If there is a flood of messages exceeding the queue size then
    /// messages will be dropped.
    /// </para>
    /// </remarks>
    public class EmailTraceListener : TraceListenerBase
    {
        // <remarks>
        // Output trace messages of warning and error only as Email messages via SMTP. Every messages sent will go through a MailMessage queue which will send 
        // messages through multiple SmtpClient connections in a connection pool.
        // 
        // The subject line of the Email message will be the text before ':', ';', ',', '.', '-' in the trace message, along with the identity of the application.
        // The body of the Email will be the trace message.
        // 
        // The SMTP host settings are defined in MailSettings of app.config, as documented at http://msdn.microsoft.com/en-us/library/w355a94k.aspx. 
        // 
        // It supports the following custom attributes used in config:
        // * maxConnections: Maximum SmtpClient connections in pool. The default is 2.
        // 
        // Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
        // it is recommended that the Email subject or body should log the datetime of the trace message.
        // </remarks>

        TraceFormatter traceFormatter = new TraceFormatter();

        const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
        const int defaultMaxConnections = 2;
        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "bodyTemplate", "BodyTemplate", "bodytemplate",
            "traceTemplate", "TraceTemplate", "tracetemplate",
            "poolVersion" };

        string toAddress;
        SmtpWorkerPoolC smtpWorkerPoolC;
        SmtpWorkerPoolB smtpWorkerPoolB;
        static object smtpWorkerPoolLock = new object();

        public EmailTraceListener(string toAddress)
            : base(toAddress)
        {
            this.toAddress = toAddress;
        }

        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets an alternate from address, instead of the one configured in system.net mailSettings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Generally the value set in system.net mailSettings is used as the From address field in any email
        /// sent, however this attribute may be set to override the value.
        /// </para>
        /// </remarks>
        public string FromAddress
        {
            get
            {
                if (Attributes.ContainsKey("fromAddress"))
                {
                    return Attributes["fromAddress"];
                }
                else
                {
                    var smtpConfig = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection;
                    Attributes["fromAddress"] = smtpConfig.From;
                    return Attributes["fromAddress"];
                }
            }
            set
            {
                Attributes["fromAddress"] = value;
            }
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

        // Callback version
        SmtpWorkerPoolC SmtpWorkerPoolC
        {
            get
            {
                lock (smtpWorkerPoolLock)
                {
                    if (smtpWorkerPoolC == null)
                    {
                        smtpWorkerPoolC = new SmtpWorkerPoolC(MaxConnections);
                        //Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return smtpWorkerPoolC;
            }
        }

        // Background Thread version
        SmtpWorkerPoolB SmtpWorkerPoolB
        {
            get
            {
                lock (smtpWorkerPoolLock)
                {
                    if (smtpWorkerPoolB == null)
                    {
                        smtpWorkerPoolB = new SmtpWorkerPoolB(MaxConnections);
                        //Debug.WriteLine("MessageQueue is created with some connections: " + MaxConnections);
                    }
                }

                return smtpWorkerPoolB;
            }
        }

        /// <summary>
        /// Send Email via a SmtpClient in pool.
        /// </summary>
        internal void SendEmail(string subject, string body, bool waitForComplete)
        {
            // Use hidden/undocumented attribute to switch versions (for testing)
            if (Attributes["poolVersion"] == "C")
            {
                var asyncResult = SmtpWorkerPoolC.BeginSend(FromAddress, ToAddress, subject, body, null, null);
                if (waitForComplete)
                {
                    SmtpWorkerPoolC.EndSend(asyncResult);
                }
            }
            else // default
            {
                var asyncResult = SmtpWorkerPoolB.BeginSend(FromAddress, ToAddress, subject, body, null, null);
                if (waitForComplete)
                {
                    SmtpWorkerPoolB.EndSend(asyncResult);
                }
            }
        }

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            string subject = traceFormatter.Format(SubjectTemplate, eventCache, source, 
                eventType, id, message, relatedActivityId, data);

            string body = traceFormatter.Format(BodyTemplate, eventCache, source, eventType, id, 
                message, relatedActivityId, data);

            SendEmail(subject, body, false);
        }

    }

}
