Essential.Diagnostics - Abstraction Dependency
==============================================

An example using of the interfaces and classes from the Essential.Diagnostics.Abstractions namespace.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. Examine the output.

Remarks
-------

* The Application class has a dependency on ITraceSource<Application> that can be supplied by 
construction injection. Using the generic and specifying the target class allows many dependency
injection frameworks to automatically provide a corresponding instance of AssemblyTraceSource.

* AssemblyTraceSource automatically configures a trace source based on the name of the assembly
that the generic class is from. This provides one trace source per assembly that needs to be
configured and provides a balance between the amount of configuration required (less sources
means less configuration) and the flexibility (more sources provides greater flexibility).

* The first test example shows how the generic interface can be stubbed out during testing with
a specific mock implementation.

* Note however that using the generic interface is not necessary for testing your tracing code.
You can test existing code, without rewriting it to use the interface, by utilising the 
InMemoryTraceListener.

The second test example shows how you can use InMemoryTraceListener in your test configuration,
and then during testing get a reference to the listener. By clearing the listener before
each test, and then checking the contents afterwards, you can apply unit testing to your
trace messages in existing code.
