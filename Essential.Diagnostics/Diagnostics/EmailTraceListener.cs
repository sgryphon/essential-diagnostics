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
    /// Listener that sends a formatted email containing the contents of the trace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sending an email is an expensive operation, so messages are queued and sent on
    /// a separate thread. If there is a flood of messages exceeding the queue size then
    /// messages will be dropped.
    /// </para>
    /// <para>
    /// It is strongly recommended to set a filter to only accept Warning and above errors,
    /// or otherwise reduce the number of trace events that are processed by this listener.
    /// </para>
    /// </remarks>
    public class EmailTraceListener : EmailTraceListenerBase
    {
        TraceFormatter traceFormatter = new TraceFormatter();

        public EmailTraceListener(string toAddress)
            : base(toAddress)
        {
            if (Filter == null)
            {
                Filter = new EventTypeFilter(SourceLevels.Warning);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
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

            SendEmail(subject, body, false);
        }

    }

}
