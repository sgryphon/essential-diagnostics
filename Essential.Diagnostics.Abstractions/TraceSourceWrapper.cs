using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// Provides a wrapper around TraceSource that implements the ITraceSource interface, 
    /// enable applications to trace the execution of code and associate trace messages 
    /// with their source in a decoupled manner. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using ITraceSource and TraceSourceWrapper, instead of directly using 
    /// System.Diagnostics.TraceSource, may make testing easier in some cases by
    /// allowing the entire tracing infrastructure to by bypassed by using
    /// a stub or mock implementation.
    /// </para>
    /// <para>
    /// Configuration using InMemoryTraceListener could achieve many of the
    /// same goals for testing, however using an interface may provide greater
    /// flexibility in some scenarios.
    /// </para>
    /// <para>
    /// Using the derived generic interface also provides a way to 
    /// automatically configure trace sources based on the target class
    /// (specifically naming the source after the assembly the class is from),
    /// which works well with dependency injection frameworks.
    /// </para>
    /// </remarks>
    public class TraceSourceWrapper : ITraceSource
    {
        TraceSource traceSource;

        /// <summary>
        /// Initializes a new instance of the TraceSourceWrapper class, using the specified name for the source. 
        /// </summary>
        /// <param name="name"></param>
        public TraceSourceWrapper(string name)
        {
            traceSource = new TraceSource(name);
        }

        /// <summary>
        /// Initializes a new instance of the TraceSource class, using the specified name for the source 
        /// and the default source level at which tracing is to occur. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultLevel"></param>
        public TraceSourceWrapper(string name, SourceLevels defaultLevel)
        {
            traceSource = new TraceSource(name, defaultLevel);
        }

        /// <summary>
        /// Initializes a new instance of the TraceSourceWrapper class, using the specified TraceSource for the source. 
        /// </summary>
        /// <param name="traceSource"></param>
        public TraceSourceWrapper(TraceSource traceSource)
        {
            this.traceSource = traceSource;
        }

        /// <summary>
        /// Gets the custom switch attributes defined in the application configuration file. 
        /// </summary>
        public StringDictionary Attributes
        {
            get { return traceSource.Attributes; }
        }

        /// <summary>
        /// Gets the collection of trace listeners for the trace source. 
        /// </summary>
        public TraceListenerCollection Listeners
        {
            get { return traceSource.Listeners; }
        }

        /// <summary>
        /// Gets the name of the trace source. 
        /// </summary>
        public string Name
        {
            get { return traceSource.Name; }
        }

        /// <summary>
        /// Gets or sets the source switch value. 
        /// </summary>
        public SourceSwitch Switch
        {
            get
            {
                return traceSource.Switch;
            }
            set
            {
                traceSource.Switch = value;
            }
        }

        /// <summary>
        /// Closes all the trace listeners in the trace listener collection. 
        /// </summary>
        public void Close()
        {
            traceSource.Close();
        }

        /// <summary>
        /// Flushes all the trace listeners in the trace listener collection. 
        /// </summary>
        public void Flush()
        {
            traceSource.Flush();
        }

        /// <summary>
        /// Writes trace data to the trace listeners in the Listeners collection using the 
        /// specified event type, event identifier, and trace data. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type 
        /// of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data.</param>
        public void TraceData(TraceEventType eventType, int id, object data)
        {
            traceSource.TraceData(eventType, id, data);
        }

        /// <summary>
        /// Writes trace data to the trace listeners in the Listeners collection using the specified event type, event identifier, and trace data array.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type 
        /// of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An object array containing the trace data.</param>
        public void TraceData(TraceEventType eventType, int id, params object[] data)
        {
            traceSource.TraceData(eventType, id, data);
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the Listeners collection using the specified event type and event identifier. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public void TraceEvent(TraceEventType eventType, int id)
        {
            traceSource.TraceEvent(eventType, id);
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the Listeners collection using the specified event type, event identifier, and message. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void TraceEvent(TraceEventType eventType, int id, string message)
        {
            traceSource.TraceEvent(eventType, id, message);
        }

        /// <summary>
        /// Writes a trace event to the trace listeners in the Listeners collection using the specified event type, event identifier, and argument array and format. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            traceSource.TraceEvent(eventType, id, format, args);
        }

        /// <summary>
        /// Writes an informational message to the trace listeners in the Listeners collection using the specified message.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        public void TraceInformation(string message)
        {
            traceSource.TraceInformation(message);
        }

        /// <summary>
        /// Writes an informational message to the trace listeners in the Listeners collection using the specified object array and formatting information. 
        /// </summary>
        /// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void TraceInformation(string format, params object[] args)
        {
            traceSource.TraceInformation(format, args);
        }

        /// <summary>
        /// Writes a trace transfer message to the trace listeners in the Listeners collection using the specified numeric identifier, message, and related activity identifier. 
        /// </summary>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        /// <param name="relatedActivityId">A structure that identifies the related activity.</param>
        public void TraceTransfer(int id, string message, Guid relatedActivityId)
        {
            traceSource.TraceTransfer(id, message, relatedActivityId);
        }
    }
}
