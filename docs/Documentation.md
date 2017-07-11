# Documentation

>{[Download](Download) | [FAQ](FAQ) | [Examples](Examples) | [Listeners](Listeners) | [Filters](Filters) | [Extensions](Extensions) | [Abstractions](Abstractions) | [Guidance](Guidance)}>

## [Examples](Examples)

* [Getting Started](Getting-Started)
* [Logging Primer](Logging-Primer)
	* [Hello Logging](Hello-Logging)
	* [Service Trace Viewer](Service-Trace-Viewer)
	* [Windows Event Log](Windows-Event-Log)
	* [Hello Color](Hello-Color)![EX](Documentation_http://i3.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)

## [Listeners](Listeners)

The following trace listeners are provided by the .NET Framework and the Essential.Diagnostics extensions.

| [BufferedEmailTraceListener](BufferedEmailTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace events to an Email message sent at the end of the host process. |
| [ColoredConsoleTraceListener](ColoredConsoleTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes formatted trace events to the console in color based on the type. |
| [ConsoleTraceListener](ConsoleTraceListener) | Writes trace events to the console |
| DefaultTraceListener |  |
| DelimitedListTraceListener | Writes trace events to a file as a delimited list. |
| DiagnosticMonitorTraceListener | Part of Microsoft.WindowsAzure.Diagnostics; writes traces to Azure logs |
| [EmailTraceListener](EmailTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace events to Email messages sent asynchronously. |
| [EventLogTraceListener](EventLogTraceListener) | Writes trace events to the Windows Event Log |
| EventProviderTraceListener |  |
| EventSchemaTraceListener |  |
| [FileLogTraceListener](FileLogTraceListener) | Writes trace events to a file with advanced options for file rotation and output format. |
| FlatFileTraceListener^^1^^ | |
| FormattedEventLogTraceListener^^1^^ | |
| [InMemoryTraceListener](InMemoryTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes traces to an in-memory array. |
| [RollingFileTraceListener](RollingFileTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Trace listener that writes formatted messages to a text file, rolling to a new file based on a filename template (usually including the date). |
| [RollingXmlTraceListener](RollingXmlTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date). |
| [SeqTraceListener](SeqTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace information to a [Seq](https://getseq.net/) logging server. |
| [SqlDatabaseTraceListener](SqlDatabaseTraceListener)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace information to a SQL database. |
| [TextWriterTraceListener](TextWriterTraceListener) | Writes trace events to a simple file. Recommended you at least use [FileLogTraceListener](FileLogTraceListener) instead. |
| WebPageTraceListener | Forwards trace events to the ASP.NET trace output. |
| WMITraceListener^^1^^ | |
| [XmlWriterTraceListener](XmlWriterTraceListener) | Writes events in XML format, suitable for import into the Service Trace Viewer utility. |

Note: ^^1^^ These trace listeners from the Enterprise Library Logging Application Block can also be used directly with System.Diagnostics (for details see [http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx](http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx)).

## [Filters](Filters)

| EventTypeFilter | Filters based on the level of the TraceEventType, e.g. Warning, Error, etc. |
| [ExpressionFilter](ExpressionFilter)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Filter events based on an expression. |
| SourceFilter | Filters based on the TraceSource that generated the message. |

## Listener [Extensions](Extensions)

| [TraceFormatter](TraceFormatter)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Inserts trace information into a provided template string. Used to provide the advanced formatting for several listeners. |
| [TraceConfigurationMonitor](TraceConfigurationMonitor)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Monitors the config file for changes are refreshes trace listeners when required. |

## Tracing [Extensions](Extensions)

| [ActivityScope](ActivityScope)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Sets the correlation ActivityId for the life of the scope object, performs a transfer, and logs activity messages. |
| [LogicalOperationScope](LogicalOperationScope)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Sets the correlation LogicalOperation stack for the life of the scope object. |

## [Diagnostics.Abstractions](Abstractions)

| [AssemblyTraceSource<T>](AssemblyTraceSource_T_)![EX](Documentation_ex.png) | Enable applications to trace the execution of code and associate trace messages with a source named after the assembly the generic type is from. |
| [ITraceSource](ITraceSource)![EX](Documentation_ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with their source.  |
| [ITraceSource<T>](ITraceSource_T_)![EX](Documentation_ex.png) | Defines a set of methods and properties that enable applications to trace the execution of code and associate trace messages with a source related to a specific class. |
| [TraceSourceWrapper](TraceSourceWrapper)![EX](Documentation_ex.png) | Provides a wrapper around TraceSource that implements the ITraceSource interface, enable applications to trace the execution of code and associate trace messages with their source in a decoupled manner. |

## Base Classes

| TraceListener | Base trace listener class provided by the .NET framework. |
| [TraceListenerBase](TraceListenerBase)![EX](Documentation_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Extended trace listener designed to be subclassed with as little as a single template method override. |
| TraceFilter | Base trace filter class provided by the .NET framework. |

## [Guidance](Guidance)

Guidance on considerations when implementing logging and other instrumentation for your project:

* [Logging Levels](Logging-Levels)
* [Theory of Event Ids](Event-Ids)
* [Correlation](Correlation)
* [Integration](Integration)
* [Comparison of logging frameworks](Comparison)

Guidance related to the Essential.Diagnostics project:

* [Design guidelines for TraceListener extensions](TraceListener-Design-Guidelines)
