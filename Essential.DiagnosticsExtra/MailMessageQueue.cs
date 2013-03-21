using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.ComponentModel;


namespace Essential.Diagnostics
{
    /// <summary>
    /// SmtpClient connection pool.
    /// </summary>
    /// <![CDATA[The design of the pool is inspired by this post http://stackoverflow.com/questions/2510975/c-sharp-object-pooling-pattern-implementation]]>
    internal class SmtpClientPool : IDisposable
    {
        Queue<SmtpClient> pool;

        static object poolLock = new object();

        //Queue<SmtpClient> Pool
        //{
        //    get
        //    {
        //        lock (poolLock)
        //        {
        //            return pool;
        //        }
        //    }
        //}

        const int defaultMaxConnections = 2;

        internal SmtpClientPool():this(defaultMaxConnections)
        {


        }

        internal SmtpClientPool(int maxConnections)
        {
            pool = new Queue<SmtpClient>(maxConnections);
            for (int i = 0; i < maxConnections; i++)
            {
                SmtpClient client = new SmtpClient();
                client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
                PoolEnqueue(client);
            }
        }

        internal SmtpClientPool(string hostName, int port, int maxConnections)
        {
            pool = new Queue<SmtpClient>(maxConnections);
            for (int i = 0; i < maxConnections; i++)
            {
                SmtpClient client = new SmtpClient(hostName, port);
                client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
                PoolEnqueue(client);
            }
        }

        SmtpClient PoolDequeue()
        {
            lock (poolLock)
            {
                return pool.Dequeue();
            }
        }

        void PoolEnqueue(SmtpClient client)
        {
            lock (poolLock)
            {
                pool.Enqueue(client);
            }
        }

        void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient client = sender as SmtpClient;
            Debug.Assert(client != null, "Why is this ender not SmtpClient?");
            PoolEnqueue(client);

            MailMessage messageSent = e.UserState as MailMessage;
            Debug.Assert(messageSent != null);

            Debug.WriteLine("Mail send completed at " + DateTime.Now.ToString());

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
                    Trace.TraceError("When sending message: " + e.Error.ToString());
                    status = GetMailSystemStatus(smtpException.StatusCode);
                }
            }

            if (SendCompleted != null)
            {
                SendCompleted(sender, new MailStatusEventArgs(status, messageSent));//the handler should dispose messageSent
            }
            else
            {
                messageSent.Dispose();
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
                case SmtpStatusCode.StartMailInput:
                case SmtpStatusCode.SystemStatus:
                case SmtpStatusCode.TransactionFailed:
                    return MailSystemStatus.TemporaryProblem;
                case SmtpStatusCode.Ok:
                case SmtpStatusCode.ServiceReady:
                    return MailSystemStatus.Ok;

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
                client = PoolDequeue();
                client.SendAsync(message, message);// the client will be enqueued when the thread finish. If the sending fails, the message will not be resent. This is intended.
                Debug.WriteLine("Mail sent async.");
                return MailSystemStatus.Ok;
            }
            catch (InvalidOperationException) // the pool is empty because the connections are all in use.
            {
                Debug.WriteLine("Connection pool empty.");
                return MailSystemStatus.EmptyConnectionPool;
            }
            catch (SmtpException e)//Generally it is the problem during the handshaking to the host.
            {
                Trace.TraceError("Could not send mail: " + e.ToString());
                Debug.WriteLine("Status: " + e.StatusCode);
                PoolEnqueue(client);
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
                    if (typeof(SmtpClient).GetInterface("IDisposable") != null)//In .NET 4, SmtpClient has IDisposable.
                    {
                        SmtpClient client;
                        try
                        {
                            while ((client = PoolDequeue()) != null)
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

    internal enum MailSystemStatus { 
        Ok, 
        EmptyConnectionPool, 
        TemporaryProblem, //then worth of retry
        Critical //then the Email message queue should stop accepting new messages.
    };

    /// <summary>
    /// MailMessage queue in which items will be sent by a pool of SmtpClient objects. Client codes just neeed to call AddAndSendAsync().
    /// </summary>
    /// <remarks>
    /// Upon critical conditions with a Smtp server, the MailMessageQueue will refuse to accept further message quitely, with AcceptItem is set to false.
    /// 
    /// Because the Email messages will be sent in multiple threads, the send time of Email messages may not be in the exact order and the exact time of the creation of the message, tehrefore,
    /// it is recommended that the Email subject or body should log the datetime of the trace message.
    /// </remarks>
    public class MailMessageQueue : IDisposable
    {
        Queue<MailMessage> queue;

        static object queueLock = new object();

        static System.Threading.ReaderWriterLock rwLock = new System.Threading.ReaderWriterLock();

        //Queue<MailMessage> MessageQueue
        //{
        //    get
        //    {
        //        lock (queueLock)
        //        {
        //            return queue;
        //        }
        //    }
        //}

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

        public MailMessageQueue(string hostName, int port, int maxConnections)
        {
            clientPool = new SmtpClientPool(hostName, port, maxConnections);
            clientPool.SendCompleted += new EventHandler<MailStatusEventArgs>(clientPool_SendCompleted);
            queue = new Queue<MailMessage>();
        }

        public MailMessageQueue(string hostName)
            : this(hostName, 25, 2)
        {

        }

        public MailMessageQueue(int maxConnections)
        {
            clientPool = new SmtpClientPool(maxConnections);
            clientPool.SendCompleted += new EventHandler<MailStatusEventArgs>(clientPool_SendCompleted);
            queue = new Queue<MailMessage>();
        }

        MailMessage Dequeue()
        {
            lock (queueLock)
            {
                return queue.Dequeue();
            }
        }

        void Enqueue(MailMessage message)
        {
            lock (queueLock)
            {
                queue.Enqueue(message);
            }
        }

        /// <summary>
        /// So after a message is sent, the first message in the queue will be dequeued and sent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// It seems that SmtpClient will dispose MailMessage before sending another message. Therefore, it is not nice to enqueue
        /// </remarks>
        void clientPool_SendCompleted(object sender, MailStatusEventArgs e)
        {
            switch (e.Status)
            {
                case MailSystemStatus.TemporaryProblem:
                case MailSystemStatus.Critical:
                    AcceptItem = false;
                    Trace.TraceInformation("Mail system has critical problem. Not to accept more messages.");//and the message 
                    return;
                case MailSystemStatus.EmptyConnectionPool:
                    Trace.Fail("Is it possible to have EmptyConnectionPool here?");
                    return;
                default:
                    e.MailMessage.Dispose();
                    break;
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

            Enqueue(message);
            Debug.WriteLine("Messages in queue after adding: " + Count);
            SendOne();
        }

        public event EventHandler QueueEmpty;

        void SendOne()
        {
            MailMessage messageToSend;
            try
            {
                messageToSend = Dequeue();
            }
            catch (InvalidOperationException)// nothing in message queue
            {
                Debug.WriteLine("No message in queue.");
                if (QueueEmpty != null)
                {
                    Action<int> d = (dummy) =>
                    {
                        QueueEmpty(this, new EventArgs());
                    };
                    d.BeginInvoke(0, null, null);
                }
                return;
            }

            MailSystemStatus status = clientPool.SendAsync(messageToSend);
            switch (status)
            {
                case MailSystemStatus.Ok:
                    break;
                case MailSystemStatus.EmptyConnectionPool:
                case MailSystemStatus.TemporaryProblem:
                    Debug.WriteLine("Message enqueued back.");
                    Enqueue(messageToSend);
                    break;
                case MailSystemStatus.Critical:
                    Enqueue(messageToSend);
                    AcceptItem = false;
                    Trace.TraceInformation("Mail system has critical problem. Message queue will not receive further items.");
                    break;
                default:
                    Trace.TraceWarning("Hey, what's up with new staus?");
                    break;
            }
        }


        /// <summary>
        /// Number of messages in queue. This properly is generally accessed for diagnostics purpose when AcceptItem becomes false.
        /// </summary>
        public int Count { get { lock (queueLock) { return queue.Count; } } }

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
                        while ((item = Dequeue()) != null)
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


}
