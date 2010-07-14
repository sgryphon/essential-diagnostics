using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Trace listener that forwards all calls to a single template method,
    /// allowing easy implementation of custom trace listeners.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This trace listener is designed to allow easy creation of custom
    /// trace listeners by inheriting from this class and implementing
    /// a single template method, WriteTrace.
    /// </para>
    /// <para>
    /// The WriteTrace method combines all the various combinations
    /// of trace methods from the base TraceListener class, passing
    /// the full details to the template method allowing structured
    /// logging.
    /// </para>
    /// <para>
    /// By default the write methods are also converted to Verbose trace
    /// events, allowing them to also be logged in a structured manner.
    /// </para>
    /// <para>
    /// If implementing a stream-based listener, then the default
    /// template methods for Write and WriteLine can also be overridden
    /// to provide behaviour similar to stream-based listeners in
    /// the .NET Framework.
    /// </para>
    /// </remarks>
    public abstract class TraceListenerBase : TraceListener
    {
        // //////////////////////////////////////////////////////////
        // Constructors

        /// <summary>
        /// Constructor used when creating from config file. 
        /// (The Name property is set after the TraceListener is created.)
        /// </summary>
        protected TraceListenerBase()
            : base()
        {
        }

        /// <summary>
        /// Constructor used when creating dynamically in code. The name should be set in the constructor.
        /// </summary>
        /// <param name="name">Name of the trace listener.</param>
        protected TraceListenerBase(string name)
            : base(name)
        {
        }


        // //////////////////////////////////////////////////////////
        // Public Methods

        public override sealed void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                WriteTrace(eventCache, source, eventType, id, null, null, new object[] { data });
            }
        }

        public override sealed void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                WriteTrace(eventCache, source, eventType, id, null, null, data);
            }
        }

        public override sealed void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                WriteTrace(eventCache, source, eventType, id, message, null, null);
            }
        }

        public override sealed void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                WriteTrace(eventCache, source, eventType, id, message, null, null);
            }
        }

        public override sealed void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, TraceEventType.Transfer, id, message, null, null, null))
            {
                WriteTrace(eventCache, source, TraceEventType.Transfer, id, message, relatedActivityId, null);
            }
        }

        public override sealed void Write(object o)
        {
            Write(null, null, o);
        }

        public override sealed void Write(object o, string category)
        {
            Write(category, null, o);
        }

        public override sealed void Write(string message)
        {
            Write(null, message, null);
        }

        public override sealed void Write(string message, string category)
        {
            Write(category, message, null);
        }

        public override sealed void WriteLine(object o)
        {
            WriteLine(null, null, o);
        }

        public override sealed void WriteLine(object o, string category)
        {
            WriteLine(category, null, o);
        }

        public override sealed void WriteLine(string message)
        {
            WriteLine(null, message, null);
        }

        public override sealed void WriteLine(string message, string category)
        {
            WriteLine(category, message, null);
        }


        // //////////////////////////////////////////////////////////
        // Protected

        /// <summary>
        /// Write simple data to trace output. Default converts the data to a Verbose event.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected virtual void Write(string category, string message, object data)
        {
            TraceWriteAsEvent(category, message, data);
        }

        /// <summary>
        /// Write simple data to trace output on a new line (if output has lines). Default converts the data to a Verbose event.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected virtual void WriteLine(string category, string message, object data)
        {
            TraceWriteAsEvent(category, message, data);
        }

        /// <summary>
        /// Overriden by derived classes to write event message and data to trace output.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <param name="relatedActivityId"></param>
        /// <param name="data"></param>
        protected abstract void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data);


        // //////////////////////////////////////////////////////////
        // Private

        private void TraceWriteAsEvent(string category, string message, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(null, null, TraceEventType.Verbose, 0, message, new object[] { category }, data, null))
            {
                if (message != null)
                {
                    if (category != null)
                    {
                        WriteTrace(null, null, TraceEventType.Verbose, 0, category, null, new object[] { data });
                    }
                    else
                    {
                        WriteTrace(null, null, TraceEventType.Verbose, 0, null, null, new object[] { data });
                    }
                }
                else
                {
                    if (category != null)
                    {
                        WriteTrace(null, null, TraceEventType.Verbose, 0, category + ": " + message, null, null);
                    }
                    else
                    {
                        WriteTrace(null, null, TraceEventType.Verbose, 0, message, null, null);
                    }
                }
            }
        }

    }
}
