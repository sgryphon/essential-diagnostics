using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// Defines a set of methods and properties that enable applications to trace the 
    /// execution of code and associate trace messages with their source. 
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
    public interface ITraceSource
    {
        /// <summary>
        /// Gets the custom switch attributes defined in the application configuration file. 
        /// </summary>
        StringDictionary Attributes { get; }

        /// <summary>
        /// Gets the collection of trace listeners for the trace source. 
        /// </summary>
        TraceListenerCollection Listeners { get; }

        /// <summary>
        /// Gets the name of the trace source. 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the source switch value. 
        /// </summary>
        SourceSwitch Switch { get; set; }

        /// <summary>
        /// Closes all the trace listeners in the trace listener collection. 
        /// </summary>
        void Close();

        /// <summary>
        /// Flushes all the trace listeners in the trace listener collection. 
        /// </summary>
        void Flush();

        //protected internal virtual string[] GetSupportedAttributes()

        /// <summary>
        /// Writes trace data to the trace listeners in the Listeners collection using the 
        /// specified event type, event identifier, and trace data. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type 
        /// of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data.</param>
        void TraceData(TraceEventType eventType, int id, object data);

        /// <summary>
        /// Writes trace data to the trace listeners in the Listeners collection using the specified event type, event identifier, and trace data array.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type 
        /// of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An object array containing the trace data.</param>
        void TraceData(TraceEventType eventType, int id, params object[] data);

        /// <summary>
        /// Writes a trace event message to the trace listeners in the Listeners collection using the specified event type and event identifier. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        void TraceEvent(TraceEventType eventType, int id);

        /// <summary>
        /// Writes a trace event message to the trace listeners in the Listeners collection using the specified event type, event identifier, and message. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void TraceEvent(TraceEventType eventType, int id, string message);

        /// <summary>
        /// Writes a trace event to the trace listeners in the Listeners collection using the specified event type, event identifier, and argument array and format. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void TraceEvent(TraceEventType eventType, int id, string format, params object[] args);

        /// <summary>
        /// Writes an informational message to the trace listeners in the Listeners collection using the specified message.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        void TraceInformation(string message);

        /// <summary>
        /// Writes an informational message to the trace listeners in the Listeners collection using the specified object array and formatting information. 
        /// </summary>
        /// <param name="format">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void TraceInformation(string format, params object[] args);

        /// <summary>
        /// Writes a trace transfer message to the trace listeners in the Listeners collection using the specified numeric identifier, message, and related activity identifier. 
        /// </summary>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        /// <param name="relatedActivityId">A structure that identifies the related activity.</param>
        void TraceTransfer(int id, string message, Guid relatedActivityId);
    }
}
