using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace Essential.Net.Mail
{
    // See
    // http://msdn.microsoft.com/en-us/magazine/cc163467.aspx
    // http://blogs.msdn.com/b/nikos/archive/2011/03/14/how-to-implement-iasyncresult-in-another-way.aspx

    internal class SmtpWorkerAsyncResult : IAsyncResult, IDisposable
    {
        private const int c_StatePending = 0;
        private const int c_StateCompletedSynchronously = 1;
        private const int c_StateCompletedAsynchronously = 2;

        private readonly AsyncCallback m_AsyncCallback; 
        private readonly Object m_AsyncState;
        private MailMessage message;

        // Fields set at construction which do change after 
        // operation completes 
        private int m_CompletedState = c_StatePending;

        // Field that may or may not get set depending on usage 
        private ManualResetEvent m_AsyncWaitHandle;

        // Fields set when operation completes 
        private Exception m_exception;

        internal SmtpWorkerAsyncResult(MailMessage message, AsyncCallback asyncCallback, object state)
        {
            this.message = message;
            this.m_AsyncCallback = asyncCallback; 
            this.m_AsyncState = state;
        }

        public MailMessage Message { get { return message; } }

        public Object AsyncState { 
            get { return m_AsyncState; } 
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (m_AsyncWaitHandle == null)
                {
                    Boolean done = IsCompleted; ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref m_AsyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose 
                        // the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            m_AsyncWaitHandle.Set();
                        }
                    }
                }
                return m_AsyncWaitHandle;
            }
        }

        public Boolean CompletedSynchronously
        {
            get
            {
                return Thread.VolatileRead(ref m_CompletedState) == c_StateCompletedSynchronously;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Boolean IsCompleted
        {
            get
            {
                return Thread.VolatileRead(ref m_CompletedState) != c_StatePending;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Although has message field, does not own it -- it was passed into constructor.
                // (i.e. not responsible for disposing of it, but do want to release reference).
                message = null;
                //if (message != null)
                //{
                //    message.Dispose();
                //}

                if (m_AsyncWaitHandle != null)
                {
                    ((IDisposable)m_AsyncWaitHandle).Dispose();
                }
            }
        }

        internal void Complete(Exception exception, bool completedSynchronously)
        {
            // The m_CompletedState field MUST be set prior calling the callback 
            int prevState = Interlocked.Exchange(ref m_CompletedState,
                completedSynchronously ? c_StateCompletedSynchronously : c_StateCompletedAsynchronously);

            if (prevState == c_StatePending)
            {
                // Passing null for exception means no error occurred. 
                // This is the common case 
                m_exception = exception;

                // If the event exists, set it 
                if (m_AsyncWaitHandle != null)
                {
                    m_AsyncWaitHandle.Set();
                }

                // If a callback method was set, call it 
                if (m_AsyncCallback != null)
                {
                    m_AsyncCallback(this);
                }
            }
        }

        internal static void End(SmtpWorkerAsyncResult asyncResult)
        {
            // This method assumes that only 1 thread calls EndInvoke 
            // for this object 
            if (!asyncResult.IsCompleted)
            {
                // If the operation isn't done, wait for it
                asyncResult.AsyncWaitHandle.WaitOne();
                asyncResult.AsyncWaitHandle.Close();
                // Allow early GC 
                asyncResult.m_AsyncWaitHandle = null;
            }

            // Operation is done: if an exception occured, throw it
            if (asyncResult.m_exception != null)
            {
                throw asyncResult.m_exception;
            }
        }


    }
}
