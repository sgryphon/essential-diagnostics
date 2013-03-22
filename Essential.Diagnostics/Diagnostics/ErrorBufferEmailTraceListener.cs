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
    public class ErrorBufferEmailTraceListener : EmailTraceListenerBase
    {
        public ErrorBufferEmailTraceListener(string toAddress)
            : base(toAddress)
        {
            Filter = new EventTypeFilter(SourceLevels.Warning);

            EventMessagesBuffer = new StringBuilder();
        }

        void EventMessagesBufferAdd(string s)
        {
            EventMessagesBuffer.AppendLine(s);
        }

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            if (eventType <= TraceEventType.Error)//Error or Critical
            {
                EventMessagesBufferAdd(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
                    + "Error: " + message);
                EventMessagesBufferAdd("  Call Stack: " + eventCache.Callstack);
            }
            else if (eventType == TraceEventType.Warning)
            {
                EventMessagesBufferAdd(StartupInfo.GetISO8601Text(eventCache.DateTime) + " "
                    + "Warning: " + message);
            }
            else
            {
                Debug.Fail("Hey, not Warning but " + eventType);
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

            string body = EventMessagesBuffer.ToString();
            string firstMessage = body.Substring(0, body.IndexOf("\n"));// EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            string subject = MailMessageHelper.ExtractSubject(firstMessage);
            MessageQueue.AddAndSendAsync(new MailMessage(FromAddress, ToAddress, MailMessageHelper.SanitiseSubject(subject), body));
            ClearEventMessagesBuffer();
        }

        protected override void SendAllBeforeExit()
        {
            SendAndClear();
            base.SendAllBeforeExit();
        }

        static ErrorBufferEmailTraceListener FindListener()
        {
            ErrorBufferEmailTraceListener myListener = null;
            foreach (TraceListener t in Trace.Listeners)
            {
                myListener = t as ErrorBufferEmailTraceListener;
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
