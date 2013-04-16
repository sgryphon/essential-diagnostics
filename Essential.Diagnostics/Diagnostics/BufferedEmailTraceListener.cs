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
        object bufferLock = new object();
        StringBuilder eventMessagesBuffer = new StringBuilder(10000);
        string firstMessage;

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

                SendEmail(subject, body);
            }
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

        //protected virtual string DefaultTraceTemplate { get { return "[{THREADID}] {EVENTTYPE}: {MESSAGE}"; } }


        protected override void SendAllBeforeExit()
        {
            Send();
            base.SendAllBeforeExit();
        }



        // ================================================================================================================================

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
