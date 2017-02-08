Essential.Diagnostics - Scope Example
=====================================

An example how ActivityScope can be used to simplify management of setting the ActivityId.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the trace output.

Note that the ActivityId and RelatedActivityId are set (and reset) by the ActivityScope, and transfer in, 
start, transfer out, and stop events are automatically generated.

The LogicalOperationScope sets and removes an operation identifier from the LogicalOperationStack.

The 'Trace-ScopeExample.svclog' log file should load in the Microsoft Service Trace Viewer
application, and is a good way to see the Activity transfer.
