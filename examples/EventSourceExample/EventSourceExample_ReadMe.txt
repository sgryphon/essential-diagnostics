Essential.Diagnostics - Event Source Example
============================================

An example of how to use System.Diagnostics.Tracing EventSource, introduced in .NET 4.5.

Preparation
-----------

A copy of the PerfView tool, used to view logs, is included in this project,
however if you want you can download the latest version from Microsoft:

https://www.microsoft.com/en-us/download/details.aspx?id=28567


Instructions
------------

1. Build the application.
2. Use PerfView to run the application:

   PS> .\PerfView.exe /OnlyProviders="{67CB0356-4841-4AC7-B192-1D0FBBE089C8}" run EventSourceExample.exe

3. After PerfView appears and has run the application, double click the PerfViewData.etl.zip to open
4. Double click the Events node in the tree
5. Double click on the event types under ExampleEventSource



EventSource Details
-------------------

