# ExpressionFilter Class

Trace filter that evaluates a user-supplied boolean expression.

## Installing

Install via NuGet (this package is automatically installed when you install one of the trace listeners):

* PM> **Install-Package [Essential.Diagnostics.Core](http://www.nuget.org/packages/Essential.Diagnostics.Core)**

## Remarks

The initalizeData for the filter contains the C# expression that is evaluated. The expression must return a boolean result and can use any of the parameters: Source, EventType, Id, Format, Callstack, DateTime, LogicalOperationStack, ProcessId, ThreadId,

You can also use an C# expression, including accessing environment details. The expression is compiled in memory, then evaluated to filter each message.

## Config Attributes

|| Attribute || Description ||
| initializeData | Boolean C# expression to evaluate, e.g. " Id == 1001 ". |

## Expression Parameters

The following simple parameter variables may be used in the expression:

|| Parameter || Description ||
| Callstack | Program call stack as a string. |
| DateTime | DateTimeOffset of the log event, in the UTC (+0) timezone. |
| EventType | TraceEventType, e.g. Verbose, Information, Warning, Error, Critical, or one of the activity tracing events. |
| Format | Format string (without arguments) or message. |
| Id | Id of the event. |
| LogicalOperationStack | Stack of objects from Trace.CorrelationManager.StartLogicalOperation(). |
| ProcessId | Id of the current process. |
| Source | Name of the trace source the event is from. |
| ThreadId | The current thread id. |
| Timestamp | A numeric timestamp. |

You can also use C# code and .NET functions in the expression.

## Example Config

{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sharedListeners>
      <add name="coloredconsole"
           type="Essential.Diagnostics.ColoredConsoleTraceListener, Essential.Diagnostics.ColoredConsoleTraceListener">
        <filter type="Essential.Diagnostics.ExpressionFilter, Essential.Diagnostics.Core"
                initializeData=' Id >= 8000 || System.Threading.Thread.CurrentPrincipal.Identity.Name == "User1" ' />
      </add>
    </sharedListeners>
    <sources>
      <source name="ExampleSource" switchValue="All">
        <listeners>
          <clear />
          <add name="coloredconsole" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
{code:xml}

