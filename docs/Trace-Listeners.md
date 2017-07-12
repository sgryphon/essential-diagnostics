[Home](../ReadMe.md) | [Docs](ReadMe.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Trace-Listeners.md) | [Fluent](Essential-Diagnostics-Fluent.md) | [Core](Essential-Diagnostics-Core.md)

# Trace Listeners

The following trace listeners are provided by the Essential.Diagnostics extensions.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |
| [BufferedEmailTraceListener](reference/BufferedEmailTraceListener.md)![EX](images/ex.png) | Writes trace events to an Email message sent at the end of the host process. |
| [ColoredConsoleTraceListener](reference/ColoredConsoleTraceListener.md)![EX](images/ex.png) | Writes formatted trace events to the console in color based on the type. |
| [EmailTraceListener](reference/EmailTraceListener.md)![EX](images/ex.png) | Writes trace events to Email messages sent asynchronously. |
| [InMemoryTraceListener](reference/InMemoryTraceListener.md)![EX](images/ex.png) | Writes traces to an in-memory array. |
| [RollingFileTraceListener](reference/RollingFileTraceListener.md)![EX](images/ex.png) | Trace listener that writes formatted messages to a text file, rolling to a new file based on a filename template (usually including the date). |
| [RollingXmlTraceListener](reference/RollingXmlTraceListener.md)![EX](images/ex.png) | Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date). |
| [SeqTraceListener](reference/SeqTraceListener.md)![EX](images/ex.png) | Writes trace information to a [Seq](https://getseq.net/) logging server. |
| [SqlDatabaseTraceListener](reference/SqlDatabaseTraceListener.md)![EX](images/ex.png) | Writes trace information to a SQL database. |
