using System;

namespace Essential.Diagnostics.Structured
{
    /// <summary>
    /// Defines an alternate fluent interface to trace structured data, following the pattern
    /// of having a method per level, but reinforcing the concept of event IDs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using IStructuredTrace, and the derived classes, instead of directly using System.Diagnostics.TraceSource, 
    /// may provider a more readable interface for logging trace messages, whilst supporting strongly typed
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
    /// A GenericEventId example enum is provided (along with some generic classes that implement it), however 
    /// an application should generally utilise their own custom event IDs.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEventId">Strongly typed enum of event IDs</typeparam>
    /// <typeparam name="TTarget">Target type to base the trace source on.</typeparam>
    public interface IStructuredTrace<TEventId, TTarget> : IStructuredTrace<TEventId>
        where TEventId : struct, IConvertible
    {
    }
}
