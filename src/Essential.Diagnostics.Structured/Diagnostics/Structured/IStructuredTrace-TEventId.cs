using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Essential.Diagnostics.Structured
{
    /// <summary>
    /// Defines an convenient interface for writing structured trace messages, following the pattern
    /// of having a method per level, but reinforcing the concept of event IDs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using IStructuredTrace, and the derived classes, instead of directly using System.Diagnostics.TraceSource, 
    /// may provider a more readable interface for logging structured trace messages, whilst supporting strongly typed
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
    /// A StandardEventId enum is provided, however an application should generally 
    /// utilise their own custom event IDs.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEventId">Strongly typed enum of event IDs</typeparam>
    public interface IStructuredTrace<TEventId>
        where TEventId : struct, IConvertible
    {
        /// <summary>
        /// Gets the underlying TraceSource, which can also be written to directly.
        /// </summary>
        TraceSource TraceSource { get; }

        // Critical

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        void Critical(TEventId id, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        void Critical(TEventId id, IDictionary<string, object> properties);

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Critical(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Critical(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Critical event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Critical(TEventId id, string messageTemplate, params object[] templateValues);

        // Error

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        void Error(TEventId id, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        void Error(TEventId id, IDictionary<string, object> properties);

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Error(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Error(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Error event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Error(TEventId id, string messageTemplate, params object[] templateValues);

        // Information

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        void Information(TEventId id, IDictionary<string, object> properties);

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        void Information(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Information(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Information event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Information(TEventId id, string messageTemplate, params object[] templateValues);

        // Logical Operation Scope

        /// <summary>
        /// Starts a new structured data logical operation scope, typically output as additional structured properties.
        /// </summary>
        /// <param name="key">The additional structured property to include</param>
        /// <param name="value">The additional structured property value</param>
        /// <returns>A scope object that ends the scope whem disposed.</returns>
        IDisposable BeginScope(string key, object value);

        /// <summary>
        /// Starts a new structured data logical operation scope, typically output as additional structured properties.
        /// </summary>
        /// <param name="structuredData">The additional structured data to include</param>
        /// <returns>A scope object that ends the scope whem disposed.</returns>
        IDisposable BeginScope(IStructuredData structuredData);

        // Verbose

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified structured data properties. 
        /// </summary>
        /// <param name="properties">The key-value properties to trace.</param>
        void Verbose(IDictionary<string, object> properties);

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        void Verbose(IDictionary<string, object> properties, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified message. 
        /// </summary>
        /// <param name="message">Message template to insert properties into, containing keys</param>
        void Verbose(string message);

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified message template, and template values. 
        /// </summary>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Verbose(string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Verbose event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Verbose(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues);

        // Warning

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, and structured data properties. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        void Warning(TEventId id, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        void Warning(TEventId id, IDictionary<string, object> properties);

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Warning(TEventId id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, exception, message template, and template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Warning(TEventId id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues);

        /// <summary>
        /// Writes a structured Warning event to the underlying trace source using the specified event identifier, structured data properties, exception, message template, and (optional) override template values. 
        /// </summary>
        /// <param name="id">A strongly typed identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The exception to trace.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        void Warning(TEventId id, string messageTemplate, params object[] templateValues);
    }
}
