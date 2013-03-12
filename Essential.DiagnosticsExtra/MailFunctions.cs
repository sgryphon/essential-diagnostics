using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Xml;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace Essential.Diagnostics
{
    internal class MailInfo
    {
        internal string From { get; set; }
        internal string Recipient { get; set; }
        internal DateTime TimeSent { get; set; }

        internal Exception Exception { get; set; }
    }

    /// <summary>
    /// A wrapper of Smtpclient. Though Smtpclient in .NET Framework has method SendAsync, the method apparently only do the transmitting in a new thread, while the initial hand shaking is still in the caller thread.
    /// So this class will do the whole thing in a new thread.
    /// </summary>
    internal class SmtpClientAsync : IDisposable
    {
        SmtpClient smtpClient;

        bool sendOnce;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smtpServerName"></param>
        /// <param name="sendOnce">True to dispose the connection after sending message once.</param>
        internal SmtpClientAsync(string smtpServerName, bool sendOnce)
        {
            smtpClient = new SmtpClient(smtpServerName);
            this.sendOnce = sendOnce;
        }

        /// <summary>
        /// Client codes may optionally handle something when the tread is completed.
        /// </summary>
        internal event SendCompletedEventHandler SendCompleted;

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
                    IDisposable obj = smtpClient as IDisposable;//In .net 2, SmtpClient has no IDisposable define while .net 4 it has. This will make the dispose() function be called in .NET 4.
                    if (obj != null)
                    {
                        obj.Dispose();
                        Debug.WriteLine("Mail client disposed.");
                    }
                }

                disposed = true;
            }
        }

        ~SmtpClientAsync()
        {
           Dispose();
        }

        /// <summary>
        /// Send mail via a spin off thread. And mailMessage will be disposed in the thread.
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="mailMessage"></param>
        /// <returns></returns>
        /// <remarks>If the host process is terminated, the thread running the sending will be terminated as well, therefore the last few error message traced might be lost.</remarks>
        internal IAsyncResult SendAsync(MailMessage mailMessage)
        {
            MailInfo state = new MailInfo()
            {
                From = mailMessage.From.Address,
                Recipient = mailMessage.To[0].Address,
                TimeSent = DateTime.Now,
            };

            SendEmailHandler d = (mm) =>
            {
                if (!Send(mm, state))
                {
                    Debug.WriteLine("Hey, send fails, please check trace following.");
                }
            };

            return d.BeginInvoke(mailMessage, EmailSentCallback, state);
        }

        void EmailSentCallback(IAsyncResult asyncResult)
        {
            AsyncResult resultObj = (AsyncResult)asyncResult;
            SendEmailHandler d = (SendEmailHandler)resultObj.AsyncDelegate;
            MailInfo state = null;
            try
            {
                d.EndInvoke(asyncResult);
                Debug.WriteLine("IsCompleted: " + asyncResult.IsCompleted);
                state = (MailInfo)asyncResult.AsyncState;
                if (SendCompleted != null)
                {
                    SendCompleted(this, new AsyncCompletedEventArgs(state.Exception, false, state));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The thread is terminated, and general exception is caught. " + e.ToString());
                SendCompleted(this, new AsyncCompletedEventArgs(e, false, null));
                throw;
            }
        }

        /// <summary>
        /// Send and dispose mailMessage.
        /// </summary>
        /// <param name="mailMessage"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal bool Send(MailMessage mailMessage, MailInfo state)
        {
            Debug.WriteLine("SmtpClient: " + smtpClient.GetType().AssemblyQualifiedName);
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (SmtpException e)
            {
                Debug.WriteLine("Caught in Send. " + e.ToString());
                state.Exception = e;
            }
            catch (IOException e)
            {
                Debug.WriteLine("Caught in Send. " + e.ToString());
                state.Exception = e;
            }
            finally
            {
                mailMessage.Dispose();
                if (sendOnce)
                {
                    Dispose();
                }
            }

            return false;

        }
    }

    public static class MailUtil
    {
        internal static void AssignEmailSubject(MailMessage mailMessage, string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return;

            if (mailMessage == null)
                throw new ArgumentNullException("mailMessage");

            const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
            if (subject.Length > 254)
                subject = subject.Substring(0, subjectMaxLength);

            try
            {
                for (int i = 0; i < subject.Length; i++)
                {
                    if (Char.IsControl(subject[i]))
                    {
                        mailMessage.Subject = subject.Substring(0, i);
                        return;
                    }
                }
                mailMessage.Subject = subject;

            }
            catch (ArgumentException)
            {
                mailMessage.Subject = "Invalid subject removed by TraceListener";
            }
        }

        /// <summary>
        /// Send mail via a spin off thread.
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="mailMessage"></param>
        /// <returns></returns>
        /// <remarks>If the host process is terminated, the thread running the sending will be terminated as well, therefore the last few error message traced might be lost.</remarks>
        public static void SendEmailAsync(string smtpServer, MailMessage mailMessage)
        {
            SmtpClientAsync client = new SmtpClientAsync(smtpServer, true);//SmtpClient will be disposed after Send().
            client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
            client.SendAsync(mailMessage);
        }

        static void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
#if DEBUG
            MailInfo info = (MailInfo)e.UserState;
            Debug.WriteLine(String.Format("Email sent to {0} at {1}", info.Recipient, info.TimeSent));
#endif
        }

        public static void SendEmail(string smtpServer, MailMessage mailMessage)
        {
            using (SmtpClientAsync client = new SmtpClientAsync(smtpServer, true))
            {
                client.Send(mailMessage, new MailInfo());
            }
        }


    }

    internal delegate void SendEmailHandler(MailMessage mailMessage);


}
