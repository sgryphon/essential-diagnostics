using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

namespace Essential.Net.Mail
{
    // SmtpClient connection pool.
    // Design of the pool is inspired by this post http://stackoverflow.com/questions/2510975/c-sharp-object-pooling-pattern-implementation
    internal class SmtpClientPool : IDisposable
    {
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
        Queue<SmtpClient> pool;

        static object poolLock = new object();

        const int defaultMaxConnections = 2;

        internal SmtpClientPool():this(defaultMaxConnections)
        {


        }

        internal SmtpClientPool(int maxConnections)
        {
            MaxConnections = maxConnections;
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

        /// <summary>
        /// number of connections left in pool.
        /// </summary>
        internal int Count
        {
            get
            {
                lock (poolLock)
                {
                    return pool.Count;
                }
            }
        }

        internal int MaxConnections
        {
            get;
            private set;
        }

        void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient client = sender as SmtpClient;
            //Debug.Assert(client != null, "Why is this ender not SmtpClient?");
            PoolEnqueue(client);

            MailMessage messageSent = e.UserState as MailMessage;
            //Debug.Assert(messageSent != null);

            //Debug.WriteLine("Mail send completed at " + DateTime.Now.ToString());

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
}
