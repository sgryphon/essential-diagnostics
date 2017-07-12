[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Listeners

The following trace listeners are provided by the .NET Framework and the Essential.Diagnostics extensions.

| Class | Description |
| ----- | ----------- |
| [BufferedEmailTraceListener](reference/BufferedEmailTraceListener.md)![EX](images/ex.png) | Writes trace events to an Email message sent at the end of the host process. |
| [ColoredConsoleTraceListener](reference/ColoredConsoleTraceListener.md)![EX](images/ex.png) | Writes formatted trace events to the console in color based on the type. |
| [ConsoleTraceListener](reference/ConsoleTraceListener.md) | Writes trace events to the console |
| DefaultTraceListener |  |
| DelimitedListTraceListener | Writes trace events to a file as a delimited list. |
| DiagnosticMonitorTraceListener | Part of Microsoft.WindowsAzure.Diagnostics; writes traces to Azure logs |
| [EmailTraceListener](reference/EmailTraceListener.md)![EX](images/ex.png) | Writes trace events to Email messages sent asynchronously. |
| [EventLogTraceListener](reference/EventLogTraceListener.md) | Writes trace events to the Windows Event Log |
| EventProviderTraceListener |  |
| EventSchemaTraceListener |  |
| [FileLogTraceListener](reference/FileLogTraceListener.md) | Writes trace events to a file with advanced options for file rotation and output format. |
| FlatFileTraceListener^^1^^ | |
| FormattedEventLogTraceListener^^1^^ | |
| [InMemoryTraceListener](reference/InMemoryTraceListener.md)![EX](images/ex.png) | Writes traces to an in-memory array. |
| [RollingFileTraceListener](reference/RollingFileTraceListener.md)![EX](images/ex.png4) | Trace listener that writes formatted messages to a text file, rolling to a new file based on a filename template (usually including the date). |
| [RollingXmlTraceListener](reference/RollingXmlTraceListener.md)![EX](images/ex.png) | Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date). |
| [SeqTraceListener](reference/SeqTraceListener.md)![EX](images/ex.png) | Writes trace information to a [Seq](https://getseq.net/) logging server. |
| [SqlDatabaseTraceListener](reference/SqlDatabaseTraceListener.md)![EX](images/ex.png) | Writes trace information to a SQL database. |
| [TextWriterTraceListener](reference/TextWriterTraceListener.md) | Writes trace events to a simple file. Recommended you at least use [FileLogTraceListener](reference/FileLogTraceListener.md) instead. |
| WebPageTraceListener | Forwards trace events to the ASP.NET trace output. |
| WMITraceListener^^1^^ | |
| [XmlWriterTraceListener](reference/XmlWriterTraceListener.md) | Writes events in XML format, suitable for import into the Service Trace Viewer utility. |

Note: ^^1^^ These trace listeners from the Enterprise Library Logging Application Block can also be used directly with System.Diagnostics (for details see [http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx](http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx)).
