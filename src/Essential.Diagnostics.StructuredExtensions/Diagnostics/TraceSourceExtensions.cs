using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Extension methods to TraceSource to facilitate structured tracing.
    /// </summary>
    public static class TraceSourceExtensions
    {
        /// <summary>
        /// Writes structured data to the trace listeners using the specified event type, event identifier, and structured data properties.
        /// </summary>
        /// <param name="traceSource">TraceSource to write structured data to.</param>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace.</param>
        public static void TraceStructuredData(this TraceSource traceSource, TraceEventType eventType, int id, IDictionary<string, object> properties)
        {
            TraceStructuredData(traceSource, eventType, id, properties, null, null, null);
        }

        /// <summary>
        /// Writes structured data to the trace listeners using the specified event type, event identifier, structured data properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="traceSource">TraceSource to write structured data to.</param>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public static void TraceStructuredData(this TraceSource traceSource, TraceEventType eventType, int id, IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(traceSource, eventType, id, properties, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes structured data to the trace listeners using the specified event type, event identifier, message template, and template values.
        /// </summary>
        /// <param name="traceSource">TraceSource to write structured data to.</param>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public static void TraceStructuredData(this TraceSource traceSource, TraceEventType eventType, int id, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(traceSource, eventType, id, null, null, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes structured data to the trace listeners using the specified event type, event identifier, exception, message template, and template values.
        /// </summary>
        /// <param name="traceSource">TraceSource to write structured data to.</param>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public static void TraceStructuredData(this TraceSource traceSource, TraceEventType eventType, int id, Exception exception, string messageTemplate, params object[] templateValues)
        {
            TraceStructuredData(traceSource, eventType, id, null, exception, messageTemplate, templateValues);
        }

        /// <summary>
        /// Writes structured data to the trace listeners using the specified event type, event identifier, structured data properties, exception, message template, and (optional) override template values.
        /// </summary>
        /// <param name="traceSource">TraceSource to write structured data to.</param>
        /// <param name="eventType">One of the enumeration values that specifies the event type of the trace data.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public static void TraceStructuredData(this TraceSource traceSource, TraceEventType eventType, int id, IDictionary<string, object> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            var structuredData = new StructuredData(properties, exception, messageTemplate, templateValues);
            traceSource.TraceData(eventType, id, structuredData);
        }
    }
}
