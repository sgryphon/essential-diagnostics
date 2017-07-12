[Home](../ReadMe.md) | [Index](Index.md) | [Examples](Examples.md) | [Guidance](Guidance.md) | [FAQ](FAQ.md) | [Listeners](Listeners.md) | [Filters](Filters.md) | [Extensions](Extensions.md)

# Comparison of logging frameworks

The following table shows how System.Diagnostics stacks up against some popular 3rd party logging frameworks, as well as the Enterprise Library Logging Application Block extensions from Microsoft, comparing the general features, the information logged, the filters that can be used and the output formats available. 

The System.Diagnostics column indicates both the built in features, as well as the features available in the Essential.Diagnostics and other extensions.

Please contact me if you think any information in this table is out of date.

| General Features | System. Diagnostics | log4net | NLog | Enterprise Library |
| ---------------- | ------------------- | ------- | ---- | ------------------ |
| Availability | built-in | 3rd party | 3rd party | Microsoft |
| Levels | 5 | 5 | 6 | 5 |
| Multiple sources | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| > Hierarchical sources | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Extensible | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Listener chaining | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Delayed formatting | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| > Lambda | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Templates | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Logging interface | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Dynamic configuration | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Minimum trust | ![](images/Comparison_cross.png) | TBA | TBA | ![](images/Comparison_cross.png) |
| Trace .NET framework (WCF, WIF, System.Net, etc) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| Source from .NET Trace | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
|| Log Information || System. Diagnostics || Log4net || NLog || Enterprise Library ||
| Event ID | ![](images/Comparison_tick.png) | contrib extension | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
| Priority | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
| Process/thread information | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| ASP.NET information | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Trace Context | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| > Correlation identifier | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
| > Cross-process correlation | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
| Exceptions | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | via exception block |
|| Filters || System. Diagnostics || Log4net || NLog || Enterprise Library ||
| Event level | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Source | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Property | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| String match | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Expression | ![](images/ex.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Priority | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
|| Listeners || System. Diagnostics || Log4net || NLog || Enterprise Library ||
| > Forward to .NET Trace | ![](images/Comparison_tick.png) | limited | limited | ![](images/Comparison_tick.png) |
| ASP.NET Trace | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Chainsaw (log4j) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Colored Console | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| Console | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Database | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Debug | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| Event Log | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Event Tracing (ETW) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| File | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Mail | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Memory | ![](images/ex.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| MSMQ | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Net Send | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| Remoting | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| Rolling File | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) |
| Syslog (unix) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| Telnet | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) |
| UDP | UdpPocketTrace extension | ![](images/Comparison_tick.png) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) |
| WMI | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |
| XML (Service Trace) | ![](images/Comparison_tick.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_cross.png) | ![](images/Comparison_tick.png) |

Note that for some features the answer is more complex than a simple table, e.g. System.Diagnostics provides some process/thread information in some listeners, with Essential.Diagnostics providing additional information.

In general features are only indicated where they are directly supported by the framework, for example any framework can log exception details as an argument or even just a string, but some have explicit overloads of logging methods with an Exception type parameter.

Similarly, while some explicitly support logging of lambda expressions, any framework with delayed formatting could pass in a wrapper object that evaluates a lambda when ToString() is called; or a framework that supports arbitrary Trace Context can be used as a correlation identifier, compared to those with explicit correlation identifier support.

# Performance comparison

See the solution in the Performance folder of the source code for the test harness used for comparing performance.

A comparison of logging frameworks should also compare the overhead of the different frameworks, i.e. the overhead of adding trace messages and then ignoring or filtering them out (which should be the majority situation). 

All the frameworks allow you to selectively turn on sections of messages to control the volume captured. Whilst the efficiency of capturing the messages you do want may be important, it is the overhead of ignoring the ones you don’t want that is generally the main concern.

The following table shows the results of some performance testing under the following scenarios:
* All messages ignored, i.e. tracing turned off.
* One source turned on at Warning level, capturing 16 messages per million trace statements.
* All sources turned on at Warning level, capturing 245 messages per million trace statements.
* One source turned to full (all messages), with others at Warning level, capturing 3,907 messages per million trace statements.

The source code for the performance testing application is available in the code repository if you would like to run the tests yourself. Note that the absolute values will change depending on the system they are run on.

Base test framework: 56 ms

Logging overhead (ms for 1 million log messages, lower is better):

| Scenario (logged messages) | System. Diagnostics — FileLog | ![EX](images/ex.png) RollingFile | log4net — RollingFile | NLog — File | Enterprise Application Block — RollingFlatFile |
| --- | --- | --- | --- | --- } --- |
| Logging off (0) | 50 | 50 | 46 | 3 | > 20,000 |
| Single filtered (16) | 59 | 54 | 43 | 8 | > 20,000 |
| Multiple filtered (245) | 89 | 64 | 49 | 61 | > 20,000 |
| One full (3,907) | 484 | 238 | 114 | 867 | > 20,000 |

There are also several configurations in the test harness to compare various other scenarios, for example a source with switchValue=”Off” versus no source defined at all.

**Note 1:** Yes, the values for the Enterprise Application Block are what I am getting – around 21-22,000 milliseconds overhead compared to the other frameworks! Now, the EAB does not have delayed formatting, i.e. no overloads that take a format string and arguments, so my test harness does string.Format() for all messages but even removing that it still has 18-19,000 ms overhead.

**Note 2:** With System.Diagnostics to ignore all messages for a source set switchValue="Off" or simply leave out the source altogether. Having a source with no listeners, i.e. using <clear />, has slightly more overhead but isn't too bad.

If you don't clear the list it may look like you have no listeners but instead you will get all messages written to the default listener, which will severely impact the performance of your application. i.e. the worst thing you can do is set the trace switch to allow a lot of messages (e.g. All) without clearing the listeners:

```xml
<!-- Bad example: will severely impact performance (clear listeners, but leave source) -->
<source name="SourceWithBadSwitch" switchValue="All">
</source>
<!-- Correct way to turn logging off (set switch to Off, or simply delete completely) -->
<source name="MySource" switchValue="Off">
  <listeners>
    <clear/>
    <add name="filelog"/>
   </listeners>
</source>
```
