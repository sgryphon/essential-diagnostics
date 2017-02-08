Essential.Diagnostics.Fluent
============================

Copyright 2017 Sly Gryphon. This library distributed under the 
Microsoft Public License (Ms-PL).

http://essentialdiagnostics.codeplex.com/

Using and extending System.Diagnostics trace logging. 

Templated classes and abstractions to simplify System.Diagnostics. 
Provides easy encapsulation of activity and logical operation scopes, 
as well as simple support for dependency injection frameworks. 

These abstractions write to standard .NET Framework 
Systems.Diagnostics TraceSource, so will easily interoperate with 
existing tracing from framework components (e.g. WCF, Identity Model,
IO, Sockets, Serialization, etc) and trace listeners.

Abstract ITraceSource and ITraceSource<T> that can be used for 
dependency injection:

	public class Foo
	{
		ITraceSource trace;

		public Foo(ITraceSource trace) // or ITrace<Foo>
		{
			this.trace = trace;
		}

		public void DoSomething()
		{
			trace.TraceEvent(TraceEventType.Information, 1001, "Abstraction example");
		}
	}

Templated AssemblyTraceSource that can be created by a dependency 
injection framework, and creates a trace source based on the 
assembly name:

    var foo = new Foo(new AssemblyTraceSource<Foo>());
	foo.DoSomething();

If you don't like the TraceSource interface, then there is also an ITraceLog<T> 
interface, which has utility methods for logging Verbose, Information, Warning,
etc, including overloads that format Exceptions in a standard way, and which 
enforces best practice of using structured event IDs:

    var log = new GenericTraceLog("Example.Program");
    try
    {
        log.Verbose("Simple message");
        throw new ApplicationException("a0");
    }
    catch (Exception ex)
    {
        log.Critical(GenericEventId.AuthenticationCriticalError, ex);
    }

The two can be combined in various ways, either wrapping a templated ITraceSource<T>:

	public class Foo
	{
		ITraceLog<MyEventId> log;

		public Foo(ITraceSource<Foo> trace)
		{
			this.log = new TraceLog<MyEventId>(trace);
		}
    }

Or there is an AssemblyTraceLog<T,U> class which can be used:

   var log = new AssemblyTraceLog<MyEventId, Foo>();

The library also provides easy encapsulation of ActivityId, including 
start, finish, and transfer events:

	var trace = new TraceSource("Example.Program");
	using (var activityScope = new ActivityScope(trace, 0, 2001, 0, 3001))
	{
		trace.TraceEvent(TraceEventType.Information, 6001, "Inside activity scope");
	}

Easy encapsulation of LogicalOperationStack:

	var trace = new TraceSource("Example.Program");
	using (var logicalOperationScope = new LogicalOperationScope(string.Format("Transaction={0}", 1)))
	{
		trace.TraceEvent(TraceEventType.Information, 6002, "Inside logical operation scope.");
	}

This package also includes a starting sample configuration 
for System.Diagnostics that will have been merged into your config.

It has an example configuration for the .NET Framework 
FileLogTraceListener, which writes rotating log files on a daily basis.

The .NET Framework also includes a ConsoleTraceListener and 
XmlWriterTraceListener, or you can use one of the additional 
listeners provided in projects such as Essential.Diagnostics.

Other Essential.Diagnostics packages extend the .NET Framework 
System.Diagnostics trace logging with additional trace listeners. 
Included are colored console (that allows custom formats), 
SQL database (including a tool to create tables), rolling file 
(with custom formats), rolling XML, email (per trace or batched), 
and an in-memory trace listener.

See the project site on CodePlex for examples and documentation.
