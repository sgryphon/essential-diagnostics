[Home](../ReadMe.md) | [Docs](ReadMe.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Trace-Listeners.md) | [Fluent](Essential-Diagnostics-Fluent.md) | [Core](Essential-Diagnostics-Core.md)

# Essential.Diagnostics.Core package

Core library with base classes, referenced by other packages.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |                                                     s
| [ExpressionFilter](reference/ExpressionFilter.md)![EX](images/ex.png) | Filter events based on an expression. |
| [TraceConfigurationMonitor](reference/TraceConfigurationMonitor.md)![EX](images/ex.png) | Monitors the config file for changes are refreshes trace listeners when required. |
| [TraceFormatter](reference/TraceFormatter.md)![EX](images/ex.png) | Inserts trace information into a provided template string. Used to provide the advanced formatting for several listeners. |
| [TraceListenerBase](reference/TraceListenerBase.md)![EX](images/ex.png) | Extended trace listener designed to be subclassed with as little as a single template method override. |
