//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text;

//namespace Essential.Diagnostics
//{
//    /// <summary>
//    /// Trace filter that evaluates a user-supplied boolean expression.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// The initalizeData for the filter contains the C# expression that
//    /// is evaluated. The expression must return a boolean result and
//    /// can use any of the parameters: Source, EventType, Id, Format.
//    /// </para>
//    /// <para>
//    /// Note that Format is the format template or message (if there are no
//    /// format parameters).
//    /// </para>
//    /// </remarks>
//    public class ExpressionFilter : TraceFilter
//    {
//        //Func<string, TraceEventType, int, string, bool> compiledExpression;

//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        /// <param name="expression">A C# expression that is evaluated to determine if the event should be traced or not.</param>
//        public ExpressionFilter(string expression)
//        {
//            if (expression == null) throw new ArgumentNullException("expression");

//            // TODO: Other properties - "LogicalOperationStack", "DateTime", "ThreadName", "ProcessName", "MachineName"
//            // Note: Func<> has max 4 parameters; need one to be "Event", e.g. "Event.Id", etc??
//            // ... but not sure how the compiled code can get a reference to the "Event" object (class).
//            // May have to give up on the expression idea and build a parser that directly calls
//            // a function.

//            //var parser = new ExpressionParser<Func<string, TraceEventType, int, string, bool>>("Source", "EventType", "Id", "Format");
//            //compiledExpression = parser.Parse(expression).Compile();
//        }

//        /// <summary>
//        /// Determines whether the event should be traced by the listener or not.
//        /// </summary>
//        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
//        {
//            return true;
//            //return compiledExpression(source, eventType, id, formatOrMessage);
//        }
//    }
//}
