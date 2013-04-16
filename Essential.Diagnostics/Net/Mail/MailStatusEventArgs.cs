using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Essential.Net.Mail
{
    internal class MailStatusEventArgs : EventArgs
    {
        public MailSystemStatus Status { get; private set; }

        /// <summary>
        /// Assigned when Status is negative, so a callback function may dispose the mail message.
        /// </summary>
        public MailMessage MailMessage { get; private set; }

        public MailStatusEventArgs(MailSystemStatus status, MailMessage mailMessage)
        {
            Status = status;
            MailMessage = mailMessage;
        }
    }
}
