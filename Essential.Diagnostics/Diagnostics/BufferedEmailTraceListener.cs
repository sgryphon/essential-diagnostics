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
    /// Intended to be used in console apps which will send all warning/error traces via email at the end of the process.
    /// </para>
    /// <para>
    /// The listener will put error and warning trace messages into a buffer. 
    /// </para>
    /// </remarks>
    public class BufferedEmailTraceListener : TraceListenerBase
    {
        TraceFormatter traceFormatter = new TraceFormatter();
        object bufferLock = new object();
        StringBuilder eventMessagesBuffer = new StringBuilder(10000);
        string firstMessage;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toAddress"></param>
        public BufferedEmailTraceListener(string toAddress)
            : base(toAddress)
        {
            this.toAddress = toAddress;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe. BufferedEmailTraceListener is not threadsafe.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                // if you don't make the listener thread safe then it simply locks on the whole thing!
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the template for a a single trace message.
        /// </summary>
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
                    firstMessage = null;
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
        /// While the listener will send an email message at the end of the hosting process. This function allow your 
        /// application code to send the email message earlier. For example, you might want to send one message at the 
        /// end of each loop. 
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

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var traceDetails = traceFormatter.Format(TraceTemplate, eventCache, source, eventType, id, message, relatedActivityId, data);
            lock (bufferLock)
            {
                if (eventMessagesBuffer.Length == 0)
                {
                    firstMessage = message;
                }
                eventMessagesBuffer.AppendLine(traceDetails);
            }
        }

        protected virtual string DefaultTraceTemplate
        {
            get
            {
                return "{LocalDateTime:HH:mm:ss} {EventType} [{ThreadId}] {Message}";
            }
        }

        // ================================================================================================================================

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
            string firstMessageToSend;

            lock (bufferLock)
            {
                bufferToSend = eventMessagesBuffer;
                firstMessageToSend = firstMessage;
                if (eventMessagesBuffer.Length > 0)
                {
                    eventMessagesBuffer = new StringBuilder(100000);
                    firstMessage = null;
                }
            }

            if (bufferToSend.Length > 0)
            {
                // TODO: Would it be easier to simply format the subject using the first message?
                //       Sure the user could put in something like the event level which is event specific, but that's their choice.
                string subject = traceFormatter.Format(SubjectTemplate, null, null, TraceEventType.Information, 0,
                    firstMessage, null, null);

                // TODO: Maybe replace 'BodyTemplate' with 'HeaderTemplate' (generated from first message), then simply append all traces using the trace format??
                string allMessages = bufferToSend.ToString();
                string body = traceFormatter.Format(BodyTemplate, null, null, TraceEventType.Information, 0, allMessages, null, null);

                SendEmail(subject, body, waitForComplete);
            }
        }

    }

}
