[Home](../ReadMe.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Trace-Listeners.md) | [Fluent](Essential-Diagnostics-Fluent.md) | [Core](Essential-Diagnostics-Core.md)

# Index

## [Examples](Examples.md)

* [Getting Started](Getting-Started.md)
* [Logging Primer](Logging-Primer.md)
  * [Hello Logging](Hello-Logging.md)
  * [Service Trace Viewer](Service-Trace-Viewer.md)
  * [Windows Event Log](Windows-Event-Log.md)
  * [Hello Color](Hello-Color.md)![EX](images/ex.png)

## [Trace Listeners](Trace-Listeners.md)

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

## [Essential.Diagnostics.Fluent](Essential-Diagnostics-Fluent.md)

Contains the scope utility classes, abstractions, and templated classes for easy use with dependency injection. This package makes using System.Diagnotics trace sources easier, and can be used either separately (with system trace listeners), or in conjunction with the extended trace listeners above.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |
| [ActivityScope](reference/ActivityScope.md)![EX](images/ex.png) | Sets the correlation ActivityId for the life of the scope object, performs a transfer, and logs activity messages. |
| [LogicalOperationScope](reference/LogicalOperationScope.md)![EX](images/ex.png) | Sets the correlation LogicalOperation stack for the life of the scope object. |

### Essential.Diagnostics.Abstractions namespace

| Class | Description |
| ----- | ----------- |
| AssemblyTraceLog<TEventId, TTarget> | Implementation of the fluent log interface that is bound to a specific EventId type and with a source named after the target class assembly. |
| [AssemblyTraceSource<TTarget>](reference/AssemblyTraceSource_T.md)![EX](images/ex.png) | Enable applications to trace the execution of code and associate trace messages with a source named after the assembly the generic type is from. |
| GenericEventId | General event IDs. |
| GenericTraceLog | Implementation of TraceLog<TEventId> bound to GenericEventId. |
| ITraceLog<TEventId> | Fluent log interface, with strongly typed event IDs. |
| [ITraceSource](reference/ITraceSource.md)![EX](images/ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with their source.  |
| [ITraceSource<TTarget>](reference/ITraceSource_T.md)![EX](images/ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with a source related to a specific class. |
| TraceLog<TEventId> | Generic implementation of the fluent log interface. |
| [TraceSourceWrapper](reference/TraceSourceWrapper.md)![EX](images/ex.png) | Provides a wrapper around TraceSource that implements the ITraceSource interface, enable applications to trace the execution of code and associate trace messages with their source in a decoupled manner. |

## [Essential.Diagnostics.Core](Essential-Diagnostics-Core.md)

Core library with base classes, referenced by other packages.

### Essential.Diagnostics namespace

| Class | Description |
| ----- | ----------- |                                                     s
| [ExpressionFilter](reference/ExpressionFilter.md)![EX](images/ex.png) | Filter events based on an expression. |
| [TraceConfigurationMonitor](reference/TraceConfigurationMonitor.md)![EX](images/ex.png) | Monitors the config file for changes are refreshes trace listeners when required. |
| [TraceFormatter](reference/TraceFormatter.md)![EX](images/ex.png) | Inserts trace information into a provided template string. Used to provide the advanced formatting for several listeners. |
| [TraceListenerBase](reference/TraceListenerBase.md)![EX](images/ex.png) | Extended trace listener designed to be subclassed with as little as a single template method override. |

## .NET Framework classes

From the Microsoft .NET Framework.

### System.Diagnostics namespace

| Class | Description |
| ----- | ----------- |
| EventTypeFilter | Filters based on the level of the TraceEventType, e.g. Warning, Error, etc. |
| TraceFilter | Base trace filter class provided by the .NET framework. |
| TraceListener | Base trace listener class provided by the .NET framework. |

### System Trace Listeners

| Class | Description |
| ----- | ----------- |
| [ConsoleTraceListener](reference/ConsoleTraceListener.md) | Writes trace events to the console |
| DefaultTraceListener |  |
| DelimitedListTraceListener | Writes trace events to a file as a delimited list. |
| DiagnosticMonitorTraceListener | Part of Microsoft.WindowsAzure.Diagnostics; writes traces to Azure logs |
| [EventLogTraceListener](reference/EventLogTraceListener.md) | Writes trace events to the Windows Event Log |
| EventProviderTraceListener |  |
| EventSchemaTraceListener |  |
| [TextWriterTraceListener](reference/TextWriterTraceListener.md) | Writes trace events to a simple file. Recommended you at least use [FileLogTraceListener](reference/FileLogTraceListener.md) instead. |
| WebPageTraceListener | Forwards trace events to the ASP.NET trace output. |
| WMITraceListener^^1^^ | |
| [XmlWriterTraceListener](reference/XmlWriterTraceListener.md) | Writes events in XML format, suitable for import into the Service Trace Viewer utility. |

### Microsoft.VisualBasic.Logging namespace

| Class | Description |
| ----- | ----------- |
| [FileLogTraceListener](reference/FileLogTraceListener.md) | Writes trace events to a file with advanced options for file rotation and output format. |

### Enterprise Library Logging Application Block

| Class | Description |
| ----- | ----------- |
| FlatFileTraceListener | |
| FormattedEventLogTraceListener | |

Note: These trace listeners from the Enterprise Library Logging Application Block can also be used directly with System.Diagnostics (for details see [http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx](http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx)).

## [Guidance](Guidance.md)

Guidance on considerations when implementing logging and other instrumentation for your project:

* [Logging Levels](Logging-Levels.md)
* [Theory of Event Ids](Event-Ids.md)
* [Correlation](Correlation.md)
* [Integration](Integration.md)
* [Comparison of logging frameworks](Comparison.md)

Guidance related to the Essential.Diagnostics project:

* [Design guidelines for TraceListener extensions](TraceListener-Design-Guidelines.md)
