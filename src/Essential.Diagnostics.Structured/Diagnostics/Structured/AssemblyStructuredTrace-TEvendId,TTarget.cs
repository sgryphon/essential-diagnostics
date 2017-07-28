using System;
using System.Diagnostics;

namespace Essential.Diagnostics.Structured
{
    /// <summary>
    /// A version of the generic StructuredTrace, which provides a fluent interface to a trace source,
    /// which bases the trace source name on the assembly name of the referenced target class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is a simple combination of TraceLog and AssemblyTraceSource that allows client
    /// code to use <code>new AssemblyTraceLog&lt;MyEventId, MyClass&gt;()</code> instead of having
    /// to use <code>new TraceLog&lt;MyEventId&gt;(new AssemblyTraceSource&lt;MyClass&gt;())</code>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEventId">Strongly typed enum of event IDs</typeparam>
    /// <typeparam name="TTarget">Target type, whose assembly name is used as the the trace source name.</typeparam>
    public class AssemblyStructuredTrace<TEventId, TTarget> : StructuredTrace<TEventId>, IStructuredTrace<TEventId, TTarget>
        where TEventId : struct, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the AssemblyStructuredTrace class. 
        /// </summary>
        public AssemblyStructuredTrace()
            : base(new TraceSource(typeof(TTarget).Assembly.GetName().Name))
        {
        }
    }
}
