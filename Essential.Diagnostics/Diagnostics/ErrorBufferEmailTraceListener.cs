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
    /// Intended to be used in console apps which will send all warning/error traces via Email at the end of the process.
    /// 
    /// The listener will put error and warning trace messages into a buffer. 
    /// 
    /// 
    /// </summary>
    public class BufferedEmailTraceListener : EmailTraceListenerBase
    {
        public BufferedEmailTraceListener(string toAddress)
            : base(toAddress)
        {
            if (Filter == null)
            {
                Filter = new EventTypeFilter(SourceLevels.Warning);
            }

            EventMessagesBuffer = new StringBuilder();
        }

        TraceFormatter traceFormatter = new TraceFormatter();
        
        void EventMessagesBufferAdd(string s)
        {
            EventMessagesBuffer.AppendLine(s);
        }

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            EventMessagesBufferAdd(traceFormatter.Format(TraceTemplate, eventCache, source, eventType, id, message, relatedActivityId, data));
        }

        protected override string DefaultTraceTemplate
        {
            get
            {
                return "{LOCALDATETIME:HH:mm:ss} {EVENTTYPE} [{THREADID}] {MESSAGE}";
            }
        }
        /// <summary>
        /// Message buffer
        /// </summary>
        public StringBuilder EventMessagesBuffer { get; private set; }

        public void ClearEventMessagesBuffer()
        {
            EventMessagesBuffer = new StringBuilder();
        }

        /// <summary>
        /// The buffer is not empty.
        /// </summary>
        public bool HasEventErrors { get { return EventMessagesBuffer.Length > 0; } }

        /// <summary>
        /// Send all trace messages in buffer by 1 Email message. If the buffer is empty, this function does nothing.
        /// </summary>
        /// <remarks>The buffer is then clear.</remarks>
        public void SendAndClear()
        {
            if (!HasEventErrors)
                return;

            string allMessages = EventMessagesBuffer.ToString();
            string firstMessage = allMessages.Substring(0, allMessages.IndexOf("\n"));// EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            string subject = MailMessageHelper.SanitiseSubject(
    traceFormatter.Format(SubjectTemplate, null, null, TraceEventType.Information, 0, MailMessageHelper.ExtractSubject(firstMessage),    null, null)
    );

            string body = traceFormatter.Format(BodyTemplate, null, null, TraceEventType.Information, 0, allMessages, null, null);
            MessageQueue.AddAndSendAsync(new MailMessage(FromAddress, ToAddress, subject, body));
            ClearEventMessagesBuffer();
        }

        protected override void SendAllBeforeExit()
        {
            SendAndClear();
            base.SendAllBeforeExit();
        }

        static BufferedEmailTraceListener FindListener()
        {
            BufferedEmailTraceListener myListener = null;
            foreach (TraceListener t in Trace.Listeners)
            {
                myListener = t as BufferedEmailTraceListener;
                if (myListener != null)
                    return myListener;
            }

            Trace.TraceError("You want to use ErrorBufferTraceListener, but there's none in Trace.Listeners, probably not defined in the config file.");
            return null;
        }

        /// <summary>
        /// While the listener will send an Email message at the end of the hosting process. This function allow your app codes to send the Email message earlier. For example, you might want to
        /// send one message at the end of each loop. 
        /// </summary>
        public static void SendMailOfEventMessages()
        {
            var listener = FindListener();
            if (listener != null)
            {
                listener.SendAndClear();
            }
        }

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        public static void Clear()
        {
            var listener = FindListener();
            if (listener != null)
            {
                listener.ClearEventMessagesBuffer();
            }
        }
    }

}
