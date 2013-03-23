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
            Filter = new EventTypeFilter(SourceLevels.Warning);
        }


        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            if (String.IsNullOrEmpty(message))
                return;

            Debug.Assert(eventCache != null);

            string subject = MailMessageHelper.ExtractSubject(eventCache, SubjectTemplate, message);

            string messageformated = MailMessageHelper.ComposeMessage(eventCache, MessageTemplate, message);
            SendEmailAsync(subject, messageformated);
        }


    }

}
