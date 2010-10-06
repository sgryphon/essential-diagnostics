Essential.Diagnostics - Hello Logging Example
=============================================

Instructions
------------

1. Build the application.
2. You can run the main application, which is configured to use the ConsoleTraceListener.
3. The application EXE is also copied to subdirectories with different configurations (see below).

Configurations
--------------

This example shows configurations both using the built-in .NET Framework trace listeners, as well as extended listeners from the Essential.Diagnostics pack.

Default - .NET Framework ConsoleTraceListener; plain logging to the console.

EventLog - .NET Framework EventLogTraceListener; logs to the Windows Event Log. 
Note: Requires Administrator permissions to create the event log (normally this would be configured during installation).

XmlWriter - .NET Framework XmlWriterTraceListener; logs in an XML format that can be opened in the Service Trace Viewer.

FileLog - .NET Framework FileLogTraceListener (from the VisualBasic namespace); logs to a text file, creating new log files as necessary (e.g. on a schedule, such as daily, or when a maximum size is reached).

ColoredConsole - Essential.Diagnostics extended logger that logs to the console using configurable colors and with a configurable message template.

