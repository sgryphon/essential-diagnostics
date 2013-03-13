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
    internal class SmtpClientPool : IDisposable
    {
        Queue<SmtpClient> pool;

        static object poolLock = new object();

        Queue<SmtpClient> Pool
        {
            get
            {
                lock (poolLock)
                {
                    return pool;
                }
            }
        }

        const int maxConnections = 4;

        public SmtpClientPool(string hostName)
        {
            pool = new Queue<SmtpClient>(maxConnections);
            for (int i = 0; i < maxConnections; i++)
            {
                SmtpClient client = new SmtpClient(hostName);
                client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
                Pool.Enqueue(client);
            }
        }

        void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient client = sender as SmtpClient;
            Trace.Assert(client != null, "Why is this ender not SmtpClient?");
            Pool.Enqueue(client);
            Trace.Assert(Pool.Count <= maxConnections, "Hey, pool size should be fixed.");

            MailMessage messageSent = e.UserState as MailMessage;
            messageSent.Dispose();

            if (SendCompleted != null)
            {
                SendCompleted(sender, new EventArgs());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>False if no client is available.</returns>
        public bool SendAsync(MailMessage message)
        {
            try
            {
                SmtpClient client = Pool.Dequeue();
                client.SendAsync(message, message);// the client will be enqueued when the thread finish. If the sending fails, the message will not be resent. This is intended.
                return true;
            }
            catch (InvalidOperationException) // the pool is empty.
            {
                return false;
            }
        }

        /// <summary>
        /// The subscriber which is a MailMessage queue should then dequeue a message and send it.
        /// </summary>
        public event EventHandler SendCompleted;

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
                    if (typeof(SmtpClient).GetInterface("IDisposable") != null)
                    {
                        SmtpClient client;
                        try
                        {
                            while ((client = Pool.Dequeue()) != null)
                            {
                                (client as IDisposable).Dispose();
                            }

                        }
                        catch (InvalidOperationException)
                        {
                            //do nothing
                        }
                    }
                }

                disposed = true;
            }
        }


    }

    /// <summary>
    /// http://stackoverflow.com/questions/2510975/c-sharp-object-pooling-pattern-implementation
    /// 
    /// </summary>
    internal class MessageQueue : IDisposable
    {
        Queue<MailMessage> queue;

        static object queueLock = new object();

        Queue<MailMessage> Queue
        {
            get
            {
                lock (queueLock)
                {
                    return queue;
                }
            }
        }

        SmtpClientPool clientPool;

        public MessageQueue(SmtpClientPool clientPool)
        {
            this.clientPool = clientPool;
            clientPool.SendCompleted += new EventHandler(clientPool_SendCompleted);
            queue = new Queue<MailMessage>();
        }

        /// <summary>
        /// So after a message is sent, the first message in the queue will be dequeued and sent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPool_SendCompleted(object sender, EventArgs e)
        {
            SendOne();
        }

        /// <summary>
        /// When a message is enqueued, the first in the queue will be dequeued and sent.
        /// </summary>
        /// <param name="message"></param>
        public void AddAndSendAsync(MailMessage message)
        {
            Queue.Enqueue(message);
            SendOne();
        }

        void SendOne()
        {
            MailMessage messageToSend;
            try
            {
                messageToSend = Queue.Dequeue();
            }
            catch (InvalidOperationException)// nothing in message queue
            {
                return;
            }

            clientPool.SendAsync(messageToSend);
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
                        MailMessage item;
                        try
                        {
                            while ((item = Queue.Dequeue()) != null)
                            {
                                item.Dispose();
                            }

                        }
                        catch (InvalidOperationException)
                        {
                            //do nothing;
                        }
                   
                }

                disposed = true;
            }
        }


    }




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
