using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Trace filter that filters based on the value of a property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The initializeData for this filter contains a single property
    /// comparison, e.g. initializeData="Id == 1".
    /// </para>
    /// <para>
    /// Currently only equality is supported, using C# syntax (double
    /// equals signs). The property must come first and must be
    /// one of the values: 
    /// Data, Data0, EventType, Id, Message, ActivityId, RelatedActivityId, 
    /// Source, Callstack, DateTime, LogicalOperationStack, 
    /// ProcessId, ThreadId, Timestamp, MachineName, ProcessName, ThreadName.
    /// </para>
    /// </remarks>
    public class PropertyFilter : TraceFilter
    {
        private string propertyTemplate;
        private string comparisonValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropertyFilter(string propertyComparison)
        {
            if (propertyComparison == null) throw new ArgumentNullException("propertyComparison");

            var comparatorPosition = propertyComparison.IndexOf("==", StringComparison.Ordinal);
            if (comparatorPosition < 1 || comparatorPosition > propertyComparison.Length - 2)
            {
                throw new ArgumentException("Property comparison must contain a comparison ('==') with a property and value, e.g. initializeData=\"id == 1\".", "propertyComparison");
            }

            propertyTemplate = "{" + propertyComparison.Substring(0, comparatorPosition).Trim() + "}";
            comparisonValue = propertyComparison.Substring(comparatorPosition + 2).Trim();

            if (comparisonValue.Length >= 2)
            {
                if ((comparisonValue.StartsWith("\"", StringComparison.Ordinal) && comparisonValue.EndsWith("\"", StringComparison.Ordinal))
                    || (comparisonValue.StartsWith("'", StringComparison.Ordinal) && comparisonValue.EndsWith("'", StringComparison.Ordinal)))
                {
                    comparisonValue = comparisonValue.Substring(1, comparisonValue.Length - 2);
                }
            }
        }

        /// <summary>
        /// Determines whether the event should be traced by the listener or not.
        /// </summary>
        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            if (data == null)
            {
                data = new object[] { data1 };
            }
            string propertyValue = TraceFormatter.Format(propertyTemplate, cache, source, eventType, id, null,
                                                        null, data);
            return propertyValue.Equals(comparisonValue);
        }
    }
}
