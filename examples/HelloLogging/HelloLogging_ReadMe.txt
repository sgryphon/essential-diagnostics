Essential.Diagnostics - Hello Logging Example
=============================================

Example configuration files for common trace listeners.

Instructions
------------

1. Build the application.
2. You can run the main application, which is configured to use the ConsoleTraceListener.
3. The application EXE is also copied to subdirectories with different configurations (see below).

Configurations
--------------

This example shows configurations both using the built-in .NET Framework trace listeners, 
as well as extended listeners from the Essential.Diagnostics pack.

Framework listeners
-------------------

Console - .NET Framework ConsoleTraceListener; plain logging to the console.

ETW - System.Core EventProviderTraceListener; acts as a provide to the ETW system so need to use a consumer,
such as logman.exe to connect to the session and write events to an .etl file (binary), then tracerpt.exe to
convert that to a readable format. Note that some important information, such as the EventID, Correlation, 
and Computer are not recorded.

EventLog - .NET Framework EventLogTraceListener; logs to the Windows Event Log. 
Note: Requires Administrator permissions to create the event log (normally this would be configured during installation).

EventSchema - System.Core EventSchemaTraceListener; writes Event schema XML fragments to a text file, with
some good options for logRetentionOption including LimitedCircularFiles. The file can be imported by Service
Trace Viewer but loses some very important information when converting, in particular the actual message.

FileLog - .NET Framework FileLogTraceListener (from the VisualBasic namespace); logs to a text file, creating new 
log files as necessary (e.g. on a schedule, such as daily, or when a maximum size is reached).

XmlWriter - .NET Framework XmlWriterTraceListener; logs E2ETraceEvent XML fragments to a text file that can be opened 
in the Service Trace Viewer.

Essential.Diagnostics listeners
-------------------------------

BufferedEmail - Essential.Diagnostics.BufferedEmailTraceListener that adds formatted trace messages to a buffer and 
sends an email when the process exits, or on request.

ColoredConsole - Essential.Diagnostics.ColoredConsoleTraceListener that logs to the console using configurable colors 
and with a configurable message template.

Email - Essential.Diagnostics.EmailTraceListener that sends a formatted email containing the contents of each trace.

InMemory - Essential.Diagnostics.InMemoryTraceListener

RollingFile - Essential.Diagnostics.RollingFileTraceListener that logs to a text file, creating new files on a schedule
(such as daily), and with a configurable message template.

RollingXml - Essential.Diagnostics.RollingXmlTraceListener that logs E2ETraceEvent XML fragments to a text file that
can be opened in the Service Trace Viewer tool, creating new files on a schedule (such as daily).

SqlDatabase - Essential.Diagnostics.SqlDatabaseTraceListener that logs to a SQL database.
Note: Use the diagnostics_regsql.exe tool to create the required database.

