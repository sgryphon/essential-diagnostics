using Essential.Net.Mail;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

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
    /// <term>bodyTemplate</term>
    /// <value>Template used to construct the email body.</value>
    /// </item>
    /// <item>
    /// <term>fromAddress</term>
    /// <value>Optional alternate from address, instead of the one configured in system.net mailSettings.</value>
    /// </item>
    /// <item>
    /// <term>maxConnections</term>
    /// <value>Maximum concurrent SMTP connections. Default is 2 connections.</value>
    /// </item>
    /// <item>
    /// <term>maxTracesPerHour</term>
    /// <value>Maximum number of emails per hour that will be sent, to prevent flooding. Default is 50.</value>
    /// </item>
    /// <item>
    /// <term>subjectTemplate</term>
    /// <value>Template used to construct the email subject.
    /// For more information on the template tokens available, <see cref="TraceFormatter"/>.</value>
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

        const string DefaultSubjectTemplate = "{EventType} {Id}: {MessagePrefix}; {MachineName}; {User}; {ProcessName}";
        const string DefaultBodyTemplate = @"Source: {Source}
Date (UTC): {DateTime:u}
Date (Local): {LocalDateTime:yyyy'-'MM'-'dd HH':'mm':'ss zzz}
Event ID: {Id}
Level: {EventType}
Activity: {ActivityId}

Application Information:
 Computer: {MachineName}
 Application Name: {ApplicationName}
 Application Domain: {AppDomain}

Process Information:
 Process ID: {ProcessId}
 Process Name: {ProcessName}
 Process: {Process}
 User: {User}

Thread Information:
 Thread ID: [{ThreadId}]
 Thread Name: {ThreadName}
 Thread Principal: {PrincipalName}

Message:
{Message}

Data:
{Data}";

        const int defaultMaxConnections = 2;
        const int defaultMaxTraces = 50;

        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "maxTracesPerHour", "MaxTracesPerHour", "maxtracesperhour",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "bodyTemplate", "BodyTemplate", "bodytemplate",
            "poolVersion" };

        string toAddress;
        SmtpWorkerPoolC smtpWorkerPoolC;
        SmtpWorkerPoolB smtpWorkerPoolB;
        static object smtpWorkerPoolLock = new object();

        /// <summary>
        /// Constructor. Sends each trace event in an email to the specified address.
        /// </summary>
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
        /// Gets or sets the maximum concurrent SMTP connections.
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
        /// Gets or sets the maximum number of emails per hour that will be sent, to prevent flooding.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is 50.
        /// </para>
        /// <para>
        /// To prevent flooding with emails this listener has a maximum number of email messages it
        /// will send per hour. Normally the frequency of emails should be controlled by adding
        /// an appropriate filter, for example add an EventTypeFilter that only send messages of
        /// Warning level and above.
        /// </para>
        /// <para>
        /// However there could still be an issue that causes repeated error events to be generated,
        /// which would result in a flood of emails. To prevent this the component drops any further
        /// traces once the limit is reached.
        /// </para>
        /// <para>
        /// Note that restarting the process clears the counter and will send additional messages.
        /// </para>
        /// <para>
        /// Setting the value to "0" will put no limit on the number of messages sent.
        /// </para>
        /// </remarks>
        public int MaxTracesPerHour
        {
            get
            {//todo: test with config change.
                string s = Attributes["maxTracesPerHour"];
                int value;
                return Int32.TryParse(s, out value) ? value : defaultMaxTraces;
            }
            set
            {
                Attributes["maxTracesPerHour"] = value.ToString(CultureInfo.InvariantCulture);
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

        const int floodLimitStatus_Ok = 0;
        const int floodLimitStatus_Block = 1;


        object floodLimitLock = new object();
        static TimeSpan floodLimitWindow = TimeSpan.FromHours(1);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification="Deliberate dependency, .NET 2.0 SP1 required.")]
        DateTimeOffset floodLimitReset;
        int floodNumberOfTraces;

        /// <summary>
        /// Write trace event with data.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification = "Deliberate dependency, .NET 2.0 SP1 required.")]
        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var traceTime = TraceFormatter.FormatUniversalTime(eventCache);
            int maxPerHour = MaxTracesPerHour;
            bool sendEmail = false;

            if (maxPerHour > 0)
            {
                lock (floodLimitLock)
                {
                    if (traceTime > floodLimitReset)
                    {
                        // start a new flood limit
                        floodLimitReset = traceTime.Add(floodLimitWindow);
                        floodNumberOfTraces = 0;
                    }
                    if (floodNumberOfTraces < maxPerHour)
                    {
                        floodNumberOfTraces++;
                        sendEmail = true;
                    }
                }
            }
            else
            {
                sendEmail = true;
            }

            if (sendEmail)
            {
                string subject = traceFormatter.Format(SubjectTemplate, this, eventCache, source,
                    eventType, id, message, relatedActivityId, data);

                string body = traceFormatter.Format(BodyTemplate, this, eventCache, source, eventType, id,
                    message, relatedActivityId, data);

                // Use hidden/undocumented attribute to switch versions (for testing)
                if (Attributes["poolVersion"] == "B")
                {
                    SmtpWorkerPoolB.BeginSend(FromAddress, ToAddress, subject, body, null, null);
                    return;
                }
                SmtpWorkerPoolC.BeginSend(FromAddress, ToAddress, subject, body, null, null);
            }
            else
            {
                Debug.WriteLine("Dropped message due to flood protection.");
            }
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
