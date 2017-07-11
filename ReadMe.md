# Using and extending System.Diagnostics trace logging
>{[Download](Download) | [FAQ](FAQ) | [Examples](Examples) | [Guidance](Guidance) | [Listeners](Listeners) | [Filters](Filters) | [Extensions](Extensions)}>

**Essential.Diagnostics** contains additional trace listeners, filters and utility classes for the **.NET Framework System.Diagnostics** trace logging. Included are colored console (that allows custom formats), SQL database (including a tool to create tables), formatted rolling file trace listener, rolling XML trace listener, Seq logging server listener, and in-memory trace listeners, simple property and expression filters, activity and logical operation scopes, and configuration file monitoring.

## Installing

Install the Essential.Diagnostics packages for just the trace listeners you need via NuGet. Using these classes requires no change to existing System.Diagnostics tracing code, only config changes (which are included in the packages):

* PM> **Install-Package [Essential.Diagnostics.BufferedEmailTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.BufferedEmailTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.ColoredConsoleTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.ColoredConsoleTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.EmailTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.EmailTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.InMemoryTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.InMemoryTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.RollingFileTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.RollingFileTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.RollingXmlTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.RollingXmlTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.SeqTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.SeqTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.SqlDatabaseTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.SqlDatabaseTraceListener)**

The trace listener packages depend on the Core package, which has a base listener class, as well as the expression filter and file configuration watcher. Usually it is installed automatically with one of the above listeners, but it can also be installed separately if needed:

* PM> **Install-Package [Essential.Diagnostics.Core](http://www.nuget.org/packages/Essential.Diagnostics.Core)**

There is also a separate package that has the scope utility classes, abstractions, and templated classes for easy use with dependency injection. This package makes using System.Diagnotics trace sources easier, and can be used either separately (with system trace listeners), or in conjunction with the extended trace listeners above.

* PM> **Install-Package [Essential.Diagnostics.Fluent](http://www.nuget.org/packages/Essential.Diagnostics.Fluent)**

Source code and examples are available here on CodePlex. (Note: CodePlex download releases have not yet been updated and still contain version 1; for version 2 examples you need to clone the project source code.)

## Background

The **[.NET Framework System.Diagnostics](http___msdn.microsoft.com_en-us_library_system.diagnostics.aspx)** provides powerful, flexible, high performance logging for applications -- _and the core capabilities are already built into the .NET Framework_!

This project uses the inbuilt features of the System.Diagnostics namespace, and shows how logging and tracing can be integrated into a client application by taking advantage of existing .NET Framework features.

This project also provides a library that enhances System.Diagnostics through it's numerous built-in extension points, but shouldn't require any changes to existing code (that uses the .NET Framework logging) to use some or all of the features.

Extension features provided by this project are marked ![EX](Home_http://i3.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104) -- other features are already provided by the .NET Framework you are using right now.

To see how you can use **System.Diagnostics** and the **Essential.Diagnostics** extensions see [Getting Started](Getting-Started) and the [Logging Primer](Logging-Primer).
 
## Features

The **.NET Framework System.Diagnostics**, along with the extensions here, provides the following key features, or see a [comparison](comparison) with other logging frameworks.

* Multiple logging sources.
* Output to multiple trace [listeners](Listeners) with different [filtering](Filters).
* Logical operation context and activity correlation.
* Multiple levels of event types including activity tracing.
* [Integration](Integration) with existing .NET Framework tracing (such as WCF).
* Proven architecture.
* Flexible and extensible design.
* High performance.
* No change required to existing .NET Framework trace statements.

The Framework and extensions can be used to write information to any of the following [Listeners](listeners):

* A [text file](FileLogTraceListener)
* The command [console](Hello-Logging) or [colored console](ColoredConsoleTraceListener)![EX](Home_http://i3.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)
* The [event log](Windows-Event-Log)
* [ASP.NET](WebPageTraceListener) tracing
* An [XML](Service-Trace-Viewer) file (viewable using the Service Trace Viewer)
* Event Tracing for Windows (Vista [ETW](EventProviderTraceListener))
* A [database](SqlDatabaseTraceListener)![EX](Home_http://i3.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)

Log information can be [custom formatted](TraceFormatter) and include context information such as:

* Event id, type, and message
* Source - allows you to partition your logs
* Event time and date
* Thread id, process id and call stack
* Logical operation stack, activity id, and correlation
* Machine name, user name and Windows Identity ![EX](Home_http://i3.codeplex.com/download?ProjectName=essentialdiagnostics&DownloadId=150104)

[About](About)
