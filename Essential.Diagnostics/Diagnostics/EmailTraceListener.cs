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
        public EmailTraceListener(string toAddress)
            : base(toAddress)
        {
            if (Filter == null)
            {
                Filter = new EventTypeFilter(SourceLevels.Warning);
            }
        }

        TraceFormatter traceFormatter = new TraceFormatter();

        /// <summary>
        /// True.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            if (String.IsNullOrEmpty(message))
                return;

            Debug.Assert(eventCache != null);

            string subject = MailMessageHelper.SanitiseSubject(
                traceFormatter.Format(SubjectTemplate, eventCache, source, eventType, id, 
                MailMessageHelper.ExtractSubject(message), 
                relatedActivityId, data)
                );


            string messageformated = traceFormatter.Format(BodyTemplate, eventCache, source, eventType, id, message, relatedActivityId, data);
            SendEmail(subject, messageformated);
        }

    }

}
