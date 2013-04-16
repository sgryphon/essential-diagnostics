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
            string subject = SanitiseSubject(
                traceFormatter.Format(SubjectTemplate, eventCache, source, eventType, id, 
                    ExtractSubject(message), 
                    relatedActivityId, data
                    )
                );
            string messageformated = traceFormatter.Format(BodyTemplate, eventCache, source, eventType, id, 
                message, relatedActivityId, data);

            SendEmail(subject, messageformated);
        }

    }

}
