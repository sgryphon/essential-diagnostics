# TraceFormatter Class 

Formats trace output using a template, e.g. {""{DateTime:u} [{Thread}]({Thread}) {EventType} {Source} {Id}: {Message}{Data}""}.

## Installing

Install via NuGet (this package is automatically installed when you install one of the trace listeners):

* PM> **Install-Package [Essential.Diagnostics.Core](http://www.nuget.org/packages/Essential.Diagnostics.Core)**

## Remarks

Uses the StringTemplate.Format function to format trace output using a supplied template and trace information. The formatted event can then be written to the console, a file, or other text-based output. 

The following parameters are available in the template string: Data, Data0, EventType, Id, Message, ActivityId, RelatedActivityId, Source, CallStack, UtcDateTime, LogicalOperationStack, ProcessId, ThreadId, Timestamp, MachineName, ProcessName, ThreadName. 

Standard .NET formatting can be applied to these values, e.g. "{UtcDateTime:yyyy-MM-dd}" outputs the year, month and day, or "{EventType,12}" outputs the event type padded to width 12.

An example template that generates the same output as [ColoredConsoleTraceListener](ColoredConsoleTraceListener) is: "{Source} {EventType}: {Id} : {Message}". 

## Available Parameters

|| Parameter || Description ||
| {ActivityId} | Value of the Trace.CorrelationManager.ActivityId Guid; used to correlate traces relating to the same activity. |
| {ApplicationName} | Name of the current executable, without the extension. |
| {Callstack} | Program call stack; this contains multiple lines of output. |
| {Data} | Array of data objects passed to TraceData() methods, converted to a comma separated list of strings. |
| {Data0} | First or single data object passed to TraceData(), converted to a string. |
| {DateTime} | DateTimeOffset of the log event, in the UTC (+0) timezone. |
| {EventType} | TraceEventType, e.g. Verbose, Information, Warning, Error, Critical, or one of the activity tracing events. |
| {Id} | Id of the event. |
| {LocalDateTime} | DateTimeOffset of the log event, in the local timezone. |
| {LogicalOperationStack} | Stack of objects from Trace.CorrelationManager.StartLogicalOperation(), converted to a comma separated list of strings. |
| {MachineName} | Local computer name, from Environment.MachineName. |
| {Message} | Trace message, or format string with the arguments inserted. |
| {PrincipalName} | Value of Thread.CurrentPrincipal.Identity.Name; the current user's identity. |
| {ProcessId} | Id of the current process, from Process.GetCurrentProcess(). |
| {ProcessName} | Name of the current process, from Process.GetCurrentProcess(). |
| {RelatedActivityId} | Guid of the activity being transferred to; provides a link between correlated activities. |
| {Source} | Name of the trace source the event is from. |
| {Thread} | The thread name, or if it is null or empty, the thread id; useful for correlating messages from multiple threads. |
| {ThreadId} | The current thread id, from Thread.CurrentThread. |
| {ThreadName} | The current thread name, from Thread.CurrentThread. |
| {Timestamp} | A numeric timestamp. |
| {WindowsIdentityName} | The current Windows account token of the process, from WindowsIdentity.GetCurrent(). |
