Essential.Diagnostics Examples ReadMe
=====================================

-------------------------------------------------------------------------------
NOTE:

If you build and get the error "Package restore is disabled by default" it is
because the HelloMvc3 project has additional dependencies. 

To allow HelloMvc3 to build, you can go to Tools > Options, then in Package 
Manager enable "Allow NuGet to download missing packages during build".

Alternatively, to just build all other projects in the solution except 
HelloMvc3, go to Build > Configuration Manager and uncheck HelloMvc3.
-------------------------------------------------------------------------------


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

MonitorConfig - Example using TraceConfigurationMonitor to monitor your 
application config file for changes and refresh the diagnostics as needed.

ScopeExample - Example of using ActivityScope (and LogicalOperationscope) for 
simplified management of Trace.CorrelationManager.

TemplateArguments - Example of all the template tokens available in formatable
listeners such as ColoredConsoleTraceListener, RollingFileTraceListener,
and EmailTraceListener.
