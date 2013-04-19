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
    /// <para>
    /// The SMTP host settings are defined in MailSettings of app.config, as documented 
    /// at http://msdn.microsoft.com/en-us/library/w355a94k.aspx.  
    /// </para>
    /// <para>
    /// The following attributes can be set when adding the trace listener
    /// entry in the configuration file.
    /// </para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    ///     <term>Attribute</term>
    ///     <value>Description</value>
    /// </listheader>
    /// <item>
    /// <term>initializeData</term>
    /// <value>Email address of the recipient. Multiple recipients may be separated by commas.</value>
    /// </item>
    /// <item>
    /// <term>traceOutputOptions</term>
    /// <value>Ignored.</value>
    /// </item>
    /// <item>
    /// <term>maxConnections</term>
    /// <value>Maximum SMTP client connections in pool. Default 2 connections.</value>
    /// </item>
    /// <item>
    /// <term>subjectTemplate</term>
    /// <value>Template to use to format the email subject.
    /// For more information on the template tokens available, <see cref="TraceFormatter"/>.</value>
    /// </item>
    /// <item>
    /// <term>bodyTemplate</term>
    /// <value>Template to use to format the email subject.</value>
    /// </item>
    /// </list>
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

        const string DefaultSubjectTemplate = "{EventType} {Id}: {MessagePrefix}; {MachineName}; {User}; {Process}";
        const string DefaultBodyTemplate = @"Source: {Source}
Date (UTC): {DateTime:u}
Date (Local): {LocalDateTime:u}
Event ID: {Id}
Level: {EventType}
Activity: {ActivityId}
User: {User}
Computer: {MachineName}
AppDomain: {AppDomain}
Process ID: {ProcessId}
Process Name: {ProcessName}
Thread ID: [{Thread}]

Message:
{Message}

Data:
{Data}";

        const int defaultMaxConnections = 2;

        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "bodyTemplate", "BodyTemplate", "bodytemplate",
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
        /// Gets or sets the template used to construct the email body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default template includes the Source, Date (UTC and Local), Event ID, Level, 
        /// Activity, User, Computer, AppDomain, Process ID, Process Name, Thread ID, 
        /// Message (full) and Data.
        /// </para>
        /// <para>
        /// Note if configuring in XML to use the entity escape sequence to encode new
        /// lines, "&#xD;&#xA;" (if setting in code, encode new lines with '\n' as normal).
        /// </para>
        /// </remarks>
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
        /// Gets a value indicating the trace listener is thread safe.
        /// </summary>
        /// <value>true</value>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the maximum concurrent connections in the SmtpClient pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is 2.
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
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default value is "{EventType} {Id}: {MessagePrefix}; {MachineName}; {User}; {Process}".
        /// </para>
        /// <para>
        /// Note that the {MessagePrefix} inserts the trace message up to the first punctuation
        /// character with a maximum length of 40 characters. This keeps the email subject header
        /// short; the full message is included in the email body.
        /// </para>
        /// </remarks>
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
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return supportedAttributes;
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

            // Use hidden/undocumented attribute to switch versions (for testing)
            if (Attributes["poolVersion"] == "C")
            {
                var asyncResultC = SmtpWorkerPoolC.BeginSend(FromAddress, ToAddress, subject, body, null, null);
                return;
            }

            var asyncResultB = SmtpWorkerPoolB.BeginSend(FromAddress, ToAddress, subject, body, null, null);
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
                    }
                }
                return smtpWorkerPoolB;
            }
        }

    }

}
