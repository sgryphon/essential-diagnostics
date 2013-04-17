using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace Essential.Net.Mail
{
    // This class is needed because although SmtpClient has a SendAsync() method, it also states:
    // "After calling SendAsync, you must wait for the e-mail transmission to complete before attempting 
    // to send another e-mail message using Send or SendAsync."

    // That means any following message must block until the SmtpClient is available, but of course
    // there isn't really any status flag or anything, so you would need to implement that all
    // yourself (meanwhile blocking the calling thread).

    // So, to avoid blocking the calling thread there are two possible options.
    // One is to spin off a sending thread for each message that blocks until it can proceed;
    // threads are expensive, so the ThreadPool may be an option (to queue work up),
    // but generally you have less control over the ThreadPool (it is shared with the system).
  
    // The other alternative is to have a queue in memory where the calling thread adds to the
    // queue, and then have a background thread (or several) that keeps polling and dequeues messages 
    // when available and sends them. This may give a bit better visibility to the size of the queue
    // (rather than hidden off as blocked threads or ThreadPool queued work items). e.g. if the size
    // of the queue is too large it can start "dropping packets", and simply not send messages when
    // flooded.

    // Note that the blocking threads / ThreadPool option pretty much works the same (i.e. the messages
    // are queued up somewhere in memory), just with less control.

    // Also note that in both cases you almost might as well send the email message synchronously, as
    // you need to wait for it to complete anyway.

    internal class SmtpWorkerPool : IDisposable
    {
        int maxConnections;
        Queue<SmtpWorkerAsyncResult> messageQueue = new Queue<SmtpWorkerAsyncResult>();
        object queueLock = new object();
        //Stack<SmtpWorker> smtpWorkerPool = new Stack<SmtpWorker>();
        SmtpWorker smtpWorker;

        public SmtpWorkerPool(int maxConnections)
        {
            this.maxConnections = maxConnections;
        }

        public IAsyncResult BeginSend(MailMessage message, AsyncCallback callback, object state)
        {
            var asyncResult = new SmtpWorkerAsyncResult(message, callback, state);
            lock (queueLock)
            {
                messageQueue.Enqueue(asyncResult);
                EnsureWorker();
            }
            return asyncResult;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EndSend(IAsyncResult asyncResult)
        {
            var smtpResult = asyncResult as SmtpWorkerAsyncResult;
            if (smtpResult == null)
            {
                throw new ArgumentException("IAsyncResult is of the wrong type.", "asyncResult");
            }
            SmtpWorkerAsyncResult.End(smtpResult);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var worker = smtpWorker;
                if (worker != null)
                {
                    worker.Dispose();
                }
                while (messageQueue.Count > 0)
                {
                    var asyncResult = messageQueue.Dequeue();
                    asyncResult.Dispose();
                }
            }
        }

        private void EnsureWorker()
        {
            //if (smtpWorkerPool.Count == 0)
            //{
            //    var worker = new SmtpWorker(this);
            //}
            if (smtpWorker == null)
            {
                smtpWorker = new SmtpWorker(this);
                smtpWorker.Start();
            }
        }

        private class SmtpWorker : IDisposable
        {
            SmtpWorkerPool pool;
            SmtpClient SmtpClient { get; set; }
            Thread Thread { get; set; }

            public SmtpWorker(SmtpWorkerPool pool)
            {
                this.pool = pool;
                Thread = new Thread(ThreadStart);
                Thread.IsBackground = true;
                Thread.Name = "SmtpWorker";
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Start()
            {
                Thread.Start();
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (SmtpClient != null)
                    {
                        if (typeof(SmtpClient).GetInterface("IDisposable") != null)//In .NET 4, SmtpClient has IDisposable.
                        {
                            SmtpClient client = SmtpClient;
                            IDisposable disposable = client as IDisposable;
                            disposable.Dispose();
                        }
                    }
                }
            }

            private void ThreadStart()
            {
                SmtpClient = new SmtpClient();

                while (true)
                {
                    SmtpWorkerAsyncResult asyncResultToProcess = null;
                    lock (pool.queueLock)
                    {
                        if (pool.messageQueue.Count > 0)
                        {
                            asyncResultToProcess = pool.messageQueue.Dequeue();
                        }
                    }
                    if (asyncResultToProcess != null)
                    {
                        var message = asyncResultToProcess.Message;
                        Exception exception = null;
                        try
                        {
                            SmtpClient.Send(message);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                        asyncResultToProcess.Complete(exception, false);
                    }
                    Thread.Sleep(0);
                }

            }
        }
    }
}
