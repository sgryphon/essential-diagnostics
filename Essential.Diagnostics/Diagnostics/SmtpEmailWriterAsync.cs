using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net.Mail;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Send Email message asynchronously through a pool of SmtpClient connections.
    /// </summary>
    internal class SmtpEmailWriterAsync : Abstractions.ISmtpEmailHelper, IDisposable
    {
        #region ISmtpEmailHelper Members

        void Abstractions.ISmtpEmailHelper.Send(string subject, string body, string recipient, string from)
        {
            MessageQueue.AddAndSendAsync(CreateMailMessage(subject, body, recipient, from));//EmailMessage will be disposed in the queue after being sent.
        }

        bool Abstractions.ISmtpEmailHelper.Busy
        {
            get 
            {
                if (messageQueue != null)
                {
                    return !MessageQueue.Idle;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        public SmtpEmailWriterAsync(int maxConnections)
        {
            this.maxConnections = maxConnections;
        }

        int maxConnections;

        MailMessageQueue messageQueue;

        static object objectLock = new object();

        /// <summary>
        /// Mail message queue created upon the first warning trace.
        /// </summary>
        protected MailMessageQueue MessageQueue
        {
            get
            {
                lock (objectLock)
                {
                    if (messageQueue == null)
                    {
                        messageQueue = new MailMessageQueue(maxConnections);
                        Debug.WriteLine("MessageQueue is created with some connections: " + maxConnections);
                    }
                }

                return messageQueue;
            }
        }

        static MailMessage CreateMailMessage(string subject, string body, string recipient, string from)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.IsBodyHtml = false;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(from);
            mailMessage.To.Add(recipient);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            return mailMessage;
        }

        bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (messageQueue != null)
                    {
                        messageQueue.Dispose();
                    }
                }

                disposed = true;
            }
        }


    }
}
