using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.ComponentModel;
using Essential.Net.Mail;


namespace Essential.Diagnostics
{
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

        bool acceptItem = true;

        /// <summary>
        /// Whether to accept more mail message. Thread safe.
        /// </summary>
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

        Essential.Net.Mail.SmtpClientPool clientPool;

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
        /// When a message is enqueued, the first in the queue will be dequeued and sent. This function is thread safe.
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

        /// <summary>
        /// Fired when the queue becomes empty after the last message in queue is sent.
        /// </summary>
        public event EventHandler QueueEmpty;

        /// <summary>
        /// Send a message in the queue out. If the queue is empty, this function will fire event QueueEmpty.
        /// </summary>
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

        public bool Idle
        {
            get
            {
                return (Count == 0) && //No message in queue
                    (clientPool.Count == clientPool.MaxConnections);//No mail client is sending message.
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
