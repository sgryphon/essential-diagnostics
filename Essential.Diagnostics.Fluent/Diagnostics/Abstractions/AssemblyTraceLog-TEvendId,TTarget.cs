using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// A version of the generic TraceLog, which provides a fluent interface to a trace source,
    /// using the generic AssemblyTraceSource, which bases the trace source name on the assembly 
    /// name of the referenced target class.
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
    public class AssemblyTraceLog<TEventId, TTarget> : TraceLog<TEventId>
        where TEventId : struct, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the AssemblyTraceLog class. 
        /// </summary>
        public AssemblyTraceLog()
            : base(new AssemblyTraceSource<TTarget>())
        {
        }
    }
}
