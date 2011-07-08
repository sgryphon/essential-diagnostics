Essential.Diagnostics - Scope Example
=====================================

An example using of different filters than can be applied to trace listeners.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the trace output.

Note that the ActivityId and RelatedActivityId are set (and reset) by the ActivityScope, and transfer in, 
start, transfer out, and stop events are automatically generated.

The LogicalOperationScope sets and removes an operation identifier from the LogicalOperationStack.
