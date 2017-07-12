[Home](../ReadMe.md) | [Docs](ReadMe.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Trace-Listeners.md) | [Fluent](Essential-Diagnostics-Fluent.md) | [Core](Essential-Diagnostics-Core.md)

# Trace Listeners

The following trace listeners are provided by the Essential.Diagnostics extensions.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |
| [BufferedEmailTraceListener](reference/BufferedEmailTraceListener.md) | Writes trace events to an Email message sent at the end of the host process. |
| [ColoredConsoleTraceListener](reference/ColoredConsoleTraceListener.md) | Writes formatted trace events to the console in color based on the type. |
| [EmailTraceListener](reference/EmailTraceListener.md) | Writes trace events to Email messages sent asynchronously. |
| [InMemoryTraceListener](reference/InMemoryTraceListener.md) | Writes traces to an in-memory array. |
| [RollingFileTraceListener](reference/RollingFileTraceListener.md) | Trace listener that writes formatted messages to a text file, rolling to a new file based on a filename template (usually including the date). |
| [RollingXmlTraceListener](reference/RollingXmlTraceListener.md) | Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date). |
| [SeqTraceListener](reference/SeqTraceListener.md) | Writes trace information to a [Seq](https://getseq.net/) logging server. |
| [SqlDatabaseTraceListener](reference/SqlDatabaseTraceListener.md) | Writes trace information to a SQL database. |
