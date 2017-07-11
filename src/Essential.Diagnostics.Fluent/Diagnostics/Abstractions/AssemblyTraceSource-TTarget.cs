using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

// TODO: Should probably really be in Essential.Diagnotics namespace, but that would be a breaking change
namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// Enable applications to trace the execution of code and associate trace 
    /// messages with a source named after the assembly the generic type
    /// is from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a way to automatically configure trace sources 
    /// based on the target class (specifically naming the source after the 
    /// assembly the class is from),which works well with dependency 
    /// injection frameworks.
    /// </para>
    /// <para>
    /// If using a dependency injection framework, by simply declaring
    /// a dependency of type ITraceSource`T and registering AssemblyTraceSource`T
    /// with the dependency injection container, classes will automatically
    /// get an ITraceSource based on their assembly name.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTarget">Target type, whose assembly name is used as the the trace source name.</typeparam>
    public class AssemblyTraceSource<TTarget> : ITraceSource<TTarget>
    {
        TraceSource traceSource;

        /// <summary>
        /// Initializes a new instance of the AssemblyTraceSource class. 
        /// </summary>
        public AssemblyTraceSource() : this(SourceLevels.Off)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyTraceSource class, using the specified default source level at which tracing is to occur. 
        /// </summary>
        /// <param name="defaultLevel"></param>
        public AssemblyTraceSource(SourceLevels defaultLevel)
        {
            var name = typeof(TTarget).Assembly.GetName().Name;
            traceSource = new TraceSource(name, defaultLevel);
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
