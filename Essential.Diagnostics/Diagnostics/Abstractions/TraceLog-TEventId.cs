using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// Defines a generic class to log trace messages, following the popular fluent pattern 
    /// of having a method per level, and based on a strongly typed event IDs enum.
    /// </summary>
    /// <remarks>
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
    public class TraceLog<TEventId> : ITraceLog<TEventId>
        where TEventId : struct, IConvertible
    {
        ITraceSource _traceSource;

        // Check on TEventId type constraints
        static TraceLog()
        {
            var eventIdType = typeof(TEventId);           
            if (!eventIdType.Equals(typeof(int)) && !eventIdType.IsSubclassOf(typeof(Enum)))
            {
                throw new ArgumentException(string.Format(Resource.TraceLog_InvalidTEventId, typeof(TEventId).Name));
            }
        }

        /// <summary>
        /// Initializes a new instance of the TraceLog class, using the specified name for the underlying trace source. 
        /// </summary>
        /// <param name="name">Name of the underlying trace source</param>
        public TraceLog(string name)
            : this(new TraceSource(name))
        {
        }

        /// <summary>
        /// Initializes a new instance of the TraceLog class, using the specified trace source. 
        /// </summary>
        /// <param name="traceSource">TraceSource to write trace events to</param>
        public TraceLog(TraceSource traceSource)
            : this(new TraceSourceWrapper(traceSource))
        {
        }

        /// <summary>
        /// Initializes a new instance of the TraceLog class, using the specified trace source. 
        /// </summary>
        /// <param name="traceSource">ITraceSource to write trace events to</param>
        public TraceLog(ITraceSource traceSource)
        {
            _traceSource = traceSource;
        }

        /// <summary>
        /// Gets the underlying TraceSource, which can also be written to directly.
        /// </summary>
        public ITraceSource TraceSource
        {
            get { return _traceSource; }
        }

        // Critical

        /// <summary>
        /// Writes a Critical event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void Critical(TEventId id, string message)
        {
            _traceSource.TraceEvent(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), message);
        }

        /// <summary>
        /// Writes a Critical event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Critical(TEventId id, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), format, args);
        }

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        public void Critical(TEventId id, Exception ex)
        {
            TraceException(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), ex, null);
        }

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        public void Critical(TEventId id, Exception ex, string message)
        {
            TraceException(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), ex, message);
        }

        /// <summary>
        /// Writes an exception as a Critical event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Critical(TEventId id, Exception ex, string format, params object[] args)
        {
            TraceException(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), ex, format, args);
        }

        // Error

        /// <summary>
        /// Writes an Error event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void Error(TEventId id, string message)
        {
            _traceSource.TraceEvent(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), message);
        }

        /// <summary>
        /// Writes an Error event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Error(TEventId id, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), format, args);
        }

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        public void Error(TEventId id, Exception ex)
        {
            TraceException(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), ex, null);
        }

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        public void Error(TEventId id, Exception ex, string message)
        {
            TraceException(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), ex, message);
        }

        /// <summary>
        /// Writes an exception as an Error event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Error(TEventId id, Exception ex, string format, params object[] args)
        {
            TraceException(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), ex, format, args);
        }

        // Information

        /// <summary>
        /// Writes an Information event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void Information(TEventId id, string message)
        {
            _traceSource.TraceEvent(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), message);
        }

        /// <summary>
        /// Writes an Information event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Information(TEventId id, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), format, args);
        }

        // Verbose

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified message (with no event ID). 
        /// </summary>
        /// <param name="message">The trace message to write.</param>
        public void Verbose(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified format, and argument array (with no event ID). 
        /// </summary>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Verbose(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void Verbose(TEventId id, string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, id.ToInt32(CultureInfo.InvariantCulture), message);
        }

        /// <summary>
        /// Writes a Verbose event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Verbose(TEventId id, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, id.ToInt32(CultureInfo.InvariantCulture), format, args);
        }

        // Warning

        /// <summary>
        /// Writes a Warning event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public void Warning(TEventId id, string message) {
            _traceSource.TraceEvent(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), message);
        }

        /// <summary>
        /// Writes a Warning event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Warning(TEventId id, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), format, args);
        }

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        public void Warning(TEventId id, Exception ex)
        {
            TraceException(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), ex, null);
        }

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier, and message. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The trace message to write.</param>
        public void Warning(TEventId id, Exception ex, string message)
        {
            TraceException(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), ex, message);
        }

        /// <summary>
        /// Writes an exception as a Warning event to the underlying trace source using the specified event identifier, format, and argument array. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void Warning(TEventId id, Exception ex, string format, params object[] args)
        {
            TraceException(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), ex, format, args);
        }

        // Private

        private void TraceException(TraceEventType eventType, int id, Exception ex, string message)
        {
            var formatWithException = string.IsNullOrEmpty(message) 
                ? Resource.TraceLog_AppendedExceptionFormat
                : message.Replace("{", "{{").Replace("}", "}}") 
                    + Resource.TraceLog_ExceptionFormatSeparator 
                    + Resource.TraceLog_AppendedExceptionFormat;

            _traceSource.TraceEvent(eventType, id, formatWithException, ex);
        }

        private void TraceException(TraceEventType eventType, int id, Exception ex, string format, params object[] args)
        {
            var nextIndex = (args == null) ? 0 : args.Length;

            var argsWithException = new object[nextIndex + 1];
            Array.Copy(args, argsWithException, args.Length);
            argsWithException[nextIndex] = ex;

            var formatWithException = string.IsNullOrEmpty(format) 
                ? Resource.TraceLog_AppendedExceptionFormat.Replace("{0}", "{" + nextIndex.ToString() + "}") 
                : format
                    + Resource.TraceLog_ExceptionFormatSeparator
                    + Resource.TraceLog_AppendedExceptionFormat.Replace("{0}", "{" + nextIndex.ToString() + "}");

            _traceSource.TraceEvent(eventType, id, formatWithException, argsWithException);
        }

    }
}
