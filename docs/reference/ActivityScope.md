# ActivityScope Class 

Sets the correlation manager ActivityId for the life of the object, resetting it when disposed, and optionally logging activity messages. 

## Installing

Install via NuGet:

* PM> **Install-Package [Essential.Diagnostics.Fluent](http://www.nuget.org/packages/Essential.Diagnostics.Fluent)**

## Remarks

This scope object wraps an activity transfer, setting the Trace.CorrelationManager.ActivityId to a new value and then resetting it when the scope ends.

Optionally it can also generate start and stop traces for the new activity, plus transfer traces to and from the existing activity. Event ID values for the transfer in, start, transfer out and stop can be supplied, or default to 0.

The sequence of events follows the convention used in WCF logging:
* When created, the object logs a Transfer event, changes the ActivityId, and then logs a Start event. 
* When disposed, the object logs a Transfer event (back to the original), a Stop event, and then changes the ActivityId (back to the original). 

## Example

```c#
TraceSource source = new TraceSource("ExampleSource");

source.TraceEvent(TraceEventType.Information, 1, "Message 1");
using (var scope = new ActivityScope(source, 11, 12, 13, 14))
{
    source.TraceEvent(TraceEventType.Warning, 2, "Message 2");
}
source.TraceEvent(TraceEventType.Error, 3, "Message 3");
```

Trace events generated:
* {original activity}, 1, "Message 1"
* {original activity}, 11, "Transfer to {new activity}"
* {new activity}, 12, "Start activity"
* {new activity}, 2, "Message 2"
* {new activity}, 13, "Transfer to {original activity}"
* {new activity}, 14, "Stop activity"
* {original activity}, 3, "Message 3"

