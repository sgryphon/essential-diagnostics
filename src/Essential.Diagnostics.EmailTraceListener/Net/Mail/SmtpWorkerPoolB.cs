using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

    // Also note that in both cases you might as well send the email message synchronously, as
    // you need to wait for it to complete anyway.
    
    // Another way is use the async thread that the SmtpClient calls back on to check the queue
    // and process the next message if necessary, however while SendAsync() sends the message
    // asynchronously, the initial connection is done synchronously (and may throw an
    // exception). If we don't even want to block on the initial connection, then we need to use
    // a separate worker thread.



    // TODO: Do we really want this public?? -- it is used for testing.

    class SmtpWorkerPoolB : IDisposable
    {
        const int maxExitWaitMilliseconds = 2000;
        const int exitCheckIntervalMilliseconds = 100;
        const int idleTimeoutMilliseconds = 500;

        int maxConnections;
        Queue<SmtpWorkerAsyncResult> messageQueue = new Queue<SmtpWorkerAsyncResult>();
        object queueLock = new object();
        List<SmtpWorker> smtpWorkerPool = new List<SmtpWorker>();

        public SmtpWorkerPoolB(int maxConnections)
        {
            this.maxConnections = maxConnections;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        public IAsyncResult BeginSend(MailMessage message, AsyncCallback callback, object state)
        {
            var asyncResult = new SmtpWorkerAsyncResult(message, callback, state);
            lock (queueLock)
            {
                // lock keeps queue count and active client count in sync
                messageQueue.Enqueue(asyncResult);
                EnsureWorker();
            }
            return asyncResult;
        }

        public IAsyncResult BeginSend(string from, string recipients, string subject, string body, AsyncCallback callback, object state)
        {
            var mailMessage = new MailMessage(from, recipients, subject, body);
            var asyncResult = BeginSend(mailMessage,
                    (ar) =>
                    {
                        mailMessage.Dispose();
                        if (callback != null)
                        {
                            callback(ar);
                        }
                    },
                    state);
            return asyncResult;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Want API to be instance method.")]
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
                while (messageQueue.Count > 0)
                {
                    var asyncResult = messageQueue.Dequeue();
                    asyncResult.Dispose();
                }
            }
        }        

        private static void TryDisposeSmtpClient(SmtpClient smtpClient)
        {
            if (typeof(SmtpClient).GetInterface("IDisposable") != null)//In .NET 4, SmtpClient has IDisposable.
            {
                IDisposable disposable = smtpClient as IDisposable;
                disposable.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification = "Deliberate dependency, .NET 2.0 SP1 required.")]
        private void EnsureWorker()
        {
            if (smtpWorkerPool.Count < maxConnections)
            {
                bool anyIdle = false;
                foreach (var worker in smtpWorkerPool)
                {
                    if (worker.IsIdle) 
                    {
                        anyIdle = true;
                        break;
                    }
                }
                if (!anyIdle)
                {
                    var newWorker = new SmtpWorker(this);
                    smtpWorkerPool.Add(newWorker);
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: EnsureWorker, pool count = {1}, queue length = {2}", DateTimeOffset.Now, smtpWorkerPool.Count, messageQueue.Count));
                    newWorker.Start();
                }
            }
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SendAllBeforeExit();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification = "Deliberate dependency, .NET 2.0 SP1 required.")]
        void SendAllBeforeExit()
        {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: SendAllBeforeExit, pool count = {1}, queue length = {2}", DateTimeOffset.Now, this.smtpWorkerPool.Count, this.messageQueue.Count));
            int totalWaitTime = 0;
            while (smtpWorkerPool.Count > 0 && totalWaitTime < maxExitWaitMilliseconds)
            {
                Thread.Sleep(exitCheckIntervalMilliseconds);
                totalWaitTime += exitCheckIntervalMilliseconds;
            }
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: DONE SendAllBeforeExit, pool count = {1}, queue length = {2}", DateTimeOffset.Now, this.smtpWorkerPool.Count, this.messageQueue.Count));
        }

        private class SmtpWorker
        {
            SmtpWorkerPoolB pool;
            Thread thread;
            bool isIdle;

            public SmtpWorker(SmtpWorkerPoolB pool)
            {
                this.pool = pool;
                thread = new Thread(ThreadStart);
                thread.IsBackground = true;
                thread.Name = "SmtpWorker";
            }

            public void Start()
            {
                thread.Start();
            }

            public bool IsIdle
            {
                get { return isIdle; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Exception is reported in IAsyncResult for client to handle. Any thread exceptions are also caught and printed to debug, as this is the top of the stack.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.DateTimeOffset", Justification = "Deliberate dependency, .NET 2.0 SP1 required.")]
            private void ThreadStart()
            {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: ThreadStart, pool count = {1}, queue length = {2}", DateTimeOffset.Now, pool.smtpWorkerPool.Count, pool.messageQueue.Count));
                SmtpClient smtpClient = null;
                try
                {
                    smtpClient = new SmtpClient();

                    Stopwatch idleTimeout = new Stopwatch();
                    bool running = true;

                    while (running)
                    {
                        SmtpWorkerAsyncResult asyncResultToProcess = null;
                        lock (pool.queueLock)
                        {
                            // lock keeps queue count and active client count in sync
                            if (pool.messageQueue.Count > 0)
                            {
                                asyncResultToProcess = pool.messageQueue.Dequeue();
                            }
                            else
                            {
                                if (idleTimeout.IsRunning && idleTimeout.ElapsedMilliseconds > idleTimeoutMilliseconds)
                                {
                                    pool.smtpWorkerPool.Remove(this);
                                    running = false;
                                }
                            }
                        }
                        if (asyncResultToProcess != null)
                        {
                            if (idleTimeout.IsRunning)
                            {
                                isIdle = false;
                                idleTimeout.Reset();
                            }
                            var message = asyncResultToProcess.Message;
                            Exception exception = null;
                            try
                            {
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: Before Send, pool count = {1}, queue length = {2}", DateTimeOffset.Now, pool.smtpWorkerPool.Count, pool.messageQueue.Count));
                                smtpClient.Send(message);
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: After Send, pool count = {1}, queue length = {2}", DateTimeOffset.Now, pool.smtpWorkerPool.Count, pool.messageQueue.Count));
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error sending: {0}", exception));
                            }
                            asyncResultToProcess.Complete(exception, false);
                        }
                        else
                        {
                            if (!idleTimeout.IsRunning)
                            {
                                isIdle = true;
                                idleTimeout.Start();
                            }
                        }
                        Thread.Sleep(0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: Exception: {1}", DateTimeOffset.Now, ex));
                }
                finally
                {
                    if (smtpClient != null)
                    {
                        TryDisposeSmtpClient(smtpClient);
                    }
                }

                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:mm':'ss.ffffff}: ThreadEnding, pool count = {1}, queue length = {2}", DateTimeOffset.Now, pool.smtpWorkerPool.Count, pool.messageQueue.Count));
            }
        }
    }
}
