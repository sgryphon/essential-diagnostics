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
    /// Send Email against TraceWarning and TraceError.
    /// </summary>
    public class EmailTraceListener : EmailTraceListenerBase
    {
        public EmailTraceListener()
            : base()
        {
        }

        public EmailTraceListener(string toAddress)
            : base(toAddress)
        {
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (String.IsNullOrEmpty(message))
                return;

            if (eventCache == null)
                throw new ArgumentNullException("eventCache");

            string subject = MailMessageHelper.ExtractSubject(message);

            string messageformated;

            if (eventType <= TraceEventType.Error)//Error or Critical
            {
                messageformated = "Error: "
                    + message + Environment.NewLine
                    + "  Call Stack: " + eventCache.Callstack;
                SendEmailAsync(subject, messageformated);
            }
            else if (eventType == TraceEventType.Warning)
            {
                messageformated = "Warning: " + message;
                SendEmailAsync(subject, messageformated);
            }
            //ignore the other types
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            // do nothing  base.TraceEvent(eventCache, source, eventType, id);
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            //do nothing
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            //do nothing
        }

    }

}
