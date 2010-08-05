using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics
{
    public class ExpressionFilter : TraceFilter
    {
        Func<string, TraceEventType, int, string, bool> compiledExpression;

        public ExpressionFilter(string expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            // , "LogicalOperationStack", "DateTime", "ThreadName", "ProcessName", "MachineName"
            // Note: Func<> has max 4 parameters; need one to be "Event", e.g. "Event.Id", etc??
            // Note: "Format" may be the entire message
            var parser = new ExpressionParser<Func<string, TraceEventType, int, string, bool>>("Source", "EventType", "Id", "Format");

            compiledExpression = parser.Parse(expression).Compile();
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return compiledExpression(source, eventType, id, formatOrMessage);
        }
    }
}
