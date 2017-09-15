Essential.Diagnostics - Seq Trace Listener Example
==================================================

Examples using SeqTraceListener to send events to a Seq Server (https://getseq.net/).

The message template and arguments are passed as structured data to the Seq Server, allowing
the individually logged items to be processed and filtered. Note that standard tracing
only supports standard string format messages and args; although the arg types are preserved
and logged separately, they have anonymous names. Args are simply named 0, 1, 2, etc.

However, within a particular logging context, there is only a limited range of values, so
you can still filter on a particular value, e.g. if CustomerID is logged as item {1}, then you
can search and filter on property 1.

Standard properties such as EventId, EventType, and ActivityId are included in all traces,
with any TraceOutputOptions that are configured sent as additional properties. The listener
also supports an expanded additionalProperties attribute to include other key data,
such as MachineName and User.

Instructions
------------

1. Download and install Seq Server - the personal developer version is sufficient
2. Build the application.
3. Run the application from the command line.
4. Examine the output in Seq.
5. Expand trace messages to see the individual data elements that have been traced.
