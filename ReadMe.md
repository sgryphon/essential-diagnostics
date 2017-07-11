![Essential Diagnostics](docs/images/Essential-Diagnostics-64.png)
# Using and extending System.Diagnostics trace logging

[Index](docs/Index.md) | [Examples](docs/Examples.md) | [Guidance](docs/Guidance.md) | [FAQ](docs/FAQ.md) | [Listeners](docs/Listeners.md) | [Filters](docs/Filters.md) | [Extensions](docs/Extensions.md)

**Essential.Diagnostics** contains additional trace listeners, filters and utility classes for the **.NET Framework System.Diagnostics** trace logging. Included are colored console (that allows custom formats), SQL database (including a tool to create tables), formatted rolling file trace listener, rolling XML trace listener, Seq logging server listener, and in-memory trace listeners, simple property and expression filters, activity and logical operation scopes, and configuration file monitoring.

## Installing

### Listeners (various output locations)

Install the Essential.Diagnostics packages for just the trace listeners you need via NuGet. Using these classes requires no change to existing System.Diagnostics tracing code, only config changes (which are included in the packages):

* PM> **Install-Package [Essential.Diagnostics.BufferedEmailTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.BufferedEmailTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.ColoredConsoleTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.ColoredConsoleTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.EmailTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.EmailTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.InMemoryTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.InMemoryTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.RollingFileTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.RollingFileTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.RollingXmlTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.RollingXmlTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.SeqTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.SeqTraceListener)**
* PM> **Install-Package [Essential.Diagnostics.SqlDatabaseTraceListener](http://www.nuget.org/packages/Essential.Diagnostics.SqlDatabaseTraceListener)**

### Core

The trace listener packages depend on the Core package, which has a base listener class, as well as the expression filter and file configuration watcher. Usually it is installed automatically with one of the above listeners, but it can also be installed separately if needed:

* PM> **Install-Package [Essential.Diagnostics.Core](http://www.nuget.org/packages/Essential.Diagnostics.Core)**

### Fluent extensions (application-side, to improve tracing)

There is also a separate package that has the scope utility classes, abstractions, and templated classes for easy use with dependency injection. This package makes using System.Diagnotics trace sources easier, and can be used either separately (with system trace listeners), or in conjunction with the extended trace listeners above.

* PM> **Install-Package [Essential.Diagnostics.Fluent](http://www.nuget.org/packages/Essential.Diagnostics.Fluent)**

### Examples

Source code and examples are available here on GitHub.

## Background

The **[.NET Framework System.Diagnostics](http://msdn.microsoft.com/en-us/library/system.diagnostics.aspx)** provides powerful, flexible, high performance logging for applications -- _and the core capabilities are already built into the .NET Framework_!

This project uses the inbuilt features of the System.Diagnostics namespace, and shows how logging and tracing can be integrated into a client application by taking advantage of existing .NET Framework features.

This project also provides a library that enhances System.Diagnostics through it's numerous built-in extension points, but shouldn't require any changes to existing code (that uses the .NET Framework logging) to use some or all of the features.

Extension features provided by this project are marked ![EX](docs/images/ex.png) -- other features are already provided by the .NET Framework you are using right now.

To see how you can use **System.Diagnostics** and the **Essential.Diagnostics** extensions see [Getting Started](docs/Getting-Started.md) and the [Logging Primer](docs/Logging-Primer.md).
 
## Features

The **.NET Framework System.Diagnostics**, along with the extensions here, provides the following key features, or see a [comparison](docs/Comparison.md) with other logging frameworks.

* Multiple logging sources.
* Output to multiple trace [listeners](docs/Listeners.md) with different [filtering](docs/Filters.md).
* Logical operation context and activity correlation.
* Multiple levels of event types including activity tracing.
* [Integration](docs/Integration.md) with existing .NET Framework tracing (such as WCF).
* Proven architecture.
* Flexible and extensible design.
* High performance.
* No change required to existing .NET Framework trace statements.

The Framework and extensions can be used to write information to any of the following [Listeners](docs/listeners.md):

* A [text file](docs/reference/FileLogTraceListener.md)
* The command [console](docs/Hello-Logging.md) or [colored console](docs/reference/ColoredConsoleTraceListener.md)![EX](docs/images/ex.png)
* The [event log](docs/Windows-Event-Log.md)
* [ASP.NET](docs/reference/WebPageTraceListener.md) tracing
* An [XML](docs/Service-Trace-Viewer.md) file (viewable using the Service Trace Viewer)
* Event Tracing for Windows (Vista [ETW](docs/reference/EventProviderTraceListener.md))
* A [database](docs/reference/SqlDatabaseTraceListener.md)![EX](docs/images/ex.png)
* The [Seq](https://getseq.net/) logging server![EX](docs/images/ex.png)

Log information can be [custom formatted](docs/reference/TraceFormatter.md) and include context information such as:

* Event id, type, and message
* Source - allows you to partition your logs
* Event time and date
* Thread id, process id and call stack
* Correlation, activity ID, and logical operation stack
* Machine name, user name and Windows Identity ![EX](docs/images/ex.png)
