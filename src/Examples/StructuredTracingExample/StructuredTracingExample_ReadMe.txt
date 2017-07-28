Essential.Diagnostics - Structured Tracing Example
==================================================

Examples of structured tracing, also known as semantic logging, support in Microsoft .NET System.Diagnostics.

Base support is provided in Essential.Diagnostics.Core, with a StructuredData class that can store
properties, a message template, and template values, and will then render the properties out to
existing trace listeners (see the output in C:\Temp\Logs for example output).

The real power of structured data comes when combined with a trace listener and tool that supports it.
Support for handling structured data is provided by the SeqTraceListener class, which writes to a 
Seq Server (https://getseq.net/).

The project also includes StructuredExtensions, which add extensions methods to TraceSource to make
tracing structured data slightly easier (.NET 3.5+ only).

It also includes an IStructuredTrace interface and generic implementation that has a fluent
interface to directly write structured trace data using a common pattern of one method for each 
trace level. The interface and generic typing makes the trace easy to integrate with a 
dependency injection framework

The producer side (IStructuredTrace) and consumer side (SeqTraceListener) are independent of each
other, and each works with existing System.Diagnostics structures, and work even better when
used together.

Instructions
------------

1. Download and install Seq Server - the personal developer version is sufficient
2. Build the application.
3. Run the application from the command line.
4. Examine the trace output in C:\Temp\Logs.
5. Examine the output in Seq.

