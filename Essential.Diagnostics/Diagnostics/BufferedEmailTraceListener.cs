using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.ComponentModel;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Intended to be used in console apps which will send all warning/error traces via Email at the end of the process.
    /// 
    /// The listener will put error and warning trace messages into a buffer. 
    /// 
    /// 
    /// </summary>
    public class BufferedEmailTraceListener : EmailTraceListenerBase
    {
        TraceFormatter traceFormatter = new TraceFormatter();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toAddress"></param>
        public BufferedEmailTraceListener(string toAddress)
            : base(toAddress)
        {
            if (Filter == null)
            {
                Filter = new EventTypeFilter(SourceLevels.Warning);
            }

            EventMessagesBuffer = new StringBuilder(10000);
        }


        /// <summary>
        /// Message buffer
        /// </summary>
        public StringBuilder EventMessagesBuffer { 
            get; 
            private set; 
        }

        /// <summary>
        /// Gets whether or not the listener has any messages buffered.
        /// </summary>
        public bool HasEventErrors { 
            get { return EventMessagesBuffer.Length > 0; } 
        }

        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe. BufferedEmailTraceListener is not threadsafe.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return false;
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
            EventMessagesBuffer = new StringBuilder(100000);
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
            if (!HasEventErrors)
                return;

            string allMessages = EventMessagesBuffer.ToString();
            string firstMessage = allMessages.Substring(0, allMessages.IndexOf("\n"));// EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            string subject = SanitiseSubject(
    traceFormatter.Format(SubjectTemplate, null, null, TraceEventType.Information, 0, ExtractSubject(firstMessage), null, null)
    );

            string body = traceFormatter.Format(BodyTemplate, null, null, TraceEventType.Information, 0, allMessages, null, null);
            EmailWriter.Send(subject, body, ToAddress);

            Clear();
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



        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            
            EventMessagesBufferAdd(traceFormatter.Format(TraceTemplate, eventCache, source, eventType, id, message, relatedActivityId, data));
        }

        protected virtual string DefaultTraceTemplate
        {
            get
            {
                return "{LOCALDATETIME:HH:mm:ss} {EVENTTYPE} [{THREADID}] {MESSAGE}";
            }
        }

        //protected virtual string DefaultTraceTemplate { get { return "[{THREADID}] {EVENTTYPE}: {MESSAGE}"; } }


        protected override void SendAllBeforeExit()
        {
            Send();
            base.SendAllBeforeExit();
        }



        // ================================================================================================================================

        void EventMessagesBufferAdd(string s)
        {
            EventMessagesBuffer.AppendLine(s);
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

    }

}
