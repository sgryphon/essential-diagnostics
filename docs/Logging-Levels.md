# Trace levels

Instrumentation needs to support different levels of operation, from high performance production systems through to developmental debugging.

Usually there are several levels of event type of increasing detail. The structure of the levels can be considered a pyramid, with each level having less importance but a higher volume of events:

![](Logging Levels_Levels-Diagram-Small.png)
_Note: This diagram is an older version and has slightly different levels than described in the text._

There should be few (hopefully no) critical events, maybe a few errors, and hopefully more warnings than errors; information events and activities should be relatively regular occurrences, but not an overwhelming volume, while there would be a large volume of verbose and more detailed tracing – if it was all turned on at once.

Usually you want all Critical, Error, Warning events reported in the trace; Information and Activity Tracing is also important for context, but can add modest volume.

However, turning on all verbose events as a single level would usually result in an overwhelming volume of information, so it is common to partition this level up into separate functions that can be individual turned on or off, e.g. a separate TraceSource (and SourceSwitch) for each area.

For particular low level and high volume detailed information, it may be necessary to have even individual control flags (BooleanSwitch) whether to trace or not, e.g. writing full request/response details to the trace.

Your application may have a different set of levels or in a different order, for example SharePoint has Critical, Warning, Unexpected, Monitorable, Information, High, Medium, Verbose, and VerboseEx, but the general principal of increasing detail and volume of messages at each level still applies.

# Logging vs tracing

A distinction should be made between the concepts of logging and tracing (although sometimes the terminology used is unclear).

Logging provides an audit trail of significant events for your application. An example is services or applications writing to the Windows event log when they start or stop, and when an error (such as an unexpected exception) occurs. This also covers application logs of activity such as the HTTP Logging provided by IIS.

System logging, such as writing to the Windows event log, does not have to be configurable – usually you always want these events to be reported and it doesn’t make sense to be able to turn them off. These events correspond to the Information through Critical levels above.

Application logs usually contain structured data about the activity being logged. The volume of activities is usually more than system events so often application logging is configurable to turn on and off (but with separate configuration than tracing).

As well as logs, system events and activities may also be monitored through Windows Performance Monitor (perfmon) counters.

Tracing, in contrast, is designed to provide lower level information for detailed diagnostics and debugging. Events should be consistent with logging (i.e. tracing should include all Critical, Error, Warning, Information and activity events) but is usually configurable.

Although the higher levels provide context for the trace, the gritty details correspond to the Verbose level, or even finer grained detail controlled by individual boolean switches. (Another option is to simply have additional trace sources for the finer detail.)

# Diagnostics strategy

For each level in your diagnostics model, you should plan the logging, monitoring and tracing strategies you will use. For example, with the levels above:

|| Type || Description || Logging || Monitoring || Tracing ||
| Critical | Events that demand the immediate attention of the system administrator, e.g. an application or system has failed or stopped responding. | Windows event log (Error) |  | TraceSource* |
| Error | Events that indicate problems or errors that should be investigated and fixed, for example unexpected exceptions. | Windows event log (Error) | Errors/sec | TraceSource* |
| Warning | Events that provide forewarning of potential problems or data that can be collected and analysed over time, looking for problem trends. | Windows event log (Warning) | Resource level (where appropriate) | TraceSource* |
| Information | Events that pass noncritical information to the administrator, such as a server start, stop or other significant (but infrequent) event. | Windows event log (Information) |  | TraceSource* |
| Activities | For logging and tracing each operation performed by an application, e.g. each transaction or each message processed. | Application log | Trans./sec, Total trans. | TraceSource with Activity Tracing (start, stop, etc) |
| Verbose | Useful primarily to help developers debug low-level code failures, however should not produce more detail than can be handled. |  |  | TraceSource |
| Detail | Useful for traces that are likely to be high volume, especially information that is not needed for all debugging scenarios. |  |  | BooleanSwitch |

 * Note: Normally a SourceSwitch will either be off (no tracing) or turned on with at least Information level of logging.


# Instrumentation consistency

Handling of events should be consistent between the different mechanisms. Applications should ensure trace events are written at the same time they are written to the Windows event log or application log, and the same time that performance monitor counters are updated.

It can make troubleshooting difficult if a transactions/second performance counter increases but there is no entry in the application log; of if there is an entry in the application log but no start/stop in the trace.

One way to implement this is have an application specific diagnostics component that provides a central location for logging, tracing and monitoring. This can have convenience methods for logging exceptions, errors, warnings, etc that write to the Windows event log, update performance counters, and trace all at one time.

The central component can also provide any custom application logging and convenience methods for simple verbose tracing. This component would be application specific, as each application’s logging needs would be different, although a general template can be followed.

Applications should also use a consistent Windows event log “source”. If the application is a Windows Service, then the service name should be used as the event log source. Otherwise, the application name (as it appears in Windows) should be used. Note that you need to install event log sources (as administrator) before they can be used.

