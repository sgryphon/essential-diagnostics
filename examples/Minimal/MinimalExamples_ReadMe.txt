Essential.Diagnostics - Minimal Examples
========================================

Equivalent of 'Hello world', showing a simple example of usage.


Getting Started - TraceSource (available since .NET 2.0)
--------------------------------------------------------

Files: Hello.cs, Hello.exe.config

It only takes two lines of code, and a few lines of config (mostly structure),
to get .NET System.Diagnostics TraceSource working:

1. Examine Hello.cs, the two key lines are "var trace = new TraceSource(...", 
   and "trace.TraceInformation(...", as well as the using statement.

2. Examine Hello.exe.config. Most of it is setting up the structure, the key parts
   are defining the source and adding the listener.

3. Compile the file from PowerShell (or other command line):

   PS> & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" Hello.cs

   You may need to vary the command a little bit, depending on where csc.exe is located.

   In some cases you may need to add "/r:System.dll" to reference the system libraries. 

4. Once compiled, run the program:

   PS> .\Hello.exe

5. You should see the output trace message: 

   Hello Information: 0 : Hello World!


Getting Started - EventSource (available since .NET 4.5)
---------------------------------------------------------

To get a minimal EventSource working requires a bit more effort.

You need to subclass EventSource and add explicit methods for different overrides,
and there are no default implementations of EventListener, so you need to add that
as well. (There are some available in Enterprise Library)

Events are sent by default to Event Tracing for Windows (ETW), and it is designed
to support tracing in a separate thread, but if all you want is a simple log file
or output to get started with, then there is a lot of overhead.

1. Examine ETWHello.cs. You don't need TRACE defined, but need to create your
   own EventSource, and if you want to see the output directly you also need
   to implement an EventListener, and then connect them together (in code).

2. Compile the file from PowerShell (or other command line):

   PS> & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ETWHello.cs

3. Once compiled, run the program:

   PS> .\ETWHello.exe

4. You should see the output trace message: 

   1, EventSource(HelloEventSource, a6d80366-00f3-5ad1-7ba9-42bb0a86ba39), 
   Informational, 263882790666240/Info/65533/0, Hello World!

5. To see integration with the ETW framework, use the PerfView tool:

   PS> .\PerfView.exe /OnlyProviders="*HelloEventSource" run ETWHello.exe

   In PerfView, once the run has finished, open the data file, then the 
   Events node, then the HelloEventSource events.


