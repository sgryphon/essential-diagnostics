Essential.Diagnostics - Filtering Example
=========================================

An example showing the different retention options of EventSchemaTraceListener.

See http://msdn.microsoft.com/en-us/library/system.diagnostics.tracelogretentionoption.aspx

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the different output in the EventA - EventD log files.

Filter Details
--------------

File1 - Uses .NET Framework EventTypeFilter to only log the Warning and higher
events. It does not log the Information events.

File2 - Uses Essential.Diagnostics.PropertyFilter to log events that have a property
with a specific value (Id = 1001).

File3 - Uses Essential.Diagnostics.ExpressionFilter to log using an C# expression,
calculated from the event properties (e.g. Id >= 8000) and/or environmental
information (e.g. the current user name).
