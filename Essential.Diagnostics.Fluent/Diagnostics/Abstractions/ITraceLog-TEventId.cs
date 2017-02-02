using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// Defines an alternate fluent interface to log trace messages, following the popular pattern
    /// of having a method per level, but reinforcing the concept of event IDs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using ITraceLog, and the derived classes, instead of directly using System.Diagnostics.TraceSource, 
    /// may provider a more readable interface for logging trace messages, whilst supporting strongly typed
    /// event IDs.
    /// </para>
    /// <para>
    /// The class also includes support for explicit logging of exceptions at warning or higher level.
    /// </para>
    /// <para>
    /// The TEventId type is intended to be an enum, containing assigned event ID numbers, although constraints
    /// can only limit it to be a struct and to support conversion (to convert to System.Int32 for writing to
    /// the trace source). 
    /// </para>
    /// <para>
    /// A GenericEventId example enum is provided (along with some generic classes that implement it), however 
    /// an application should generally utilise their own custom event IDs.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEventId">Strongly typed enum of event IDs</typeparam>
    public interface ITraceLog<TEventId>
        where TEventId : struct, IConvertible
    {
        /// <summary>
        /// Gets the underlying TraceSource, which can also be written to directly.
        /// </summary>
        ITraceSource TraceSource { get; }

        /// <summary>
        /// Writes a Critical event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void Critical(TEventId id, string message);

        /// <summary>
        /// Writes a Critical event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Critical(TEventId id, string format, params object[] args);

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        void Critical(TEventId id, Exception ex);

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        void Critical(TEventId id, Exception ex, string message);

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Critical(TEventId id, Exception ex, string format, params object[] args);

        // Error

        /// <summary>
        /// Writes an Error event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void Error(TEventId id, string message);

        /// <summary>
        /// Writes an Error event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Error(TEventId id, string format, params object[] args);

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        void Error(TEventId id, Exception ex);

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        void Error(TEventId id, Exception ex, string message);

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Error(TEventId id, Exception ex, string format, params object[] args);

        // Information

        /// <summary>
        /// Writes an Information event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void Information(TEventId id, string message);

        /// <summary>
        /// Writes an Information event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Information(TEventId id, string format, params object[] args);

        // Verbose

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified message (with no event ID). 
        /// </summary>
        /// <param name="message">The trace message to write.</param>
        void Verbose(string message);

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified format, and argument array (with no event ID). 
        /// </summary>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Verbose(string format, params object[] args);

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void Verbose(TEventId id, string message);

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Verbose(TEventId id, string format, params object[] args);

        // Warning

        /// <summary>
        /// Writes a Warning event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        void Warning(TEventId id, string message);

        /// <summary>
        /// Writes a Warning event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Warning(TEventId id, string format, params object[] args);

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        void Warning(TEventId id, Exception ex);

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        void Warning(TEventId id, Exception ex, string message);

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        void Warning(TEventId id, Exception ex, string format, params object[] args);
    }
}
