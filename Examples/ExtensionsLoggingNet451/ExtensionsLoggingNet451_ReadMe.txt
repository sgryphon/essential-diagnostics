Essential.Diagnostics - Microsoft.Extensions.Logging for .NET 4.5.1
===================================================================

In .NET 4.5.1, and .NET Core 1.0, a new logging abstraction was introduced, consisting
of a factory interface and a logger interface.

New code (.NET 4.5.1 and above) should usually use this new interface instead of using 
TraceSource directly.

This example shows how the new abstractions can be integrated with existing TraceListeners,
including those from Essential.Diagnostics.

Instructions
------------

1. Build the application.
2. Run the application from the command line.
3. See the values traced via ILogger are displayed by the ColoredConsoleTraceListener.
4. Note that rather than use config values directly, the extensions create a hierarchy
   of trace sources and inherit settings.

e.g. (1) The listener is only added to the top level ExtensionsLoggingNet451,
        with messages logged to the ExtensionsLoggingNet451.Program 
		and other sources all inheriting that listener.

	(2) The source ExtensionsLoggingNet451.Foo.Bar overrides the switchValue,
		although other settings are still inherited. This means the Information
		level messages written to the ExtensionsLoggingNet451.Foo.Bar logger
		are skipped.

The example also shows how BeginScope functions with TraceSource -- although set on
an individual logger, it actually affects all messages (on the thread).
