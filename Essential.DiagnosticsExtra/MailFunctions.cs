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

        internal SmtpClientPool(string hostName, int port)
        {
            pool = new Queue<SmtpClient>(maxConnections);
            for (int i = 0; i < maxConnections; i++)
            {
                SmtpClient client = new SmtpClient(hostName, port);
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

            Debug.WriteLine("Mail send completed.");
            Debug.WriteLine("Connections in pool: " + Pool.Count);

            MailSystemStatus status = MailSystemStatus.Ok;
            if (e.Error != null)
            {
                SmtpException smtpException = e.Error as SmtpException;
                if (smtpException == null)
                {
                    Trace.TraceError("It is strange that the exception caught is not SmtpException: " + e.ToString());
                    status = MailSystemStatus.Critical;
                }
                else
                {
                    Trace.TraceError("When sending message: " + e.Error.Message);
                    status = GetMailSystemStatus(smtpException.StatusCode);
                }
            }

            if (SendCompleted != null)
            {
                SendCompleted(sender, new MailStatusEventArgs(status));
            }

        }

        static MailSystemStatus GetMailSystemStatus(SmtpStatusCode code)
        {
            switch (code)
            {
                case SmtpStatusCode.BadCommandSequence:
                case SmtpStatusCode.CannotVerifyUserWillAttemptDelivery:
                case SmtpStatusCode.ClientNotPermitted:
                case SmtpStatusCode.CommandNotImplemented:
                case SmtpStatusCode.CommandParameterNotImplemented:
                case SmtpStatusCode.CommandUnrecognized:
                case SmtpStatusCode.ExceededStorageAllocation:
                case SmtpStatusCode.GeneralFailure:
                case SmtpStatusCode.InsufficientStorage:
                case SmtpStatusCode.LocalErrorInProcessing:
                case SmtpStatusCode.MailboxBusy:
                case SmtpStatusCode.MailboxNameNotAllowed:
                case SmtpStatusCode.MailboxUnavailable:
                case SmtpStatusCode.MustIssueStartTlsFirst:
                case SmtpStatusCode.ServiceClosingTransmissionChannel:
                case SmtpStatusCode.ServiceNotAvailable:
                case SmtpStatusCode.UserNotLocalTryAlternatePath:
                case SmtpStatusCode.UserNotLocalWillForward:
                case SmtpStatusCode.SyntaxError:
                    return MailSystemStatus.Critical;


                case SmtpStatusCode.HelpMessage:
                case SmtpStatusCode.Ok:
                case SmtpStatusCode.ServiceReady:
                case SmtpStatusCode.StartMailInput:
                case SmtpStatusCode.SystemStatus:
                case SmtpStatusCode.TransactionFailed:
                    return MailSystemStatus.TemporaryProblem;
                default:
                    return MailSystemStatus.TemporaryProblem;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>False if no client is available.</returns>
        internal MailSystemStatus SendAsync(MailMessage message)
        {
            SmtpClient client = null;
            try
            {
                client = Pool.Dequeue();
                client.SendAsync(message, message);// the client will be enqueued when the thread finish. If the sending fails, the message will not be resent. This is intended.
                Debug.WriteLine("Mail sent async.");
                return MailSystemStatus.Ok;
            }
            catch (InvalidOperationException) // the pool is empty.
            {
                Debug.WriteLine("Connection pool empty.");
                return MailSystemStatus.EmptyConnectionPool;
            }
            catch (SmtpException e)
            {
                Trace.TraceError("Could not send mail: " + e.ToString());
                Debug.WriteLine("Status: " + e.StatusCode);
                Pool.Enqueue(client);
                return GetMailSystemStatus(e.StatusCode);
            }
        }

        /// <summary>
        /// The subscriber which is a MailMessage queue should then dequeue a message and send it.
        /// </summary>
        internal event EventHandler<MailStatusEventArgs> SendCompleted;

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

    internal class MailStatusEventArgs : EventArgs
    {
        public MailSystemStatus Status { get; private set; }
        public MailStatusEventArgs(MailSystemStatus status)
        {
            Status = status;
        }
    }


    internal enum MailSystemStatus { Ok, EmptyConnectionPool, TemporaryProblem, Critical };

    /// <summary>
    /// http://stackoverflow.com/questions/2510975/c-sharp-object-pooling-pattern-implementation
    /// MailMessage queue in which items will be sent out by a pool of SmtpClient objects.
    /// </summary>
    public class MailMessageQueue : IDisposable
    {
        Queue<MailMessage> queue;

        static object queueLock = new object();

        static System.Threading.ReaderWriterLock rwLock = new System.Threading.ReaderWriterLock();

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

        bool acceptItem = true;

        public bool AcceptItem
        {
            get
            {
                rwLock.AcquireReaderLock(10);
                try
                {
                    return acceptItem;
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }
            }

            set
            {
                rwLock.AcquireWriterLock(100);
                try
                {
                    acceptItem = value;
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }
        }

        SmtpClientPool clientPool;

        public MailMessageQueue(string hostName, int port)
        {
            clientPool = new SmtpClientPool(hostName, port);
            clientPool.SendCompleted += new EventHandler<MailStatusEventArgs>(clientPool_SendCompleted);
            queue = new Queue<MailMessage>();
        }

        public MailMessageQueue(string hostName)
            : this(hostName, 25)
        {

        }

        /// <summary>
        /// So after a message is sent, the first message in the queue will be dequeued and sent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPool_SendCompleted(object sender, MailStatusEventArgs e)
        {
            if (e.Status == MailSystemStatus.Critical)
            {
                AcceptItem = false;
                Trace.TraceInformation("Mail system has critical problem. Not to accept more messages.");
                return;
            }

            SendOne();
        }

        /// <summary>
        /// When a message is enqueued, the first in the queue will be dequeued and sent.
        /// </summary>
        /// <param name="message"></param>
        public void AddAndSendAsync(MailMessage message)
        {
            if (!AcceptItem)
            {
                Debug.WriteLine("Hey, not accept item probably because of smtp problem.");
                return;
            }

            Queue.Enqueue(message);
            Debug.WriteLine("Messages in queue after adding: " + Queue.Count);
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
                Debug.WriteLine("No message in queue.");
                return;
            }

            MailSystemStatus status = clientPool.SendAsync(messageToSend);
            switch (status)
            {
                case MailSystemStatus.Ok:
                    break;
                case MailSystemStatus.EmptyConnectionPool:
                case MailSystemStatus.TemporaryProblem:
                    Queue.Enqueue(messageToSend);
                    break;
                case MailSystemStatus.Critical:
                    Queue.Enqueue(messageToSend);
                    AcceptItem = false;
                    Trace.TraceInformation("Mail system has critical problem. Message queue will not receive further items.");
                    break;
                default:
                    Trace.TraceWarning("Hey, what's up with new staus?");
                    break;
            }
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

                    clientPool.Dispose();

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
