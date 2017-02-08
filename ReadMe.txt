Essential.Diagnostics
=====================

Copyright 2010-2017 Sly Gryphon. This library distributed under the 
Microsoft Reciprocal License (Ms-RL).

http://essentialdiagnostics.codeplex.com/

Using and extending System.Diagnostics trace logging. 

This project uses the inbuilt features of the System.Diagnostics 
namespace, and shows how logging and tracing can be integrated into a 
client application whilst taking advantage of the services exposed by 
System.Diagnostics.

The Essential.Diagnostics.dll contains extensions to the .NET Framework 
System.Diagnostics trace listeners, filters, and other utilities.


Trace listeners available via Nuget. These install the listener, as well
as a sample configuration (and the SqlDatabaseTraceListener also installs
a tool for creating the database).

  PM> Install-Package Essential.Diagnostics.BufferedEmailTraceListener

  PM> Install-Package Essential.Diagnostics.ColoredConsoleTraceListener

  PM> Install-Package Essential.Diagnostics.EmailTraceListener

  PM> Install-Package Essential.Diagnostics.InMemoryTraceListener

  PM> Install-Package Essential.Diagnostics.RollingFileTraceListener

  PM> Install-Package Essential.Diagnostics.RollingXmlTraceListener

  PM> Install-Package Essential.Diagnostics.SeqTraceListener

  PM> Install-Package Essential.Diagnostics.SqlDatabaseTraceListener

  
The trace listener packages depend on the Core package, which has a base 
listener class, as well as the expression filter and file configuration 
watcher. Usually it is installed automatically with one of the above 
listeners, but it can also be installed separately if needed.  
  
  PM> Install-Package Essential.Diagnostics.Core

There is also a separate package that has the scope utility classes, 
abstractions, and templated classes for easy use with dependency injection. 
  
  PM> Install-Package Essential.Diagnostics.Fluent
  
  

Version History
---------------

v2.0.208.0 (Feb 2017)

* Only includes Essential.Diagnostics.SeqTraceListener, a trace listener that
  writes to Seq (no other components released).

v2.0.206.0 (Feb 2017)

* Reorganisation of the project into separate packages for each of the
  trace listeners, to make usage via NuGet easier. No actual change to
  the implementation.

v1.2.501.0 (May 2013)

* Feature #16: Added EmailTraceListener, BufferedEmailTraceListener,
  with examples and documentation.
* Add Essential.Diagnostics.Config package that inserts sample .config 
  sections, with a dependency on the main Essential.Diagnostics.
  This is now the preferred way to add a reference (i.e. with the config)
* Add System.Diagnostics Configuration package with sample .config
  sections for Framework listeners.
* Added TraceFormatter parameters: AppDomain, Listener, MessagePrefix.
* Issue #14: Inverted condition in TraceListenerBase.TraceWriteAsEvent
* Issue #21: RollingFileTraceListener should be flushed when it's closed.
* Issue #22: Missing timezone specified "Z" in RollingXmlTraceListener
* Issue #23: HttpContext template items not working correctly

v1.1.20103 (January 2012)

* Feature #4: Add HttpContext items to TraceFormatter parameters: RequestUrl, 
  RequestPath, UserHostAddress, AppData.
* Issue #1: TraceFormatter.cs dependent on System.Windows.Forms.
  We only want the application name part, so use either Assembly.GetEntryAssembly() 
  directly, or for native code use kernel32 GetModuleFileName(), without checking 
  security.
* Issue #2: traceSource.TraceInformation("Information message") throws exception 
  with SqlDatabaseTraceListener. (Issue was in TraceListenerBase and affected all 
  listeners.)
* Issue #12: Allow currently open log file to be shared with another program (for 
  read access only). 

v1.1.10711 (July 2011)

* RollingFileTraceListener, with trace format templates
* RollingXmlTraceListener, rolling files compatible with Service Trace Viewer
* Added TraceFormatter parameters: LocalDateTime, DateTime (preferred name 
  for UtcDateTime), PrincipalName, WindowsIdentityName, Thread (name, if
  available, otherwise id)
* Added new parameters to ExpressionFilter
* Added new SQL parameters to SqlDatabaseTraceListener
* Added Diagnostics.Abstractions library, for better dependency injection support
* Make backwards compatible with .NET 2.0 SP1
* Updated hello logging example for new trace listeners
* Added filter examples

v1.0.1011 (October 2010)

* Release as nuget package

v1.0.1008 (October 2010)

* Initial release
* ColoredConsoleTraceListener, with trace format templates
* SqlDatabaseTraceListener and diagnostics_regsql tool
* InMemoryTraceListener
* PropertyFilter and ExpressionFilter
* ActivityScope and LogicalOperationScope
* TraceConfigurationMonitor
