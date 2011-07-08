Essential.Diagnostics - Filtering Example
=========================================

An example using of different filters than can be applied to trace listeners.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the different output in the File1-...File4- log files.

Filter Details
--------------

File1 - Uses .NET Framework EventTypeFilter to only log the Information and higher
events. It does not log Verbose events.

File2 - Uses Essential.Diagnostics.PropertyFilter to log events that have a property
with a specific value (Id = 0).

File3 - Uses Essential.Diagnostics.ExpressionFilter to log using an C# expression,
calculated from the event properties (Id >= 4000). 

File4 - Uses Essential.Diagnostics.ExpressionFilter to log using an C# expression,
based on some enviromental information (the current user name).
