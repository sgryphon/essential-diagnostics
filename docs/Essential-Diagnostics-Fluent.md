[[Home](../ReadMe.md) | [Docs](ReadMe.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Trace-Listeners.md) | [Fluent](Essential-Diagnostics-Fluent.md) | [Core](Essential-Diagnostics-Core.md)

# Essential.Diagnostics.Fluent package

Contains the scope utility classes, abstractions, and templated classes for easy use with dependency injection. This package makes using System.Diagnotics trace sources easier, and can be used either separately (with system trace listeners), or in conjunction with the extended trace listeners above.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |
| [ActivityScope](reference/ActivityScope.md) | Sets the correlation ActivityId for the life of the scope object, performs a transfer, and logs activity messages. |
| [LogicalOperationScope](reference/LogicalOperationScope.md) | Sets the correlation LogicalOperation stack for the life of the scope object. |

### Essential.Diagnostics.Abstractions namespace

| Class | Description |
| ----- | ----------- |
| AssemblyTraceLog<TEventId, TTarget> | Implementation of the fluent log interface that is bound to a specific EventId type and with a source named after the target class assembly. |
| [AssemblyTraceSource<TTarget>](reference/AssemblyTraceSource_T.md) | Enable applications to trace the execution of code and associate trace messages with a source named after the assembly the generic type is from. |
| GenericEventId | General event IDs. |
| GenericTraceLog | Implementation of TraceLog<TEventId> bound to GenericEventId. |
| ITraceLog<TEventId> | Fluent log interface, with strongly typed event IDs. |
| [ITraceSource](reference/ITraceSource.md) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with their source.  |
| [ITraceSource<TTarget>](reference/ITraceSource_T.md) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with a source related to a specific class. |
| TraceLog<TEventId> | Generic implementation of the fluent log interface. |
| [TraceSourceWrapper](reference/TraceSourceWrapper.md) | Provides a wrapper around TraceSource that implements the ITraceSource interface, enable applications to trace the execution of code and associate trace messages with their source in a decoupled manner. |
