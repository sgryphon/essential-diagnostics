using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Essential.Diagnostics.Structured
{
    /// <summary>
    /// Defines a generic class to write structured trace messages, following the pattern 
    /// of having a method per level, and based on a strongly typed event IDs enum.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TEventId type is intended to be an enum, containing assigned event ID numbers, although constraints
    /// can only limit it to be a struct and to support conversion (to convert to System.Int32 for writing to
    /// the trace source). 
    /// </para>
    /// <para>
    /// A StandardEventId enum is provided, however an application should generally 
    /// utilise their own custom event IDs.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEventId">Strongly typed enum of event IDs</typeparam>
    public class StructuredTrace<TEventId> : IStructuredTrace<TEventId>
        where TEventId : struct, IConvertible
    {
        TraceSource _traceSource;

        // Check on TEventId type constraints
        static StructuredTrace()
        {
            var eventIdType = typeof(TEventId);           
            if (!eventIdType.Equals(typeof(int)) && !eventIdType.IsSubclassOf(typeof(Enum)))
            {
                throw new ArgumentException(string.Format(Resource_Structured.StructuredTrace_InvalidTEventId, typeof(TEventId).Name));
            }
        }

        /// <summary>
        /// Initializes a new instance of the StructuredTrace class, using the specified name for the underlying trace source. 
        /// </summary>
        /// <param name="name">Name of the underlying trace source</param>
        public StructuredTrace(string name)
            : this(new TraceSource(name))
        {
        }

        /// <summary>
        /// Initializes a new instance of the StructuredTrace class, using the specified trace source. 
        /// </summary>
        /// <param name="traceSource">TraceSource to write trace events to</param>
        public StructuredTrace(TraceSource traceSource)
        {
            _traceSource = traceSource;
        }

        /// <summary>
        /// Gets the underlying TraceSource, which can also be written to directly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value can be overriden or set by derived classes.
        /// </para>
        /// </remarks>
        public virtual TraceSource TraceSource
        {
            get { return _traceSource; }
            protected set { _traceSource = value; }
        }

        // Critical

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        public void Critical(TEventId id, IDictionary<string, object> properties)
        {
            TraceStructuredData(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), properties, null, null);
        }

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public void Critical(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Critical(TEventId id, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Critical(TEventId id, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), null, exception, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Critical(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Critical, id.ToInt32(CultureInfo.InvariantCulture), properties, exception, messageTemplate, templateValues);
        }

        // Error

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        public void Error(TEventId id, IDictionary<string, object> properties)
        {
            TraceStructuredData(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), properties, null, null);
        }

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public void Error(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Error(TEventId id, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Error(TEventId id, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), null, exception, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Error(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Error, id.ToInt32(CultureInfo.InvariantCulture), properties, exception, messageTemplate, templateValues);
        }

        // Information

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        public void Information(TEventId id, IDictionary<string, object> properties)
        {
            TraceStructuredData(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), properties, null, null);
        }

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public void Information(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Information(TEventId id, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Information(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Information, id.ToInt32(CultureInfo.InvariantCulture), properties, exception, messageTemplate, templateValues);
        }

        // Logical Operation Scope

        /// <summary>
        /// Starts a new structured data logical operation scope, typically output as additional structured properties.
        /// </summary>
        /// <param name="key">The additional structured property to include</param>
        /// <param name="value">The additional structured property value</param>
        /// <returns>A scope object that ends the scope whem disposed.</returns>
        public IDisposable BeginScope(string key, object value)
        {
            var structuredData = new StructuredData(new Dictionary<string, object>() { { key, value } });
            return BeginScope(structuredData);
        }

        /// <summary>
        /// Starts a new structured data logical operation scope, typically output as additional structured properties.
        /// </summary>
        /// <param name="structuredData">The additional structured data to include</param>
        /// <returns>A scope object that ends the scope whem disposed.</returns>
        public IDisposable BeginScope(IStructuredData structuredData)
        {
            return new StructuredDataScope(structuredData);
        }

        // Verbose

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified structured data properties. 
        /// </summary>
        /// <param name="properties">The key-value properties to trace.</param>
        public void Verbose(IDictionary<string, object> properties)
        {
            TraceStructuredData(TraceEventType.Verbose, 0, properties, null, null);
        }

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public void Verbose(IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Verbose, 0, properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified message. 
        /// </summary>
        /// <param name="message">Message template to insert properties into, containing keys</param>
        public void Verbose(string message)
        {
            TraceStructuredData(TraceEventType.Verbose, 0, null, null, message);
        }

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified message template, and template values. 
        /// </summary>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Verbose(string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Verbose, 0, null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Verbose(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Verbose, id.ToInt32(CultureInfo.InvariantCulture), properties, exception, messageTemplate, templateValues);
        }

        // Warning

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        public void Warning(TEventId id, IDictionary<string, object> properties)
        {
            TraceStructuredData(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), properties, null, null);
        }

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public void Warning(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Warning(TEventId id, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Warning(TEventId id, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), null, exception, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public void Warning(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(TraceEventType.Warning, id.ToInt32(CultureInfo.InvariantCulture), properties, exception, messageTemplate, templateValues);
        }

        // Protected

        /// <summary>
        /// Writes structured data to the underlying trace source using the specified event type, event identifier, structured data properties, exception, message template, and (optional) override template values.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method to alter behaviour.
        /// </para>
        /// </remarks>
        protected virtual void TraceStructuredData(TraceEventType eventType, int id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            var structuredData = new StructuredData(properties, exception, messageTemplate, templateValues);
            _traceSource.TraceData(eventType, id, structuredData);
        }
    }
}
