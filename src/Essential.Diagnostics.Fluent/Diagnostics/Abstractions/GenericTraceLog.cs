using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Essential.Diagnostics.Abstractions
{
    /// <summary>
    /// A concrete sample version of the generic TraceLog, specifically using the enum GenericEventId.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class can be used directly by clients as an alternative fluent interface to a
    /// TraceSource, however it is better if applications develop their own event ID enum and use that.
    /// </para>
    /// <para>
    /// It is also just as easy to directly use <code>new TraceLog&lt;GenericEventId&gt;("name")</code>,  
    /// so this class is really just provided as an example.
    /// </para>
    /// <para>
    /// To automatically generate a name based on the assembly of the target class being traced,
    /// also consider <code>new AssemblyTraceLog&lt;MyEventId, MyClass&gt;()</code>
    /// </para>
    /// </remarks>
    public class GenericTraceLog : TraceLog<GenericEventId>
    {
        /// <summary>
        /// Initializes a new instance of the GenericTraceLog class, using the specified name for the underlying trace source. 
        /// </summary>
        /// <param name="name">Name of the underlying trace source</param>
        public GenericTraceLog(string name)
            : base(name)
        {
        }
    }
}
