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
    /// Put error and warning trace messages into a buffer, taking care of Trace.Trace*() while ignoring Trace.Write*(). The client codes may want to get
    /// all messages at the end of a business operation in order to send out the messages in one batch.
    /// For error message, call stack and local datetime will be accompanied.
    /// 
    /// 
    /// </summary>
    public class ErrorBufferEmailTraceListener : EmailTraceListenerBase
    {
        public ErrorBufferEmailTraceListener()
        {
            EventMessagesBuffer = new StringBuilder();
        }

        public ErrorBufferEmailTraceListener(string name)
            : base(name)
        {
            EventMessagesBuffer = new StringBuilder();
        }

        void EventMessagesBufferAdd(string s)
        {
            EventMessagesBuffer.AppendLine(s);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
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
            //Ignore other event type
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            // do nothing  base.TraceEvent(eventCache, source, eventType, id);
        }

        public override void Write(string message)
        {
            //do nothing
        }

        public override void WriteLine(string message)
        {
            //do nothing
        }

        /// <summary>
        /// Message buffer
        /// </summary>
        public StringBuilder EventMessagesBuffer { get; private set; }

        public void ClearEventMessagesBuffer()
        {
            EventMessagesBuffer = new StringBuilder();
        }

        public bool HasEventErrors { get { return EventMessagesBuffer.Length > 0; } }

        protected void SendEventMessages()
        {
            if (!HasEventErrors)
                return;

            string body = EventMessagesBuffer.ToString();
            string firstMessage = body.Substring(0, body.IndexOf("\n"));// EventMessagesBuffer.Count == 0 ? String.Empty : EventMessagesBuffer[0];
            Debug.WriteLine("firstMessage: " + firstMessage);
            string subject = MailMessageHelper.ExtractSubject(firstMessage);
            SendEmail(subject, body);
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
        /// This is commonly called at the end of a process.
        /// </summary>
        public static void SendMailOfEventMessages()
        {
            var listener = FindListener();
            if (listener != null)
            {
                listener.SendEventMessages();
            }
        }

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
