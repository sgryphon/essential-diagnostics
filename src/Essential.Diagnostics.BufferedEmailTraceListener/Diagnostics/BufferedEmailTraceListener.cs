using Essential.Net.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Adds formatted trace messages to a buffer and sends an email when the process exits, or on request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended to be used in console apps which will send all warning/error traces via email at the end 
    /// of the process.
    /// </para>
    /// </remarks>
    public class BufferedEmailTraceListener : TraceListenerBase
    {
        const string DefaultSubjectTemplate = "{Listener} {DateTime:u}; {MachineName}; {User}; {ProcessName}";
        const string DefaultHeaderTemplate = @"Date (UTC): {DateTime:u}
Date (Local): {LocalDateTime:yyyy'-'MM'-'dd HH':'mm':'ss zzz}

Application Information:
 Computer: {MachineName}
 Application Name: {ApplicationName}
 Application Domain: {AppDomain}

Process Information:
 Process ID: {ProcessId}
 Process Name: {ProcessName}
 Process: {Process}
 User: {User}

Trace Events:";
        const string DefaultTraceTemplate = "{DateTime:u} [{Thread}] {EventType} {Source} {Id}: {Message}{Data}";

        TraceFormatter traceFormatter = new TraceFormatter();
        object bufferLock = new object();
        StringBuilder eventMessagesBuffer = new StringBuilder(10000);
        string eventMessagesHeader;
        string eventMessagesSubject;

        const int defaultMaxConnections = 2;
        static string[] supportedAttributes = new string[] { 
            "maxConnections", "MaxConnections", "maxconnections",
            "subjectTemplate", "SubjectTemplate", "subjecttemplate",
            "headerTemplate", "HeaderTemplate", "headertemplate",
            "traceTemplate", "TraceTemplate", "tracetemplate",
            "poolVersion" };

        string toAddress;
        SmtpWorkerPoolC smtpWorkerPoolC;
        SmtpWorkerPoolB smtpWorkerPoolB;
        static object smtpWorkerPoolLock = new object();

        /// <summary>
        /// Constructor. Adds each trace to a buffer and then sends an email to the specified 
        /// address with all buffered messages when the process exits, or on request.
        /// </summary>
        public BufferedEmailTraceListener(string toAddress)
            : base(toAddress)
        {
            this.toAddress = toAddress;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
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
        /// Gets or sets the template used to construct the header of the email body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default template includes the Date (UTC and Local), User, Computer, 
        /// AppDomain, Process ID, and Process Name. The details used are based on
        /// the first trace received, so should be used for information that will 
        /// be consistent or meaningful for all traces.
        /// </para>
        /// <para>
        /// The rest of the body of the email then contains the buffered messages 
        /// using TraceTemplate (including the buffered first message in the 
        /// TraceTemplate format).
        /// </para>
        /// <para>
        /// Note if configuring in XML to use the entity escape sequence to encode new
        /// lines, "&#xD;&#xA;" (if setting in code, encode new lines with '\n' as normal).
        /// </para>
        /// </remarks>
        public string HeaderTemplate
        {
            get
            {
                string s = Attributes["headerTemplate"];
                if (String.IsNullOrEmpty(s))
                {
                    return DefaultHeaderTemplate;
                }
                return s;
            }
            set
            {
                Attributes["headerTemplate"] = value;
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
                // if you don't make the listener thread safe then it simply locks on the whole thing!
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
        /// Gets or sets the template used to construct the email subject.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default value is "{Listener} {DateTime:u}; {MachineName}; {User}; {Process}", 
        /// and uses details from the first trace received, so should contain information that will 
        /// be consistent or meaningful for all traces.        
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
        /// Gets or sets the template for a single trace message.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See TraceFormatter for details of the supported formats.
        /// </para>
        /// <para>
        /// The default value is "{DateTime:u} [{Thread}] {EventType} {Source} {Id}: {Message}{Data}",
        /// which is also the format used in RollingFileTraceListener.
        /// </para>
        /// </remarks>
        public string TraceTemplate
        {
            get
            {
                string s = Attributes["traceTemplate"];
                if (String.IsNullOrEmpty(s))
                {
                    return DefaultTraceTemplate;
                }
                return s;
            }
            set
            {
                Attributes["traceTemplate"] = value;
            }
        }

        /// <summary>
        /// Clears all buffered messages.
        /// </summary>
        public void Clear()
        {
            lock (bufferLock)
            {
                if (eventMessagesBuffer.Length > 0)
                {
                    eventMessagesBuffer = new StringBuilder(100000);
                    eventMessagesHeader = null;
                    eventMessagesSubject = null;
                }
            }
        }

        /// <summary>
        /// Clears the buffer for all BufferedEmailTraceListener attached to Trace.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this does not clear listeners that are only attached to a TraceSource,
        /// they must be attached to the main Trace entry as well.
        /// </para>
        /// </remarks>
        public static void ClearAll()
        {
            var listeners = FindListeners();
            foreach (var listener in listeners)
            {
                listener.Clear();
            }
        }

        /// <summary>
        /// Send all trace messages in buffer in a single email message. If the buffer is empty, no email is sent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The buffer is then cleared.
        /// </para>
        /// </remarks>
        public void Send()
        {
            InternalSend(false);
        }

        /// <summary>
        /// Send all buffered messages for all listeners attached to Trace.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While the listener will send an email message at the end of the hosting process, this function 
        /// will allow your application code to send the email message earlier. For example, you might want 
        /// to send one message at the end of each loop. 
        /// </para>
        /// <para>
        /// Note that this does not clear listeners that are only attached to a TraceSource,
        /// they must be attached to the main Trace entry as well.
        /// </para>
        /// </remarks>
        public static void SendAll()
        {
            var listeners = FindListeners();
            foreach (var listener in listeners)
            {
                listener.Send();
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
            var traceDetails = traceFormatter.Format(TraceTemplate, this, eventCache, source, eventType, id, message, relatedActivityId, data);
            bool firstMessage = false;
            lock (bufferLock)
            {
                if (eventMessagesBuffer.Length == 0)
                {
                    firstMessage = true;
                }
                eventMessagesBuffer.AppendLine(traceDetails);
            }
            if (firstMessage)
            {
                eventMessagesSubject = traceFormatter.Format(SubjectTemplate, this, eventCache, source, eventType, id, message, relatedActivityId, data);
                eventMessagesHeader = traceFormatter.Format(HeaderTemplate, this, eventCache, source, eventType, id, message, relatedActivityId, data);
            }
        }

        // ================================================================================================================================

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

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            // Send anything queued
            Debug.WriteLine("BufferedEmailTraceListener CurrentDomain_ProcessExit - Sending synchronously");
            InternalSend(true);
            Debug.WriteLine("BufferedEmailTraceListener CurrentDomain_ProcessExit - Send DONE");
        }

        static IEnumerable<BufferedEmailTraceListener> FindListeners()
        {
            var listenerFound = false;
            foreach (TraceListener t in Trace.Listeners)
            {
                var myListener = t as BufferedEmailTraceListener;
                if (myListener != null)
                {
                    listenerFound = true;
                    yield return myListener;
                }
            }

            if (!listenerFound)
            {
                Trace.TraceError("You want to use BufferedEmailTraceListener, but there's none in Trace.Listeners, probably not defined in the config file.");
            }
        }

        private void InternalSend(bool waitForComplete)
        {
            StringBuilder bufferToSend;
            string headerToSend;
            string subjectToSend;

            lock (bufferLock)
            {
                bufferToSend = this.eventMessagesBuffer;
                headerToSend = this.eventMessagesHeader;
                subjectToSend = this.eventMessagesSubject;
                if (eventMessagesBuffer.Length > 0)
                {
                    eventMessagesBuffer = new StringBuilder(100000);
                    eventMessagesHeader = null;
                    eventMessagesSubject = null;
                }
            }

            if (bufferToSend.Length > 0)
            {
                string body = headerToSend + Environment.NewLine + bufferToSend.ToString();

                // Use hidden/undocumented attribute to switch versions (for testing)
                if (Attributes["poolVersion"] == "B")
                {
                    var asyncResult = SmtpWorkerPoolB.BeginSend(FromAddress, ToAddress, subjectToSend, body, null, null);
                    if (waitForComplete)
                    {
                        SmtpWorkerPoolB.EndSend(asyncResult);
                    }
                }
                else // default
                {
                    var asyncResult = SmtpWorkerPoolC.BeginSend(FromAddress, ToAddress, subjectToSend, body, null, null);
                    if (waitForComplete)
                    {
                        SmtpWorkerPoolC.EndSend(asyncResult);
                    }
                }

            }
        }

    }

}
