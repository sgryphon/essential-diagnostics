Essential.Diagnostics Examples ReadMe
=====================================

Note: To get the ASP.NET MVC3 project to build, you need to enable
Projects > Enable NuGet Package Restore.


HelloLogging - The main example, with example configurations for most common
listeners from both System.Diagnostics and Essential.Diagnostics extensions.
The output creates a copy of the same base program for each subdirectory,
but with a different logging configuration.

AbstractionDependency - Shows how you can use the Essential.Diagnostics.Abstractions
namespace to stub out diagnostics for testing or for easy injection with an
inversion of control container. It also shows how for unit testing you can also
just using a testing listener (such as InMemoryTraceListener) and don't necessarily
need to use the abstractions.

BufferedEmailExample and EmailExample - Examples showing how the email listeners
work. You can use these either with a mail pickup directory or configure them
to use your network email server.
C:\Code\Diagnostics\EssentialDiagnostics\Examples\HelloLogging\HelloLogging.cs
EventRetentionExamples - Examples of the various logRetentionOption available
for EventSchemaTraceListener.

FilteringExample - Examples of the different filters that can be applied to
trace listeners.

HelloMvc3 - Example using WebPageTraceListener to direct output to trace.axd,
as well as using FileLogTraceListener (.NET Framework) and 
RollingFileTraceListener (Essential.Diagnostics extensions) in a web application.

Note: The MVC example has additional dependencies and is turned off in 
Build > Configuration Manager. You need to turn on Project > Enable NuGet
Package Restore to get the additional dependencies to build this project.

MonitorConfig - Example using TraceConfigurationMonitor to monitor your 
application config file for changes and refresh the diagnostics as needed.

ScopeExample - Example of using ActivityScope (and LogicalOperationscope) for 
simplified management of Trace.CorrelationManager.

TemplateArguments - Example of all the template tokens available in formatable
listeners such as ColoredConsoleTraceListener, RollingFileTraceListener,
and EmailTraceListener.
