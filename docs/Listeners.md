# Listeners

The following trace listeners are provided by the .NET Framework and the Essential.Diagnostics extensions.

| [BufferedEmailTraceListener](BufferedEmailTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace events to an Email message sent at the end of the host process. |
| [ColoredConsoleTraceListener](ColoredConsoleTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes formatted trace events to the console in color based on the type. |
| [ConsoleTraceListener](ConsoleTraceListener) | Writes trace events to the console |
| DefaultTraceListener |  |
| DelimitedListTraceListener | Writes trace events to a file as a delimited list. |
| DiagnosticMonitorTraceListener | Part of Microsoft.WindowsAzure.Diagnostics; writes traces to Azure logs |
| [EmailTraceListener](EmailTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace events to Email messages sent asynchronously. |
| [EventLogTraceListener](EventLogTraceListener) | Writes trace events to the Windows Event Log |
| EventProviderTraceListener |  |
| EventSchemaTraceListener |  |
| [FileLogTraceListener](FileLogTraceListener) | Writes trace events to a file with advanced options for file rotation and output format. |
| FlatFileTraceListener^^1^^ | |
| FormattedEventLogTraceListener^^1^^ | |
| [InMemoryTraceListener](InMemoryTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes traces to an in-memory array. |
| [RollingFileTraceListener](RollingFileTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Trace listener that writes formatted messages to a text file, rolling to a new file based on a filename template (usually including the date). |
| [RollingXmlTraceListener](RollingXmlTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Trace listener that writes E2ETraceEvent XML fragments to a text file, rolling to a new file based on a filename template (usually including the date). |
| [SeqTraceListener](SeqTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace information to a Seq server. |
| [SqlDatabaseTraceListener](SqlDatabaseTraceListener)![EX](Listeners_http://www.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) | Writes trace information to a SQL database. |
| [TextWriterTraceListener](TextWriterTraceListener) | Writes trace events to a simple file. Recommended you at least use [FileLogTraceListener](FileLogTraceListener) instead. |
| WebPageTraceListener | Forwards trace events to the ASP.NET trace output. |
| WMITraceListener^^1^^ | |
| [XmlWriterTraceListener](XmlWriterTraceListener) | Writes events in XML format, suitable for import into the Service Trace Viewer utility. |

Note: ^^1^^ These trace listeners from the Enterprise Library Logging Application Block can also be used directly with system.diagnostics (for details see [http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx](http://msdn.microsoft.com/en-us/library/ff664735%28v=PandP.50%29.aspx)).
