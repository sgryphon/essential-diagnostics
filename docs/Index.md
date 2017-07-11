[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Index

## [Examples](Examples.md)

* [Getting Started](Getting-Started.md)
* [Logging Primer](Logging-Primer.md)
  * [Hello Logging](Hello-Logging.md)
  * [Service Trace Viewer](Service-Trace-Viewer.md)
  * [Windows Event Log](Windows-Event-Log.md)
  * [Hello Color](Hello-Color.md)![EX](images/ex.png)

## [Listeners](Listeners.md)

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

## [Filters](Filters)

| Class | Description |
| ----- | ----------- |
| EventTypeFilter | Filters based on the level of the TraceEventType, e.g. Warning, Error, etc. |
| [ExpressionFilter](ExpressionFilter.md)![EX](images/ex.png) | Filter events based on an expression. |
| SourceFilter | Filters based on the TraceSource that generated the message. |

## Listener [Extensions](Extensions.md)

| Class | Description |
| ----- | ----------- |
| [TraceFormatter](TraceFormatter.md)![EX](images/ex.png) | Inserts trace information into a provided template string. Used to provide the advanced formatting for several listeners. |
| [TraceConfigurationMonitor](TraceConfigurationMonitor.md)![EX](images/ex.png) | Monitors the config file for changes are refreshes trace listeners when required. |

## Tracing [Extensions](Extensions.md)

| Class | Description |
| ----- | ----------- |
| [ActivityScope](ActivityScope.md)![EX](images/ex.png) | Sets the correlation ActivityId for the life of the scope object, performs a transfer, and logs activity messages. |
| [LogicalOperationScope](LogicalOperationScope.md)![EX](images/ex.png) | Sets the correlation LogicalOperation stack for the life of the scope object. |

## [Diagnostics.Abstractions](Abstractions.md)

| Class | Description |
| ----- | ----------- |
| [AssemblyTraceSource<T>](AssemblyTraceSource_T_.md)![EX](images/ex.png) | Enable applications to trace the execution of code and associate trace messages with a source named after the assembly the generic type is from. |
| [ITraceSource](ITraceSource.md)![EX](images/ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with their source.  |
| [ITraceSource<T>](ITraceSource_T_.md)![EX](images/ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with a source related to a specific class. |
| [TraceSourceWrapper](TraceSourceWrapper.md)![EX](images/ex.png) | Provides a wrapper around TraceSource that implements the ITraceSource interface, enable applications to trace the execution of code and associate trace messages with their source in a decoupled manner. |

## Base Classes

| Class | Description |
| ----- | ----------- |
| TraceListener | Base trace listener class provided by the .NET framework. |
| [TraceListenerBase](TraceListenerBase.md)![EX](images/ex.png) | Extended trace listener designed to be subclassed with as little as a single template method override. |
| TraceFilter | Base trace filter class provided by the .NET framework. |

## [Guidance](Guidance.md)

Guidance on considerations when implementing logging and other instrumentation for your project:

* [Logging Levels](Logging-Levels.md)
* [Theory of Event Ids](Event-Ids.md)
* [Correlation](Correlation.md)
* [Integration](Integration.md)
* [Comparison of logging frameworks](Comparison.md)

Guidance related to the Essential.Diagnostics project:

* [Design guidelines for TraceListener extensions](TraceListener-Design-Guidelines.md)
