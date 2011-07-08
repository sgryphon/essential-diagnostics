Essential.Diagnostics - Filtering Example
=========================================

An example using of different filters than can be applied to trace listeners.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the different output in the Log1, Log2, Log3 and Log4 directories.

Filter Details
--------------

The trace source is set to log all events of Information level or higher, plus all 
Activity Tracing events. No Verbose events, from that source, will be logged.

Log1 - Uses .NET Framework EventTypeFilter to only log the Information and higher
events. It does not log the Activity Tracing events (Start, Stop)

Log2 - Uses Essential.Diagnostics.PropertyFilter to log events that have a property
with a specific value.

Log3 - Uses Essential.Diagnostics.ExpressionFilter to log using an C# expression,
calculated from the event properties. 

Log4 - Uses Essential.Diagnostics.ExpressionFilter to log using an C# expression,
based on some enviromental information (the current user name).
